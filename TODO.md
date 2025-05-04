Initial:
1. Clean should remove obj/bin/ephemeral folders
1. Plugins
    1. Plugin template
    1. Inject plugin .csproj into the app's .sln. Maybe move .sln to build folder?
    1. Dart plugins
    1. C++ plugins
1. Update README
1. Docs using https://docs.page
    1. Get started
        1. How to install VS Code + .NET
    1. Changelog
    1. How to create a plugin
    1. How to find pub.dev plugins
    1. How to debug C# code
    1. CLI [tab](https://use.docs.page/navigation#tab-navigation)
        1. Build command
        1. Create command
        1. Etc...
1. Do a pass on runner app. Make sure it supports everything Flutter Windows does.
1. Do a pass on TODOs.
1. Do a pass on VS Code lints.
1. Check .NET public APIs. Make stuff internal. Add comments.

After:
1. SynchronizationContext for default win32 app? Provide a way to get back to the main thread?
1. Improve build performance
1. Doctor command
1. Upgrade command
1. Add-to-app
    1. Win32
    1. WPF
    1. WinForms
1. Tests
    1. Integration tests
    1. Test that templates copied from Flutter are up-to-date. Ensure not missing new templates.
1. Local engine
1. Support x64 and arm64
1. Docs
    1. Add-to-app
    1. How to bind to native code
    1. How to deploy Butter app
    1. .NET API reference
1. Launch Flutter app from VS Code with debugging
1. More messaging codecs like JSON
1. Pigeon
1. NativeAOT
1. Games: Godot and Unity
