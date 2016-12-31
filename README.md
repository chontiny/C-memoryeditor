# Anathena

Official web site: [anathena.com](https://www.anathena.com)

**Anathena** is Memory Editing software that allows users to create and share cheats in their windows desktop games. This includes memory scanning, pointers, x86/x64 assembly injection, and so on.

## Wiki Documentation

You can find more documentation on the [Wiki](https://github.com/zcanann/Anathena/wiki)

## Build

In order to compile Anathena, you should only need **Visual Studio 2015 Update 3**. External libraries are mostly compiled from source (with the exception of most C++ applications). This is because almost every library I use has had issues where I've needed access to the source code (to be fair, I am using most of these in unexpected ways). Currently this consists of:

Library | Description
--- | ---
[SharpDX](https://github.com/sharpdx/SharpDX) | DirectX Wrapper
[OpenTK](https://github.com/opentk/opentk) | OpenGL Wrapper (Not actually used yet)
[PeNet](https://github.com/secana/PeNet) | PE Format Parser
[CLRMD](https://github.com/Microsoft/clrmd) | .NET Application Inspection Library
[AvalonDock](https://avalondock.codeplex.com/) | Docking Library
[AvalonEdit](https://github.com/icsharpcode/AvalonEdit) | Code Editing Library
[LiveCharts](https://github.com/beto-rodriguez/Live-Charts) | WPF Charts
[CsScript](https://github.com/oleg-shilo/cs-script) | C# Scripting Library
[EasyHook](https://github.com/EasyHook/EasyHook) | Managed/Unmanaged API Hooking
[SharpDisasm](https://github.com/spazzarama/SharpDisasm) | Udis86 Assembler Ported to C#
[MemorySharp](https://github.com/ZenLulz/MemorySharp) | Memory Editing Library
[FASM](https://flatassembler.net/) | Flat Assembler (C# Invocation done via MemorySharp)
[TreeViewAdv](https://sourceforge.net/projects/treeviewadv/) | Advanced Tree View Control
[WPFToolKit](http://wpftoolkit.codeplex.com/) | Toolkit for WPF Applications

## Licensing
 
GPLv3
