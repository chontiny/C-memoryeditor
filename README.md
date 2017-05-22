# Squalr

[![License: GPL v3](https://img.shields.io/badge/License-GPL%20v3-blue.svg)](http://www.gnu.org/licenses/gpl-3.0)

Official web site: [anathena.com](https://www.squalr.com)

**Squalr** is Memory Editing software that allows users to create and share cheats in their windows desktop games. This includes memory scanning, pointers, x86/x64 assembly injection, and so on.

## Wiki Documentation

You can find more documentation on the [Wiki](https://github.com/Squalr/Squalr/wiki)

## Build

In order to compile Squalr, you should only need **Visual Studio 2017**. External libraries are mostly compiled from source (with the exception of most C++ applications). This is because almost every library I use has had issues where I've needed access to the source code (to be fair, I am using most of these in unexpected ways). Currently this consists of:

Library | Description
--- | ---
[SharpDX](https://github.com/sharpdx/SharpDX) | DirectX Wrapper
[OpenTK](https://github.com/opentk/opentk) | OpenGL Wrapper (Not actually used yet)
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
[TwitchLib](https://github.com/swiftyspiffy/TwitchLib) | Twitch API Wrapper
[WebSocketSharp](https://github.com/sta/websocket-sharp) | WebSocket Protocol Implementation
[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) | JSON Framework

Buff icons made by:
- Carl Olsen, https://twitter.com/unstoppableCarl
- Cathelineau
- Delapouite, http://delapouite.com
- Faithtoken, http://fungustoken.deviantart.com
- Lorc, http://lorcblog.blogspot.com
- PriorBlue
- Skoll
- Zeromancer - CC0