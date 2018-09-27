About
=====

Win3mu fork from Topten Software

Compilation
===========

Requires Visual Studio 2017 and .NET 4.6.1

Usage
=====

Right-click on your .16-bit exe file, then choose "Convert with Win3mu". If everything is good, your original file will be renamed and the new exe will appear with the original icon.

You can then run the new executable. If it complains about some modules (SHELL, COMMDLG, OLECLI...), copy the corresponding DLL file from the original WINDOWS\SYSTEM to the current folder, and try again.

Finally, if you get some error like "Unsupported ordinal #**** in module **** invoked", then sorry, this particular function hasn't been implemented yet.

Original links and source code
==============================

- About the project: https://www.toptensoftware.com/win3mu/
- Technical details: https://hackernoon.com/win3mu-part-1-why-im-writing-a-16-bit-windows-emulator-2eae946c935d
- Win3mu: https://bitbucket.org/toptensoftware/win3mu
- Sharp86: https://bitbucket.org/toptensoftware/sharp86
- ConFrames: https://bitbucket.org/toptensoftware/conframes
- PetaJson: https://github.com/toptensoftware/PetaJson
