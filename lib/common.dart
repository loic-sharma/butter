/// @docImport 'package:flutter_tools/src/cache.dart';
library;

import 'package:flutter_tools/src/globals.dart' as globals;
import 'package:path/path.dart' as path;

/// See: [Cache.defaultFlutterRoot] in `cache.dart`
String get butterRootPath {
  final String scriptPath = globals.platform.script.toFilePath();
  return path.normalize(path.join(
    scriptPath,
    scriptPath.endsWith('.snapshot') ? '../../..' : '../..',
  ));
}
