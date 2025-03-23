// Do not add package imports to this file.
import 'dart:convert';
import 'dart:io';

Future<void> main(List<String> arguments) async {
  final String targetPlatform = arguments[0];
  final String buildMode = arguments[1].toLowerCase();

  final String? butterRoot = Platform.environment['BUTTER_ROOT'];
  final String? dartDefines = Platform.environment['DART_DEFINES'];
  final bool dartObfuscation = Platform.environment['DART_OBFUSCATION'] == 'true';
  final String? frontendServerStarterPath = Platform.environment['FRONTEND_SERVER_STARTER_PATH'];
  final String? extraFrontEndOptions = Platform.environment['EXTRA_FRONT_END_OPTIONS'];
  final String? extraGenSnapshotOptions = Platform.environment['EXTRA_GEN_SNAPSHOT_OPTIONS'];
  final String? flutterEngine = Platform.environment['FLUTTER_ENGINE'];
  final String? flutterRoot = Platform.environment['FLUTTER_ROOT'];
  final String flutterTarget =
      Platform.environment['FLUTTER_TARGET'] ?? pathJoin(<String>['lib', 'main.dart']);
  final String? codeSizeDirectory = Platform.environment['CODE_SIZE_DIRECTORY'];
  final String? localEngine = Platform.environment['LOCAL_ENGINE'];
  final String? localEngineHost = Platform.environment['LOCAL_ENGINE_HOST'];
  final String? projectDirectory = Platform.environment['PROJECT_DIR'];
  final String? splitDebugInfo = Platform.environment['SPLIT_DEBUG_INFO'];
  final String? bundleSkSLPath = Platform.environment['BUNDLE_SKSL_PATH'];
  final bool trackWidgetCreation = Platform.environment['TRACK_WIDGET_CREATION'] == 'true';
  final bool treeShakeIcons = Platform.environment['TREE_SHAKE_ICONS'] == 'true';
  final bool verbose = Platform.environment['VERBOSE_SCRIPT_LOGGING'] == 'true';
  final bool prefixedErrors = Platform.environment['PREFIXED_ERROR_LOGGING'] == 'true';

  if (projectDirectory == null) {
    stderr.write(
      'PROJECT_DIR environment variable must be set to the location of Flutter project to be built.',
    );
    exit(1);
  }
  if (butterRoot == null || butterRoot.isEmpty) {
    stderr.write(
      'BUTTER_ROOT environment variable must be set to the location of the Butter SDK.',
    );
    exit(1);
  }
  if (flutterRoot == null || flutterRoot.isEmpty) {
    stderr.write(
      'FLUTTER_ROOT environment variable must be set to the location of the Flutter SDK.',
    );
    exit(1);
  }

  Directory.current = projectDirectory;

  // TODO: Update these error messages
  if (localEngine != null && !localEngine.contains(buildMode)) {
    stderr.write('''
ERROR: Requested build with Flutter local engine at '$localEngine'
This engine is not compatible with FLUTTER_BUILD_MODE: '$buildMode'.
You can fix this by updating the LOCAL_ENGINE environment variable, or
by running:
  flutter build <platform> --local-engine=<platform>_$buildMode --local-engine-host=host_$buildMode
or
  flutter build <platform> --local-engine=<platform>_${buildMode}_unopt --local-engine-host=host_${buildMode}_unopt
========================================================================
''');
    exit(1);
  }
  if (localEngineHost != null && !localEngineHost.contains(buildMode)) {
    stderr.write('''
ERROR: Requested build with Flutter local engine host at '$localEngineHost'
This engine is not compatible with FLUTTER_BUILD_MODE: '$buildMode'.
You can fix this by updating the LOCAL_ENGINE_HOST environment variable, or
by running:
  flutter build <platform> --local-engine=<platform>_$buildMode --local-engine-host=host_$buildMode
or
  flutter build <platform> --local-engine=<platform>_$buildMode --local-engine-host=host_${buildMode}_unopt
========================================================================
''');
    exit(1);
  }
  final String dartExecutable = pathJoin(<String>[
    flutterRoot,
    'bin',
    'cache',
    'dart-sdk',
    'bin',
    'dart',
  ]);
  final String butterFile = pathJoin(<String>[
    butterRoot,
    'bin',
    'butter.dart',
  ]);
  // TODO: Support x64 and arm64 builds.
  final String target = '${buildMode}_bundle_butter_assets';
  //final String bundlePlatform = targetPlatform;
  // final String target = '${buildMode}_bundle_${bundlePlatform}_assets';
  final Process assembleProcess = await Process.start(dartExecutable, <String>[
    butterFile,
    if (verbose) '--verbose',
    if (prefixedErrors) '--prefixed-errors',
    if (flutterEngine != null) '--local-engine-src-path=$flutterEngine',
    if (localEngine != null) '--local-engine=$localEngine',
    if (localEngineHost != null) '--local-engine-host=$localEngineHost',
    'assemble',
    '--no-version-check',
    '--output=build',
    '-dTargetPlatform=$targetPlatform',
    '-dTrackWidgetCreation=$trackWidgetCreation',
    '-dBuildMode=$buildMode',
    '-dTargetFile=$flutterTarget',
    '-dTreeShakeIcons="$treeShakeIcons"',
    '-dDartObfuscation=$dartObfuscation',
    if (bundleSkSLPath != null) '-dBundleSkSLPath=$bundleSkSLPath',
    if (codeSizeDirectory != null) '-dCodeSizeDirectory=$codeSizeDirectory',
    if (splitDebugInfo != null) '-dSplitDebugInfo=$splitDebugInfo',
    if (dartDefines != null) '--DartDefines=$dartDefines',
    if (extraGenSnapshotOptions != null) '--ExtraGenSnapshotOptions=$extraGenSnapshotOptions',
    if (frontendServerStarterPath != null) '-dFrontendServerStarterPath=$frontendServerStarterPath',
    if (extraFrontEndOptions != null) '--ExtraFrontEndOptions=$extraFrontEndOptions',
    target,
  ]);
  assembleProcess.stdout
      .transform(utf8.decoder)
      .transform(const LineSplitter())
      .listen(stdout.writeln);
  assembleProcess.stderr
      .transform(utf8.decoder)
      .transform(const LineSplitter())
      .listen(stderr.writeln);

  if (await assembleProcess.exitCode != 0) {
    exit(1);
  }
}

/// Perform a simple path join on the segments based on the current platform.
///
/// Does not normalize paths that have repeated separators.
String pathJoin(List<String> segments) {
  final String separator = Platform.isWindows ? r'\' : '/';
  return segments.join(separator);
}
