import 'package:flutter_tools/src/base/template.dart';
import 'package:flutter_tools/src/flutter_plugins.dart';
import 'package:flutter_tools/src/globals.dart' as globals;
import 'package:flutter_tools/src/plugins.dart';
import 'package:flutter_tools/src/project.dart';

import 'butter_project.dart';

class ButterPlugin {
  static const String kConfigKey = 'butter';
}

// See: injectPlugins
Future<void> injectButterPlugins(
  FlutterProject project, {
  bool? releaseMode,
}) async {
  final List<Plugin> plugins = await findPlugins(project);

  await writeButterPluginFiles(project, plugins, globals.templateRenderer);
}

// See: writeWindowsPluginFiles
Future<void> writeButterPluginFiles(
  FlutterProject project,
  List<Plugin> plugins,
  TemplateRenderer templateRenderer
) async {
  final butterProject = ButterProject.fromFlutter(project);

  _writeGeneratedPluginRegistrant(butterProject);
}

void _writeGeneratedPluginRegistrant(
  ButterProject project,
) {
  final StringBuffer buffer = StringBuffer();
  buffer.write('''
//
// Generated file. Do not edit.
//
namespace Butter;

public class GeneratedPluginRegistrant
{
  public static void RegisterPlugins(Engine engine)
  {
  }
}
''');

  project.generatedPluginRegistrantFile.writeAsStringSync(buffer.toString(), flush: true);
}
