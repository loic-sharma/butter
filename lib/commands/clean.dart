import 'package:file/file.dart';
import 'package:flutter_tools/src/base/common.dart';
import 'package:flutter_tools/src/base/logger.dart';
import 'package:flutter_tools/src/commands/clean.dart';
import 'package:flutter_tools/src/globals.dart' as globals;
import 'package:flutter_tools/src/project.dart';
import 'package:flutter_tools/src/runner/flutter_command.dart';
import 'package:path/path.dart';

import '../butter_project.dart';

class ButterCleanCommand extends CleanCommand {
  ButterCleanCommand({super.verbose});

  /// See: [CleanCommand.runCommand] in `clean.dart`
  @override
  Future<FlutterCommandResult> runCommand() async {
    final FlutterProject flutterProject = FlutterProject.current();
    _cleanButterProject(ButterProject.fromFlutter(flutterProject));

    return super.runCommand();
  }

  void _cleanButterProject(ButterProject project) {
    if (!project.existsSync()) {
      return;
    }
    _runDotnetClean(project);
    _deleteFile(project.ephemeralDirectory);
  }

  /// Source: [CleanCommand.deleteFile] in `clean.dart` (simplified)
  void _deleteFile(FileSystemEntity file) {
    if (!file.existsSync()) {
      return;
    }
    final String path = relative(file.path);
    final Status status = globals.logger.startProgress(
      'Deleting $path...',
    );
    try {
      file.deleteSync(recursive: true);
    } on FileSystemException catch (error) {
      globals.printError('Failed to remove $path: $error');
    } finally {
      status.stop();
    }
  }
}

Future<void> _runDotnetClean(ButterProject project) async {
  int result;
  try {
    result = await globals.processUtils.stream(
      <String>[
        'dotnet',
        'clean',
        project.hostAppRoot.path,
      ],
      trace: true,
    );
  } on ArgumentError {
    throwToolExit("dotnet not found. Run 'flutter doctor' for more information.");
  }
  if (result != 0) {
    throwToolExit('.NET clean failed');
  }
}
