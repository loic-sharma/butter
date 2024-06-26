import 'package:flutter_tools/src/artifacts.dart';
import 'package:flutter_tools/src/base/common.dart';
import 'package:flutter_tools/src/base/file_system.dart';
import 'package:flutter_tools/src/base/logger.dart';
import 'package:flutter_tools/src/base/terminal.dart';
import 'package:flutter_tools/src/build_info.dart';
import 'package:flutter_tools/src/build_system/build_system.dart';
import 'package:flutter_tools/src/cache.dart';
import 'package:flutter_tools/src/globals.dart' as globals;
import 'package:path/path.dart' as path;

import 'butter_project.dart';
import 'butter_targets.dart';

String getButterBuildDirectory() {
  return globals.fs.path.join(getBuildDirectory(), 'butter');
}

Future<void> buildButter(
  ButterProject project,
  BuildInfo buildInfo,
  TargetPlatform targetPlatform,
  String targetFile,
) async {
  final String buildModeName = switch (buildInfo.mode) {
    BuildMode.debug => 'Debug',
    BuildMode.profile => 'Profile',
    BuildMode.release => 'Release',
    BuildMode.jitRelease => 'Release',
  };
  final Artifacts artifacts = globals.artifacts!;
  final Environment environment = Environment(
    outputDir: project.ephemeralDirectory,
    buildDir: project.parent.directory
      .childDirectory('.dart_tool')
      .childDirectory('flutter_build'),
    projectDir: project.parent.directory,
    defines: <String, String>{
      kTargetFile: targetFile,
      kBuildMode: buildModeName,
      kTargetPlatform: getNameForTargetPlatform(targetPlatform),
      ...buildInfo.toBuildSystemEnvironment(),
    },
    cacheDir: globals.cache.getRoot(),
    flutterRootDir: globals.fs.directory(path.join(rootPath, 'third_party', 'flutter')),
    artifacts: artifacts,
    fileSystem: globals.fs,
    logger: globals.logger,
    processManager: globals.processManager,
    usage: globals.flutterUsage,
    platform: globals.platform,
    engineVersion: artifacts.isLocalEngine
      ? null
      : globals.flutterVersion.engineRevision,
    generateDartPluginRegistry: true,
  );

  final Target target = switch (buildInfo.mode) {
    BuildMode.release => const ReleaseBundleButterAssets(),
    BuildMode.jitRelease => const ReleaseBundleButterAssets(),
    BuildMode.profile => const ProfileBundleButterAssets(),
    BuildMode.debug => const DebugBundleButterAssets(),
  };

  final Status status = globals.logger.startProgress(
    'Building Butter application...',
  );
  try {
    _writeGeneratedConfig(project, buildInfo);

    final BuildResult result = await globals.buildSystem.build(
      target,
      environment,
    );
    if (!result.success) {
      for (final ExceptionMeasurement measurement in result.exceptions.values) {
        globals.printError(measurement.exception.toString());
      }
      throwToolExit('The Butter build failed.');
    }

    await _runDotnetBuild(project, buildInfo);
  } finally {
    status.stop();
  }

  // TODO: Share this logic with butter_devices.dart
  // TODO: Use pubspec yaml to determine app name.
  final String buildPath = globals.fs.path.join(getButterBuildDirectory(), buildModeName);
  final Directory buildDir = globals.fs.directory(buildPath);
  final File appFile = buildDir.childFile('${project.parent.manifest.appName}.exe');
  if (appFile.existsSync()) {
    globals.logger.printStatus(
      '${globals.logger.terminal.successMark}  '
      'Built ${globals.fs.path.relative(appFile.path)}.',
      color: TerminalColor.green,
    );
  }
}

void _writeGeneratedConfig(ButterProject project, BuildInfo buildInfo) {
  final StringBuffer buffer = StringBuffer('''
<!-- Generated code. Do not modify -->
<Project>

  <PropertyGroup>
    <FlutterAppName>${project.parent.manifest.appName}</FlutterAppName>
    <FlutterAppVersion>1.2.3</FlutterAppVersion>
  </PropertyGroup>

</Project>
''');

  project.generatedConfigPropsFile
    ..createSync(recursive: true)
    ..writeAsStringSync(buffer.toString());
}

Future<void> _runDotnetBuild(ButterProject project, BuildInfo buildInfo) async {
  int result;
  try {
    final String buildMode = buildInfo.mode == BuildMode.debug ? 'Debug' : 'Release';
    result = await globals.processUtils.stream(
      <String>[
        'dotnet',
        'build',
        project.runnerDirectory.path,
        '-c',
        buildMode,
        '-o',
        globals.fs.path.join(getButterBuildDirectory(), buildMode),
      ],
      trace: true,
    );
  } on ArgumentError {
    throwToolExit("dotnet not found. Run 'flutter doctor' for more information.");
  }
  if (result != 0) {
    throwToolExit('.NET build failed');
  }
}

/// See: [Cache.defaultFlutterRoot] in `cache.dart`
String get rootPath {
  final String scriptPath = globals.platform.script.toFilePath();
  return path.normalize(path.join(
    scriptPath,
    scriptPath.endsWith('.snapshot') ? '../../..' : '../..',
  ));
}
