import 'package:flutter_tools/src/cache.dart';
import 'package:flutter_tools/src/commands/run.dart';

class ButterRunCommand extends RunCommand {
  ButterRunCommand({super.verboseHelp});

  @override
  Future<Set<DevelopmentArtifact>> get requiredArtifacts async =>
    <DevelopmentArtifact>{
      DevelopmentArtifact.windows
    };
}
