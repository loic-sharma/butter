import 'package:flutter_tools/src/application_package.dart';
import 'package:flutter_tools/src/base/file_system.dart';
import 'package:flutter_tools/src/base/logger.dart';
import 'package:flutter_tools/src/base/os.dart';
import 'package:flutter_tools/src/build_info.dart';
import 'package:flutter_tools/src/desktop_device.dart';
import 'package:flutter_tools/src/device.dart';
import 'package:flutter_tools/src/globals.dart' as globals;
import 'package:flutter_tools/src/project.dart';
import 'package:flutter_tools/src/windows/windows_workflow.dart';
import 'package:process/process.dart';

import 'butter_build.dart';
import 'butter_project.dart';

class ButterDeviceManager extends DeviceManager {
  ButterDeviceManager({
    required super.logger,
    required ProcessManager processManager,
    required FileSystem fileSystem,
    required OperatingSystemUtils operatingSystemUtils,
    required WindowsWorkflow windowsWorkflow,
  }) : deviceDiscoverers = <DeviceDiscovery>[
    ButterDevices(
      processManager: processManager,
      operatingSystemUtils: operatingSystemUtils,
      logger: logger,
      fileSystem: fileSystem,
      windowsWorkflow: windowsWorkflow,
    ),
  ];

  @override
  final List<DeviceDiscovery> deviceDiscoverers;
}

class ButterDevices extends PollingDeviceDiscovery {
  ButterDevices({
    required ProcessManager processManager,
    required Logger logger,
    required FileSystem fileSystem,
    required OperatingSystemUtils operatingSystemUtils,
    required WindowsWorkflow windowsWorkflow,
  }) : _fileSystem = fileSystem,
      _logger = logger,
      _processManager = processManager,
      _operatingSystemUtils = operatingSystemUtils,
      _windowsWorkflow = windowsWorkflow,
      super('butter devices');

  final FileSystem _fileSystem;
  final Logger _logger;
  final ProcessManager _processManager;
  final OperatingSystemUtils _operatingSystemUtils;
  final WindowsWorkflow _windowsWorkflow;

  @override
  bool get supportsPlatform => _windowsWorkflow.appliesToHostPlatform;

  @override
  bool get canListAnything => _windowsWorkflow.canListDevices;

  @override
  Future<List<Device>> pollingGetDevices({ Duration? timeout }) async {
    if (!canListAnything) {
      return const <Device>[];
    }
    return <Device>[
      ButterDevice(
        fileSystem: _fileSystem,
        logger: _logger,
        processManager: _processManager,
        operatingSystemUtils: _operatingSystemUtils,
      ),
    ];
  }

  @override
  Future<List<String>> getDiagnostics() async => const <String>[];

  @override
  List<String> get wellKnownIds => const <String>['butter'];
}

/// A device that represents a desktop Butter Windows target.
class ButterDevice extends DesktopDevice {
  ButterDevice({
    required ProcessManager processManager,
    required Logger logger,
    required FileSystem fileSystem,
    required OperatingSystemUtils operatingSystemUtils,
  }) : super(
      'butter',
      platformType: PlatformType.windows,
      ephemeral: false,
      processManager: processManager,
      logger: logger,
      fileSystem: fileSystem,
      operatingSystemUtils: operatingSystemUtils,
  );

  @override
  bool isSupported() => true;

  @override
  String get name => 'Butter';

  @override
  Future<TargetPlatform> get targetPlatform async => TargetPlatform.windows_x64;

  @override
  bool isSupportedForProject(FlutterProject flutterProject) {
    return ButterProject.fromFlutter(flutterProject).existsSync();
  }

  @override
  Future<void> buildForDevice({
    String? mainPath,
    required BuildInfo buildInfo,
  }) async {
    await buildButter(
      ButterProject.fromFlutter(FlutterProject.current()),
      buildInfo,
      await targetPlatform,
      mainPath ?? 'lib/main.dart',
    );
  }

  @override
  String executablePathForDevice(ApplicationPackage package, BuildInfo buildInfo) {
    final project = ButterProject.fromFlutter(FlutterProject.current());

    // TODO: Better logic to find the TFM / build mode / app name
    return globals.fs.directory(getButterBuildDirectory())
      .childDirectory(buildInfo.mode == BuildMode.debug ? 'Debug' : 'Release')
      .childFile('${project.parent.manifest.appName}.exe')
      .path;
  }
}
