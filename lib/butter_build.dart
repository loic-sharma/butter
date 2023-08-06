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
  final String outputPath = globals.fs.path.join(getButterBuildDirectory(), buildModeName);
  final Directory outputDir = globals.fs.directory(outputPath);

  globals.logger.printStatus('Flutter root: ${globals.fs.directory(Cache.flutterRoot).path}');

  final Artifacts artifacts = globals.artifacts!;
  final Environment environment = Environment(
    outputDir: outputDir,
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
    BuildMode.release => ReleaseBundleButterAssets(),
    BuildMode.jitRelease => ReleaseBundleButterAssets(),
    BuildMode.profile => ProfileBundleButterAssets(),
    BuildMode.debug => DebugBundleButterAssets(),
  };

  final Status status = globals.logger.startProgress(
    'Building Butter application...',
  );
  try {
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

    // _unpackButterArtifacts(
    //   project,
    //   buildInfo,
    //   targetPlatform,
    //   artifacts,
    //   fs,
    // );

    // await _createWindowsAotBundle(
    //   buildInfo,
    //   targetPlatform,
    //   buildDir,
    //   artifacts,
    //   fs,
    //   globals.processManager,
    //   globals.logger,
    // );

    await _runDotnetBuild(project, buildInfo);
  } finally {
    status.stop();
  }

  // TODO: Share this logic with butter_devices.dart
  final File appFile = outputDir.childFile('Butter.Example.exe');
  if (appFile.existsSync()) {
    globals.logger.printStatus(
      '${globals.logger.terminal.successMark}  '
      'Built ${globals.fs.path.relative(appFile.path)}.',
      color: TerminalColor.green,
    );
  }
}

// See: packages\flutter_tools\lib\src\build_system\targets\windows.dart (WindowsAotBundle)
// See: packages\flutter_tools\lib\src\build_system\targets\common.dart (AotElfBase)
// See: packages\flutter_tools\lib\src\base\build.dart (AOTSnapshotter)
// Future<void> _createWindowsAotBundle(
//   BuildInfo buildInfo,
//   TargetPlatform targetPlatform,
//   Directory buildDir,
//   Artifacts artifacts,
//   FileSystem fs,
//   ProcessManager processManager,
//   Logger logger,
// ) async {
//   return Future<void>.value();

//   final AOTSnapshotter snapshotter = AOTSnapshotter(
//     fileSystem: fs,
//     logger: logger,
//     xcode: globals.xcode!,
//     processManager: processManager,
//     artifacts: artifacts,
//   );
//   final String outputPath = buildDir.path;
//   final List<String> extraGenSnapshotOptions = const <String>[]; // decodeCommaSeparated(environment.defines, kExtraGenSnapshotOptions);
//   final String? splitDebugInfo = null; // environment.defines[kSplitDebugInfo];
//   final bool dartObfuscation = false; //environment.defines[kDartObfuscation] == 'true';

//   final int snapshotExitCode = await snapshotter.build(
//     platform: targetPlatform,
//     buildMode: buildInfo.mode,
//     mainPath: buildDir.childFile('app.dill').path,
//     outputPath: outputPath,
//     extraGenSnapshotOptions: extraGenSnapshotOptions,
//     splitDebugInfo: splitDebugInfo,
//     dartObfuscation: dartObfuscation,
//   );
//   if (snapshotExitCode != 0) {
//     throw Exception('AOT snapshotter exited with code $snapshotExitCode');
//   }
// }

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