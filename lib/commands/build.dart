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

  /// See: [android.validateBuild] in `build_validation.dart`
  // void validateBuild(ELinuxBuildInfo eLinuxBuildInfo) {
  //   if (eLinuxBuildInfo.buildInfo.mode.isPrecompiled &&
  //       eLinuxBuildInfo.targetArch == 'x86') {
  //     throwToolExit('x86 ABI does not support AOT compilation.');
  //   }
  // }

  /// See: [BuildApkCommand.runCommand] in `build_apk.dart`
  @override
  Future<FlutterCommandResult> runCommand() async {
    final FlutterProject flutterProject = FlutterProject.current();
    final BuildInfo buildInfo = await getBuildInfo();
    if (!globals.platform.isWindows) {
      throwToolExit('"build butter" only supported on Windows hosts.');
    }
    displayNullSafetyMode(buildInfo);
    await _buildButter(
      ButterProject.fromFlutter(flutterProject),
      buildInfo,
    );
    return FlutterCommandResult.success();
  }
}

Future<void> _buildButter(ButterProject project, BuildInfo buildInfo) async {
  final Artifacts? artifacts = globals.artifacts;
  final FileSystem fs = globals.fs;

  final TargetPlatform targetPlatform = TargetPlatform.windows_x64;

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
  } finally {
    status.stop();
  }

  await Future<void>.value();
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