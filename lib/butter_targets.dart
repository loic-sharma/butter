import 'package:flutter_tools/src/artifacts.dart';
import 'package:flutter_tools/src/base/file_system.dart';
import 'package:flutter_tools/src/build_info.dart';
import 'package:flutter_tools/src/build_system/build_system.dart';
import 'package:flutter_tools/src/build_system/depfile.dart';
import 'package:flutter_tools/src/build_system/exceptions.dart';
import 'package:flutter_tools/src/build_system/targets/assets.dart';
import 'package:flutter_tools/src/build_system/targets/common.dart';
import 'package:flutter_tools/src/build_system/targets/icon_tree_shaker.dart';
import 'package:flutter_tools/src/build_system/targets/shader_compiler.dart';

// Forked from packages\flutter_tools\lib\src\build_system\targets\windows.dart

const String _kButterDepfile = 'butter_sources.d';

/// Copies the Butter files to the copy directory.
class UnpackButter extends Target {
  const UnpackButter();

  @override
  String get name => 'unpack_butter';

  @override
  List<Source> get inputs => const <Source>[
    // TODO: Verify
    Source.pattern('{FLUTTER_ROOT}/../../lib/butter_targets.dart'),
  ];

  @override
  List<Source> get outputs => const <Source>[];

  @override
  List<String> get depfiles => const <String>[_kButterDepfile];

  @override
  List<Target> get dependencies => const <Target>[];

  @override
  Future<void> build(Environment environment) async {
    final String? buildModeEnvironment = environment.defines[kBuildMode];
    if (buildModeEnvironment == null) {
      throw MissingDefineException(kBuildMode, name);
    }
    final String? targetPlatformEnvironment = environment.defines[kTargetPlatform];
    if (targetPlatformEnvironment == null) {
      throw MissingDefineException(kTargetPlatform, name);
    }
    final BuildMode buildMode = BuildMode.fromCliName(buildModeEnvironment);
    final TargetPlatform targetPlatform = getTargetPlatformForName(
      targetPlatformEnvironment,
    );
    final Directory ephemeralDirectory = environment.outputDir;
    final Depfile depfile = _unpackButterArtifacts(
      buildMode,
      targetPlatform,
      ephemeralDirectory,
      environment.fileSystem,
      environment.artifacts,
    );
    environment.depFileService.writeToFile(
      depfile,
      environment.buildDir.childFile(_kButterDepfile),
    );
  }
}

// See: packages\flutter_tools\lib\src\build_system\targets\windows.dart
// See: packages\flutter_tools\lib\src\build_system\targets\desktop.dart
const List<String> _kWindowsArtifacts = <String>[
  'flutter_windows.dll',
  'flutter_windows.dll.pdb',
];
Depfile _unpackButterArtifacts(
  BuildMode buildMode,
  TargetPlatform targetPlatform,
  Directory ephemeralDirectory,
  FileSystem fs,
  Artifacts artifacts,
) {
  final List<File> inputs = <File>[];
  final List<File> outputs = <File>[];
  final String artifactsPath = artifacts.getArtifactPath(
    Artifact.windowsDesktopPath,
    platform: targetPlatform,
    mode: buildMode,
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
      'artifacts',
      fs.path.relative(artifactPath, from: artifactsPath),
    );
    final File artifactFile = fs.file(artifactPath);
    final File destinationFile = fs.file(outputPath);
    if (!destinationFile.parent.existsSync()) {
      destinationFile.parent.createSync(recursive: true);
    }
    artifactFile.copySync(destinationFile.path);
    inputs.add(artifactFile);
    outputs.add(destinationFile);
  }

  // Copy ICU data
  final String icuDataPath = artifacts.getArtifactPath(
    Artifact.icuData,
    platform: targetPlatform,
  );
  final File icuDataFile = fs.file(icuDataPath);
  final String icuDataDestinationPath = fs.path.join(
    ephemeralDirectory.path,
    'artifacts',
    'data',
    icuDataFile.basename,
  );
  final File icuDataDestinationFile = fs.file(icuDataDestinationPath);
  if (!icuDataDestinationFile.parent.existsSync()) {
    icuDataDestinationFile.parent.createSync(recursive: true);
  }
  icuDataFile.copySync(icuDataDestinationFile.path);
  inputs.add(icuDataFile);
  outputs.add(icuDataDestinationFile);

  return Depfile(inputs, outputs);
}

/// Creates a bundle for the Windows Butter desktop target.
abstract class BundleButterAssets extends Target {
  const BundleButterAssets();

