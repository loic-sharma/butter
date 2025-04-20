import 'package:flutter_tools/src/base/file_system.dart';
import 'package:flutter_tools/src/project.dart';

import 'butter_plugins.dart';

/// The Butter sub project.
class ButterProject extends FlutterProjectPlatform {
  ButterProject.fromFlutter(this.parent);

  final FlutterProject parent;

  @override
  String get pluginConfigKey => ButterPlugin.kConfigKey;

  String get _childDirectory => 'butter';

  @override
  bool existsSync() => _editableDirectory.existsSync();

  Directory get _editableDirectory => parent.directory.childDirectory(_childDirectory);

  Directory get hostAppRoot => _editableDirectory;

  /// The directory in the project that is managed by Flutter. As much as
  /// possible, files that are edited by Flutter tooling after initial project
  /// creation should live here.
  Directory get managedDirectory => _editableDirectory.childDirectory('Butter');

  /// The subdirectory of [managedDirectory] that contains files that are
  /// generated on the fly. All generated files that are not intended to be
  /// checked in should live here.
  Directory get ephemeralDirectory => managedDirectory.childDirectory('ephemeral');

  /// The directory in the project that is owned by the app. As much as
  /// possible, Flutter tooling should not edit files in this directory after
  /// initial project creation.
  Directory get runnerDirectory => _editableDirectory.childDirectory('Runner');

  /// The file containing the generated configuration for the current build.
  File get generatedConfigPropsFile => ephemeralDirectory.childFile('GeneratedConfig.props');

  /// The file containing the generated plugin registrant.
  File get generatedPluginRegistrantFile => ephemeralDirectory.childFile('GeneratedPluginRegistrant.cs');
}
