$flutterRoot = $env:FLUTTER_ROOT
$dartExe = Join-Path $flutterRoot 'bin\cache\dart-sdk\bin\dart.exe'

$binPath = $PSScriptRoot 
$butterPath = Join-Path $binPath 'butter.dart'

& $dartExe run $butterPath @args
