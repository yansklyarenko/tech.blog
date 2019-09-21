---
Title: "Generate a solution file for a number of C# projects files in a folder"
Published: 2012-07-06 14:59
Tags:
    - T4
    - .NET
    - template
    - C#
RedirectFrom: "blog/2012/07/06/generate-solution-file-for-number-of-c"
---

Some time ago I wrote my first T4 template which generates a solution (`*.sln`) file out of a number of C# project (`*.cspoj`) files, located in a folder and all descendants. Although it turned out not to be necessary to solve the task I was working on, and assuming it’s quite simple, I still decided to share it for further reference. May someone can find it useful. So, below is the entire T4 template, with no extra comments:

```csharp
Microsoft Visual Studio Solution File, Format Version 11.00
# Visual Studio 2010
<#@ template language="cs" hostspecific="false" #>
<#@ output extension=".sln" #>
<#@ parameter name="Folder" type="System.String" #>
<#@ assembly name="System.Core" #>
<#@ assembly name="System.Xml" #>
<#@ assembly name="System.Xml.Linq" #>
<#@ import namespace="System.IO" #>
<#@ import namespace="System.Linq" #>
<#@ import namespace="System.Xml.Linq" #>
<#
    if (Directory.Exists(Folder))
    {
        var csprojFiles= Directory.GetFiles(Folder, "*.csproj", SearchOption.AllDirectories);
        foreach (var file in csprojFiles)
        {
            ProjectFileMetaData metaData = new ProjectFileMetaData(file, Folder);
            WriteLine("Project(\"{3}\") = \"{0}\", \"{1}\", \"{2}\"",  metaData.Name, metaData.Path, metaData.Id, ProjectFileMetaData.ProjectTypeGuid);
            WriteLine("EndProject");
        }
    }
#>

<#
    public class ProjectFileMetaData
    {
        public static string ProjectTypeGuid = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";

        public ProjectFileMetaData(string file, string root)
        {
            InitProperties(file, root);
        }

        public string Name { get; set; }

        public string Path { get; set; }

        public string Id { get; set; }

        private void InitProperties(string file, string root)
        {
            XDocument xDoc = XDocument.Load(file);
            XNamespace ns = @"http://schemas.microsoft.com/developer/msbuild/2003";
            XElement xElement = xDoc.Root.Elements(XName.Get("PropertyGroup", ns.NamespaceName)).First().Element(XName.Get("ProjectGuid", ns.NamespaceName));
            if (xElement != null)
            {
                this.Id = xElement.Value;
            }

            this.Path = file.Substring(root.Length).TrimStart(new char[] { '\\' });

            this.Name = System.IO.Path.GetFileNameWithoutExtension(file);
        }
    }
#>
```