  @override
  List<Target> get dependencies => const <Target>[
    KernelSnapshot(),
    UnpackButter(),
  ];

  @override
  List<Source> get inputs => const <Source>[
    // TODO: Verify
    Source.pattern('{FLUTTER_ROOT}/../../lib/butter_targets.dart'),
    Source.pattern('{PROJECT_DIR}/pubspec.yaml'),
    ...IconTreeShaker.inputs,
  ];

  @override
  List<String> get depfiles => const <String>[
    'flutter_assets.d',
  ];

  @override
  Future<void> build(Environment environment) async {
    final String? buildModeEnvironment = environment.defines[kBuildMode];
    if (buildModeEnvironment == null) {
      throw MissingDefineException(kBuildMode, 'bundle_butter_assets');
    }
    final String? targetPlatformEnvironment = environment.defines[kTargetPlatform];
    if (targetPlatformEnvironment == null) {
      throw MissingDefineException(kTargetPlatform, name);
    }
    final BuildMode buildMode = BuildMode.fromCliName(buildModeEnvironment);
    final TargetPlatform targetPlatform = getTargetPlatformForName(
      targetPlatformEnvironment,
    );

    final Directory outputDirectory = environment.outputDir
      .childDirectory('artifacts')
      .childDirectory('data')
      .childDirectory('flutter_assets');
    if (!outputDirectory.existsSync()) {
      outputDirectory.createSync(recursive: true);
    }

    // Only copy the kernel blob in debug mode.
    if (buildMode == BuildMode.debug) {
      environment.buildDir.childFile('app.dill')
        .copySync(outputDirectory.childFile('kernel_blob.bin').path);
    }

    final Depfile depfile = await copyAssets(
      environment,
      outputDirectory,
      targetPlatform: targetPlatform,
      shaderTarget: ShaderTarget.sksl,
    );
    environment.depFileService.writeToFile(
      depfile,
      environment.buildDir.childFile('flutter_assets.d'),
    );
  }
}

/// A wrapper for AOT compilation that copies app.so into the output directory.
class ButterAotBundle extends Target {
  /// Create a [ButterAotBundle] wrapper for [aotTarget].
  const ButterAotBundle(this.aotTarget);

  /// The [AotElfBase] subclass that produces the app.so.
  final AotElfBase aotTarget;

  @override
  String get name => 'butter_aot_bundle';

  @override
  List<Source> get inputs => const <Source>[
    Source.pattern('{BUILD_DIR}/app.so'),
  ];

  @override
  List<Source> get outputs =>
    const <Source>[
      Source.pattern('{OUTPUT_DIR}/artifacts/data/app.so'),
    ];

  @override
  List<Target> get dependencies => <Target>[
    aotTarget,
  ];

  @override
  Future<void> build(Environment environment) async {
    final File outputFile = environment.buildDir.childFile('app.so');
    final Directory outputDirectory = environment.outputDir
      .childDirectory('artifacts')
      .childDirectory('data');
    if (!outputDirectory.existsSync()) {
      outputDirectory.createSync(recursive: true);
    }
    outputFile.copySync(outputDirectory.childFile('app.so').path);
  }
}

class ReleaseBundleButterAssets extends BundleButterAssets {
  const ReleaseBundleButterAssets();

  @override
  String get name => 'release_bundle_butter_assets';

  @override
  List<Source> get outputs => const <Source>[];

  @override
  List<Target> get dependencies => <Target>[
    ...super.dependencies,
    const ButterAotBundle(AotElfRelease(TargetPlatform.windows_x64)),
  ];
}

class ProfileBundleButterAssets extends BundleButterAssets {
  const ProfileBundleButterAssets();

  @override
  String get name => 'profile_bundle_butter_assets';

  @override
  List<Source> get outputs => const <Source>[];

  @override
  List<Target> get dependencies => <Target>[
    ...super.dependencies,
    const ButterAotBundle(AotElfProfile(TargetPlatform.windows_x64)),
  ];
}

class DebugBundleButterAssets extends BundleButterAssets {
  const DebugBundleButterAssets();

  @override
  String get name => 'debug_bundle_butter_assets';

  @override
  List<Source> get inputs => <Source>[
    const Source.pattern('{BUILD_DIR}/app.dill'),
  ];

  @override
  List<Source> get outputs => <Source>[
    const Source.pattern('{OUTPUT_DIR}/artifacts/data/flutter_assets/kernel_blob.bin'),
  ];
}
