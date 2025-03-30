import 'package:file/file.dart';
import 'package:flutter_tools/src/template.dart';

import 'common.dart';

class ButterTemplatePathProvider extends TemplatePathProvider {
  const ButterTemplatePathProvider();

  @override
  Directory directoryInPackage(String name, FileSystem fileSystem) {
    final String templatesDir = fileSystem.path.join(
      butterRootPath,
      'templates',
    );
    return fileSystem.directory(fileSystem.path.join(templatesDir, name));
  }
}
