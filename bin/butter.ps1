$binPath = $PSScriptRoot
$butterRoot = Resolve-Path -Path (Join-Path $binPath '..')
$flutterRoot = Join-Path $butterRoot 'third_party\flutter'
$dartBat = Join-Path $flutterRoot 'bin\dart.bat'
$butterPath = Join-Path $binPath 'butter.dart'

$env:FLUTTER_ROOT = $flutterRoot

& $dartBat run $butterPath @args
