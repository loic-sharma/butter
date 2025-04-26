import 'dart:io';

import 'package:file/file.dart';
import 'package:flutter_tools/src/base/error_handling_io.dart';
import 'package:flutter_tools/src/base/template.dart';
import 'package:flutter_tools/src/dart/package_map.dart';
import 'package:flutter_tools/src/flutter_plugins.dart';
import 'package:flutter_tools/src/globals.dart' as globals;
import 'package:flutter_tools/src/platform_plugins.dart';
import 'package:flutter_tools/src/project.dart';

import 'package:package_config/package_config.dart';
import 'package:yaml/yaml.dart';

import 'butter_project.dart';

class ButterPlugin extends PluginPlatform implements NativeOrDartPlugin {
  ButterPlugin({
    required this.name,
    required this.directory,
    this.pluginClass,
    this.dartPluginClass,
    this.ffiPlugin,
    this.defaultPackage,
    this.dependencies,
  }) : assert(pluginClass != null ||
            dartPluginClass != null ||
            (ffiPlugin ?? false) ||
            defaultPackage != null);

  factory ButterPlugin.fromYaml(String name, Directory directory, YamlMap yaml,
      List<String> dependencies) {
    assert(validate(yaml));
    // Treat 'none' as not present. See https://github.com/flutter/flutter/issues/57497.
    String? pluginClass = yaml[kPluginClass] as String?;
    if (pluginClass == 'none') {
      pluginClass = null;
    }
    return ButterPlugin(
        name: name,
        directory: directory,
        pluginClass: yaml[kPluginClass] as String?,
        dartPluginClass: yaml[kDartPluginClass] as String?,
        ffiPlugin: yaml[kFfiPlugin] as bool?,
        defaultPackage: yaml[kDefaultPackage] as String?,
        dependencies: dependencies);
  }

  static const String kConfigKey = 'butter';

  static bool validate(YamlMap yaml) {
    return yaml[kPluginClass] is String ||
        yaml[kDartPluginClass] is String ||
        yaml[kFfiPlugin] == true ||
        yaml[kDefaultPackage] is String;
  }

  final String name;
  final Directory directory;
  final String? pluginClass;
  final String? dartPluginClass;
  final List<String>? dependencies;
  final bool? ffiPlugin;
  final String? defaultPackage;

  @override
  bool hasMethodChannel() => pluginClass != null;

  @override
  bool hasFfi() => ffiPlugin != null;

  @override
  bool hasDart() => dartPluginClass != null;

  @override
  Map<String, dynamic> toMap() {
    return <String, dynamic>{
      'name': name,
      if (pluginClass != null) 'class': pluginClass,
      if (pluginClass != null) 'filename': pluginClass, // TODO: Check this. Flutter uses a regex here.
      if (dartPluginClass != null) 'dartPluginClass': dartPluginClass,
      if (ffiPlugin != null && ffiPlugin!) kFfiPlugin: true,
      if (defaultPackage != null) kDefaultPackage: defaultPackage,
    };
  }

  String get path => directory.parent.path;

  /// Method to return a detailed debug string for the plugin.
  @override
  String toString() {
    return '''
ButterPlugin:
  name: $name
  directory: ${directory.path}
  pluginClass: $pluginClass
  dartPluginClass: $dartPluginClass
  ffiPlugin: $ffiPlugin
  defaultPackage: $defaultPackage
  dependencies: ${dependencies?.join(', ')}
''';
  }
}

// See: injectPlugins
Future<void> injectButterPlugins(
  FlutterProject project, {
  bool? releaseMode,
}) async {
  final List<ButterPlugin> plugins = await findButterPlugins(project);

  await writeButterPluginFiles(project, plugins, globals.templateRenderer);
}

Future<List<ButterPlugin>> findButterPlugins(
  FlutterProject project, {
  bool dartOnly = false,
  bool nativeOnly = false,
  bool throwOnError = true,
}) async {
  final List<ButterPlugin> plugins = <ButterPlugin>[];
  final File packagesFile = project.directory.childFile('.packages');
  final PackageConfig packageConfig = await loadPackageConfigWithLogging(
    packagesFile,
    logger: globals.logger,
    throwOnError: throwOnError,
  );
  for (final Package package in packageConfig.packages) {
    final Uri packageRoot = package.packageUriRoot.resolve('..');
    final ButterPlugin? plugin = _pluginFromPackage(package.name, packageRoot);
    if (plugin == null) {
      continue;
    }

    final bool isFfi = plugin.ffiPlugin ?? false;

    if (nativeOnly &&
        ((plugin.pluginClass == null || plugin.pluginClass == 'none') &&
            !isFfi)) {
      continue;
    }

    if (dartOnly && (plugin.dartPluginClass == null || isFfi)) {
      continue;
    }

    plugins.add(plugin);
  }
  return plugins;
}

