---
Title: "A solution can build fine from inside the Visual Studio, but fail to build with msbuild.exe"
Published: 2012-10-29 16:35
Tags:
    - visual studio
    - sln
    - .NET
    - upgrade
    - .net 2.0
    - .net 3.5
    - csproj
    - assembly
RedirectFrom: "blog/2012/10/29/a-solution-can-build-fine-from-inside"
---

Today I have faced with an interesting issue. Although I failed to reproduce it on a fresh new project, I think this info might be useful for others.

I have a solution which was upgraded from targeting .NET Framework 2.0 to .NET Framework 3.5. I've got a patch from a fellow developer to apply to one of the projects of that solution. The patch adds new files as well as modifies existing ones. After the patch application, the solution is successfully built from inside the Visual Studio, but fails to build from the command line with msbuild.exe. The error thrown states that

> "The type or namespace name 'Linq' does not exist in the namespace 'System' ".

The msbuild version is 3.5:

```LOG
[exec] Microsoft (R) Build Engine Version 3.5.30729.5420
[exec] [Microsoft .NET Framework, Version 2.0.50727.5456]
[exec] Copyright (C) Microsoft Corporation 2007. All rights reserved.
```

It turns out this issue has been met by other people, and even reported to Microsoft. Microsoft suggested to use MSBuild.exe 4.0 to build VS 2010 projects. However, they confirmed it is possible to use MSBuild.exe 3.5  - in this case a reference to `System.Core` (3.5.0.0) must be explicitly added to the `csproj` file.
If you try to add a reference to `System.Core` from inside the Visual Studio, you'll get the error saying:

> "A reference to 'System.Core' could not be added. This component is already automatically referenced by the build system"

So, it seems that when you build a solution from inside the Visual Studio, it is capable to automatically load implicitly referenced assemblies. I suppose, MSBuild.exe 4.0 (and even SP1-patched MSBuild.exe 3.5?) can do this as well. Apparently, this has also turned out to be a known problem – you can't add that reference from the IDE. Open `csproj` file in your favorite editor and add this:

```XML
<Reference Include="System.Core" />
```

After this, the project builds fine in both VS and MSBuild.
