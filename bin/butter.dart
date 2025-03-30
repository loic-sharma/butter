import 'dart:io';

import 'package:butter/executable.dart' as executable;

// TODO: Mirror Flutter's batch script entry point.
void main(List<String> args) {
  // if (args.isEmpty) {
  //   Platform.script;
  //   args = ['create', '.'];
  //   Directory.current = r'C:\Code\butter\example';
  // }
  executable.main(args);
}
