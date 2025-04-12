import 'package:args/args.dart';

import 'package:flutter_tools/src/base/common.dart';
import 'package:flutter_tools/src/build_system/build_system.dart';
import 'package:flutter_tools/src/build_system/targets/assets.dart';
import 'package:flutter_tools/src/build_system/targets/common.dart';
import 'package:flutter_tools/src/commands/assemble.dart';
import 'package:flutter_tools/src/runner/flutter_command.dart';

import '../butter_targets.dart';

/// All currently implemented targets.
List<Target> _kDefaultTargets = <Target>[
  // Shared targets
  const CopyAssets(),
  const KernelSnapshot(),
  // This is a one-off rule for bundle and aot compat.
  const CopyFlutterBundle(),
  // Butter targets
  // TODO: Support x64 and arm64 for butter.
  const UnpackButter(),
  const DebugBundleButterAssets(),
  const ProfileBundleButterAssets(),
  const ReleaseBundleButterAssets(),
  // const UnpackButter(TargetPlatform.windows_x64),
  // const UnpackButter(TargetPlatform.windows_arm64),
  // const DebugBundleButterAssets(TargetPlatform.windows_x64),
  // const DebugBundleButterAssets(TargetPlatform.windows_arm64),
  // const ProfileBundleButterAssets(TargetPlatform.windows_x64),
  // const ProfileBundleButterAssets(TargetPlatform.windows_arm64),
  // const ReleaseBundleButterAssets(TargetPlatform.windows_x64),
  // const ReleaseBundleButterAssets(TargetPlatform.windows_arm64),
];

class ButterAssembleCommand extends AssembleCommand {
  ButterAssembleCommand({super.verboseHelp, required super.buildSystem});

  @override
  Future<Set<DevelopmentArtifact>> get requiredArtifacts async =>
    <DevelopmentArtifact>{
      DevelopmentArtifact.windows
    };

  /// The target(s) we are building.
  List<Target> createTargets() {
    final ArgResults argumentResults = argResults!;
    if (argumentResults.rest.isEmpty) {
      throwToolExit('missing target name for flutter assemble.');
    }
    final String name = argumentResults.rest.first;
    final Map<String, Target> targetMap = <String, Target>{
      for (final Target target in _kDefaultTargets) target.name: target,
    };
    final List<Target> results = <Target>[
      for (final String targetName in argumentResults.rest)
        if (targetMap.containsKey(targetName)) targetMap[targetName]!,
    ];
    if (results.isEmpty) {
      throwToolExit('No target named "$name" defined.');
    }
    return results;
  }
}
