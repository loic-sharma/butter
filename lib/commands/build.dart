// Copyright 2023 Sony Group Corporation. All rights reserved.
// Copyright 2020 Samsung Electronics Co., Ltd. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

import 'package:flutter_tools/src/base/common.dart';
import 'package:flutter_tools/src/build_info.dart';
import 'package:flutter_tools/src/commands/build.dart';
import 'package:flutter_tools/src/globals.dart' as globals;
import 'package:flutter_tools/src/project.dart';
import 'package:flutter_tools/src/runner/flutter_command.dart';

import '../butter_build.dart';
import '../butter_project.dart';

class ButterBuildCommand extends BuildCommand {
  ButterBuildCommand({
    required super.artifacts,
    required super.fileSystem,
    required super.buildSystem,
    required super.osUtils,
    required bool verboseHelp,
    required super.androidSdk,
    required super.processUtils,
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
    const  TargetPlatform targetPlatform = TargetPlatform.windows_x64;
    if (!globals.platform.isWindows) {
      throwToolExit('"build butter" only supported on Windows hosts.');
    }
    displayNullSafetyMode(buildInfo);
    await buildButter(
      ButterProject.fromFlutter(flutterProject),
      buildInfo,
      targetPlatform,
      targetFile,
    );
    return FlutterCommandResult.success();
  }
}
