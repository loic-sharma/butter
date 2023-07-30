import 'package:flutter_tools/src/base/file_system.dart';
import 'package:flutter_tools/src/project.dart';

import 'butter_plugins.dart';

/// The Butter sub project.
class ButterProject extends FlutterProjectPlatform {
  ButterProject.fromFlutter(this.parent);

  @override
  final FlutterProject parent;

  @override
  String get pluginConfigKey => ButterPlugin.kConfigKey;

  String get _childDirectory => 'butter';

  @override
  bool existsSync() => editableDirectory.existsSync();

  // @override
  // File get cmakeFile => editableDirectory.childFile('CMakeLists.txt');

  // @override
  // File get managedCmakeFile => managedDirectory.childFile('CMakeLists.txt');

  // @override
  // File get generatedCmakeConfigFile =>
  //     ephemeralDirectory.childFile('generated_config.cmake');

  // @override
  // File get generatedPluginCmakeFile =>
  //     managedDirectory.childFile('generated_plugins.cmake');

  // @override
  // Directory get pluginSymlinkDirectory =>
  //     ephemeralDirectory.childDirectory('.plugin_symlinks');

  Directory get editableDirectory =>
      parent.directory.childDirectory(_childDirectory);

  Directory get managedDirectory => editableDirectory.childDirectory('flutter');

  Directory get ephemeralDirectory =>
      managedDirectory.childDirectory('ephemeral');
}