/// Source: [_pluginFromPackage] in `plugins.dart`
ButterPlugin? _pluginFromPackage(String name, Uri packageRoot) {
  final String pubspecPath =
      globals.fs.path.fromUri(packageRoot.resolve('pubspec.yaml'));
  if (!globals.fs.isFileSync(pubspecPath)) {
    return null;
  }

  dynamic pubspec;
  try {
    pubspec = loadYaml(globals.fs.file(pubspecPath).readAsStringSync());
  } on YamlException catch (err) {
    globals.printTrace('Failed to parse plugin manifest for $name: $err');
  }
  if (pubspec == null) {
    return null;
  }
  final dynamic flutterConfig = pubspec['flutter'];
  if (flutterConfig == null || !(flutterConfig.containsKey('plugin') as bool)) {
    return null;
  }

  final Directory packageDir = globals.fs.directory(packageRoot);
  globals.printTrace('Found plugin $name at ${packageDir.path}');

  final YamlMap pluginYaml = flutterConfig['plugin'] as YamlMap;
  if (pluginYaml['platforms'] == null) {
    return null;
  }
  final YamlMap platformsYaml = pluginYaml['platforms'] as YamlMap;
  if (platformsYaml[ButterPlugin.kConfigKey] == null) {
    return null;
  }
  final YamlMap dependencies = pubspec['dependencies'] as YamlMap;
  return ButterPlugin.fromYaml(
    name,
    packageDir.childDirectory('butter'),
    platformsYaml[ButterPlugin.kConfigKey] as YamlMap,
    <String>[...dependencies.keys.cast<String>()],
  );
}

// See: writeWindowsPluginFiles
Future<void> writeButterPluginFiles(
  FlutterProject project,
  List<ButterPlugin> plugins,
  TemplateRenderer templateRenderer
) async {
  final ButterProject butterProject = ButterProject.fromFlutter(project);

  await _writeGeneratedPluginRegistrant(butterProject, plugins);
  _createPluginSymlinks(butterProject, plugins, force: true);
}

Future<void> _writeGeneratedPluginRegistrant(
  ButterProject project,
  List<ButterPlugin> plugins,
) async {
  final List<ButterPlugin> methodChannelPlugins =
      _filterMethodChannelPlugins(plugins);
  final List<ButterPlugin> ffiPlugins = _filterFfiPlugins(plugins)
    ..removeWhere(methodChannelPlugins.contains);

  final List<Map<String, dynamic>> methodChannelPluginsMap =
      methodChannelPlugins
          .map((ButterPlugin plugin) => plugin.toMap())
          .toList();
  final List<Map<String, dynamic>> ffiPluginsMap =
      ffiPlugins.map((ButterPlugin plugin) => plugin.toMap()).toList();

  final Map<String, dynamic> templateContext = <String, dynamic>{
    'methodChannelPlugins': methodChannelPluginsMap,
    'ffiPlugins': ffiPluginsMap,
    'pluginsDir': project.pluginSymlinkDirectory.path,
  };

  const String pluginRegistrantTemplate = r'''
//
// Generated file. Do not edit.
//

namespace Butter;

public class GeneratedPluginRegistrant
{
    public static void RegisterPlugins(Engine engine)
    {
{{#methodChannelPlugins}}
        {{class}}.{{class}}.RegisterWithRegistrar(
            engine.GetRegistrarForPlugin("{{name}}"));
{{/methodChannelPlugins}}
    }
}
''';

  await _renderTemplateToFile(
    pluginRegistrantTemplate,
    templateContext,
    project.generatedPluginRegistrantFile,
    globals.templateRenderer,
  );

  const String pluginRegistrantProjTemplate = r'''
<Project>

  <ItemGroup>
{{#methodChannelPlugins}}
    <ProjectReference Include="{{pluginsDir}}/{{name}}//butter/{{name}}.csproj" />
{{/methodChannelPlugins}}
  </ItemGroup>

</Project>
''';

  await _renderTemplateToFile(
    pluginRegistrantProjTemplate,
    templateContext,
    project.generatedPluginRegistrantProjFile,
    globals.templateRenderer,
  );
}

/// Filters out any plugins that don't use method channels, and thus shouldn't be added to the native generated registrants.
List<ButterPlugin> _filterMethodChannelPlugins(List<ButterPlugin> plugins) {
  return plugins.where((ButterPlugin plugin) {
    return (plugin as NativeOrDartPlugin).hasMethodChannel();
  }).toList();
}

/// Filters out Dart-only and method channel plugins.
///
/// FFI plugins do not need native code registration, but their binaries need to be bundled.
List<ButterPlugin> _filterFfiPlugins(List<ButterPlugin> plugins) {
  return plugins.where((ButterPlugin plugin) {
    final NativeOrDartPlugin plugin_ = plugin as NativeOrDartPlugin;
    return plugin_.hasFfi();
  }).toList();
}

Future<void> _renderTemplateToFile(
  String template,
  Object? context,
  File file,
  TemplateRenderer templateRenderer,
) async {
  final String renderedTemplate = templateRenderer.renderString(template, context);
  await file.create(recursive: true);
  await file.writeAsString(renderedTemplate);
}

/// Creates [symlinkDirectory] containing symlinks to each plugin listed in [platformPlugins].
///
/// If [force] is true, the directory will be created only if missing.
void _createPluginSymlinks(
  ButterProject project,
  List<ButterPlugin> plugins, {
  bool force = false,
}) {
  final Directory symlinkDirectory = project.pluginSymlinkDirectory;
  if (force) {
    // Start fresh to avoid stale links.
    ErrorHandlingFileSystem.deleteIfExists(symlinkDirectory, recursive: true);
  }
  symlinkDirectory.createSync(recursive: true);
  if (plugins.isEmpty) {
    return;
  }

  for (final ButterPlugin plugin in plugins) {
    final Link link = symlinkDirectory.childLink(plugin.name);
    if (link.existsSync()) {
      continue;
    }
    try {
      link.createSync(plugin.path);
    } on FileSystemException catch (e) {
      // ignore: invalid_use_of_visible_for_testing_member
      handleSymlinkException(
        e,
        platform: globals.platform,
        os: globals.os,
        destination: link.path,
        source: plugin.path,
      );
      rethrow;
    }
  }
}
