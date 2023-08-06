// Copyright 2023 Sony Group Corporation. All rights reserved.
// Copyright 2020 Samsung Electronics Co., Ltd. All rights reserved.
// Copyright 2014 The Flutter Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

import 'package:flutter_tools/src/base/file_system.dart';
import 'package:flutter_tools/src/commands/create.dart';
import 'package:flutter_tools/src/globals.dart' as globals;
import 'package:flutter_tools/src/runner/flutter_command.dart';
import 'package:flutter_tools/src/template.dart';

const List<String> _kAvailablePlatforms = <String>[
  'butter',
  'ios',
  'android',
  'windows',
  'linux',
  'macos',
  'web',
];

class ButterCreateCommand extends CreateCommand {
  ButterCreateCommand({super.verboseHelp});

  @override
  void addPlatformsOptions({String? customHelp}) {
    argParser.addMultiOption(
      'platforms',
      help: customHelp,
      defaultsTo: _kAvailablePlatforms,
      allowed: _kAvailablePlatforms,
    );
  }

  @override
  Future<int> renderTemplate(
    String templateName,
    Directory directory,
    Map<String, Object?> context, {
    bool overwrite = false,
    bool printStatusWhenWriting = true,
  }) async {
    // Disables https://github.com/flutter/flutter/pull/59706 by setting
    // templateManifest to null.
    final Template template = await Template.fromName(
      templateName,
      fileSystem: globals.fs,
      logger: globals.logger,
      templateRenderer: globals.templateRenderer,
      templateManifest: null,
    );
    return template.render(directory, context, overwriteExisting: overwrite);
  }

  @override
  Future<int> renderMerged(
    List<String> names,
    Directory directory,
    Map<String, Object?> context, {
    bool overwrite = false,
    bool printStatusWhenWriting = true,
  }) async {
    // Disables https://github.com/flutter/flutter/pull/59706 by setting
    // templateManifest to null.
    final Template template = await Template.merged(
      names,
      directory,
      fileSystem: globals.fs,
      logger: globals.logger,
      templateRenderer: globals.templateRenderer,
      templateManifest: <Uri>{},
    );
    return template.render(directory, context, overwriteExisting: overwrite);
  }

  /// See:
  /// - [CreateCommand._generatePlugin] in `create.dart`
  /// - [Template.render] in `template.dart`
  @override
  Future<FlutterCommandResult> runCommand() async {
    // TODO: Add Butter's template.
    return super.runCommand();
  }
}
