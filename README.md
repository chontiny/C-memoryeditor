# Squalr

[![License: GPL v3](https://img.shields.io/badge/License-GPL%20v3-blue.svg)](http://www.gnu.org/licenses/gpl-3.0)

[Squalr Official Website](https://www.squalr.com)

Join us on our [Discord Channel](https://discord.gg/Pq2msTx)

**Squalr** is performant Memory Editing software that allows users to create and share cheats in their windows desktop games. This includes memory scanning, pointers, x86/x64 assembly injection, and so on.

How does Squalr achieve fast memory scans in .NET? Multi-threading combined with single-core parallelism via SIMD instructions. See this article: [SIMD in .NET](https://instil.co/2016/03/21/parallelism-on-a-single-core-simd-with-c/). To take advantage of these gains, your CPU needs to have support for SSE, AVX, or AVX-512.

## Wiki Documentation

You can find more documentation on the [Wiki](https://github.com/Squalr/Squalr/wiki)

## Recommended Visual Studio Extensions
Reference | Description 
--- | ---
[XAML Formatter](https://marketplace.visualstudio.com/items?itemName=TeamXavalon.XAMLStyler) | XAML should be run through this formatter
[StyleCop](https://marketplace.visualstudio.com/items?itemName=ChrisDahlberg.StyleCop) | StyleCop to enforce code conventions. Note that we deviate on some standard conventions. We use the full type name for variables (ex Int32 rather than int). The reasoning is that this is a memory editor, so we prefer to use the type name that is most explicit to avoid coding mistakes.

## Build

In order to compile Squalr, you should only need **Visual Studio 2017**. This should be up to date, we frequently update Squalr to use the latest version of the .NET framework. Here are the important 3rd party libraries that this project uses:

Library | Description 
--- | ---
[EasyHook](https://github.com/EasyHook/EasyHook) | Managed/Unmanaged API Hooking
[SharpDisasm](https://github.com/spazzarama/SharpDisasm) | Udis86 Assembler Ported to C#
[MemorySharp](https://github.com/ZenLulz/MemorySharp) | Memory Editing Library
[FASM](https://flatassembler.net/) | Flat Assembler (C# Invocation done via MemorySharp)
[CsScript](https://github.com/oleg-shilo/cs-script) | C# Scripting Library
[AvalonEdit](https://github.com/icsharpcode/AvalonEdit) | Code Editing Library
[SharpDX](https://github.com/sharpdx/SharpDX) | DirectX Wrapper
[CLRMD](https://github.com/Microsoft/clrmd) | .NET Application Inspection Library
[AvalonDock](https://avalondock.codeplex.com/) | Docking Library
[LiveCharts](https://github.com/beto-rodriguez/Live-Charts) | WPF Charts

Intending to Use (Eventually):

Library | Description | Purpose
--- | --- | ---
[AsmJit](https://github.com/hypeartist/AsmJit) | x86/x64 Assembler | Replace FASM, improve scripting drastically
[AsmJit](https://github.com/asmjit/asmjit) | x86/x64 Assembler | Original C++ project. May port/interop this if the above version does not work (Neither may fully work, and something custom may be needed)
[WpfHexEditorControl](https://github.com/abbaye/WpfHexEditorControl) | Hex Editor | Hex editor / Memory Hex Editor
[Ninject](https://github.com/ninject/Ninject) | Dependency Injection Framework | (Option #1) Fixing my abuse of singletons
[AutoFac](https://github.com/autofac/Autofac) | Dependency Injection Framework | (Option #2) Fixing my abuse of singletons
[OpenTK](https://github.com/opentk/opentk) | OpenGL Wrapper | Graphics Injection
[SharpDX](https://github.com/sharpdx/SharpDX) | DirectX Wrapper | Graphics Injection (Currently using SharpDX just for input)
[SharpPCap](https://github.com/chmorgan/sharppcap) | Packet Capture | Packet Editor
[Packet.Net](https://github.com/antmicro/Packet.Net) | Packet Capture | Packet Editor