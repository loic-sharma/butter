// Copyright 2023 Sony Group Corporation. All rights reserved.
// Copyright 2020 Samsung Electronics Co., Ltd. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

import 'package:flutter_tools/src/android/build_validation.dart' as android;
import 'package:flutter_tools/src/base/analyze_size.dart';
import 'package:flutter_tools/src/base/common.dart';
import 'package:flutter_tools/src/base/file_system.dart';
import 'package:flutter_tools/src/base/logger.dart';
import 'package:flutter_tools/src/base/os.dart';
import 'package:flutter_tools/src/base/terminal.dart';
import 'package:flutter_tools/src/build_info.dart';
import 'package:flutter_tools/src/commands/build.dart';
import 'package:flutter_tools/src/artifacts.dart';
import 'package:flutter_tools/src/globals.dart' as globals;
import 'package:flutter_tools/src/project.dart';
import 'package:flutter_tools/src/runner/flutter_command.dart';

import '../butter_project.dart';

class ButterBuildCommand extends BuildCommand {
  ButterBuildCommand({
    required super.fileSystem,
    required super.buildSystem,
    required super.osUtils,
    required bool verboseHelp,
    required super.androidSdk,
    required super.logger,
  }) : super(verboseHelp: verboseHelp) {
    addSubcommand(BuildButterCommand(verboseHelp: verboseHelp));
  }
}

class BuildButterCommand extends BuildSubCommand {
  /// See: [BuildApkCommand] in `build_apk.dart`
  BuildButterCommand({bool verboseHelp = false})
      : super(
          verboseHelp: verboseHelp,
          logger: globals.logger,
        ) {
    addCommonDesktopBuildOptions(verboseHelp: verboseHelp);
  }

  @override
  final String name = 'butter';

  @override
  Future<Set<DevelopmentArtifact>> get requiredArtifacts async =>
    <DevelopmentArtifact>{
      DevelopmentArtifact.windows
    };

  @override
  final String description = 'Build a Butter Windows desktop application.';

  @override
  Future<FlutterCommandResult> runCommand() async {
    final FlutterProject flutterProject = FlutterProject.current();
    final BuildInfo buildInfo = await getBuildInfo();
    final TargetPlatform targetPlatform = TargetPlatform.windows_x64;
    if (!globals.platform.isWindows) {
      throwToolExit('"build butter" only supported on Windows hosts.');
    }
    displayNullSafetyMode(buildInfo);
    await buildButter(
      ButterProject.fromFlutter(flutterProject),
      buildInfo,
      targetPlatform,
    );
    return FlutterCommandResult.success();
  }
}

Future<void> buildButter(
  ButterProject project,
  BuildInfo buildInfo,
  TargetPlatform targetPlatform,
) async {
  final Artifacts? artifacts = globals.artifacts;
  final FileSystem fs = globals.fs;

  if (artifacts == null) { throw 'Null artifacts'; }

  final Status status = globals.logger.startProgress(
    'Building Butter application...',
  );
  try {
    _unpackButterArtifacts(
      project,
      buildInfo,
      targetPlatform,
      artifacts,
      fs,
    );

    await _createWindowsAotBundle();

    await _runDotnetBuild(project, buildInfo);
  } finally {
    status.stop();
  }

  // TODO: Share this logic with butter_devices.dart
  final File appFile = project.runnerDirectory
    .childDirectory('bin')
    .childDirectory(buildInfo.mode == BuildMode.debug ? 'Debug' : 'Release')
    .childDirectory('net6.0-windows7.0')
    .childFile('Butter.Example.exe');
  if (appFile.existsSync()) {
    globals.logger.printStatus(
      '${globals.logger.terminal.successMark}  '
      'Built ${globals.fs.path.relative(appFile.path)}.',
      color: TerminalColor.green,
    );
  }
}

// See: packages\flutter_tools\lib\src\build_system\targets\windows.dart
// See: packages\flutter_tools\lib\src\build_system\targets\desktop.dart
const List<String> _kWindowsArtifacts = <String>[
  'flutter_windows.dll',
  'flutter_windows.dll.pdb',
];
void _unpackButterArtifacts(
  ButterProject project,
  BuildInfo buildInfo,
  TargetPlatform targetPlatform,
  Artifacts artifacts,
  FileSystem fs,
) {
  final Directory ephemeralDirectory = project.ephemeralDirectory;
  final String artifactsPath = artifacts.getArtifactPath(
    Artifact.windowsDesktopPath,
    platform: targetPlatform,
    mode: buildInfo.mode,
  );

  // Copy Windows artifacts.
  for (final String artifact in _kWindowsArtifacts) {
    final String artifactPath = fs.path.join(
      artifactsPath,
      artifact,
    );
    final FileSystemEntityType artifactType = fs.typeSync(artifactPath);
    assert(artifactType == FileSystemEntityType.file);
    final String outputPath = fs.path.join(
      ephemeralDirectory.path,
      fs.path.relative(artifactPath, from: artifactsPath),
    );
    final File artifactFile = fs.file(artifactPath);
    final File destinationFile = fs.file(outputPath);
    if (!destinationFile.parent.existsSync()) {
      destinationFile.parent.createSync(recursive: true);
    }
    artifactFile.copySync(destinationFile.path);
  }

  // Copy ICU data
  final String icuDataPath = artifacts.getArtifactPath(
    Artifact.icuData,
    platform: targetPlatform,
  );
  final File icuDataFile = fs.file(icuDataPath);
  final String icuDataDestinationPath = fs.path.join(
    ephemeralDirectory.path,
    icuDataFile.basename,
  );
  final File icuDataDestinationFile = fs.file(icuDataDestinationPath);
  icuDataFile.copySync(icuDataDestinationFile.path);
}

// See: packages\flutter_tools\lib\src\build_system\targets\windows.dart (WindowsAotBundle)
// See: packages\flutter_tools\lib\src\build_system\targets\common.dart (AotElfBase)
// See: packages\flutter_tools\lib\src\base\build.dart (AOTSnapshotter)
Future<void> _createWindowsAotBundle() async {
}

Future<void> _runDotnetBuild(ButterProject project, BuildInfo buildInfo) async {
  int result;
  try {
    result = await globals.processUtils.stream(
      <String>[
        'dotnet',
        'build',
        '-c',
        buildInfo.mode == BuildMode.debug ? 'Debug' : 'Release',
      ],
      workingDirectory: project.runnerDirectory.path,
      trace: true,
    );
  } on ArgumentError {
    throwToolExit("dotnet not found. Run 'flutter doctor' for more information.");
  }
  if (result != 0) {
    throwToolExit('.NET build failed');
  }
}
