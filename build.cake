// The following environment variables need to be set for Netlify target:
// NETLIFY_TOKEN

#tool nuget:?package=Wyam&version=2.2.4
#addin nuget:?package=Cake.Wyam&version=2.2.4
#addin "NetlifySharp"

using NetlifySharp;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .Does(() =>
    {
        Wyam(new WyamSettings
        {
            Recipe = "Blog",
            Theme = "CleanBlog",
            UpdatePackages = true
        });
    });

Task("Preview")
    .Does(() =>
    {
        Wyam(new WyamSettings
        {
            Recipe = "Blog",
            Theme = "CleanBlog",
            UpdatePackages = true,
            Preview = true,
            Watch = true
        });
    });

Task("Netlify")
    .Does(() =>
    {
        var netlifyToken = EnvironmentVariable("NETLIFY_TOKEN");
        Information("The token is " + netlifyToken);
        if(string.IsNullOrEmpty(netlifyToken))
        {
            throw new Exception("Could not get Netlify token environment variable");
        }

        Information("Deploying output to Netlify");
        var client = new NetlifyClient(netlifyToken);
        var outputFolder = MakeAbsolute(Directory("./output")).FullPath;
        Information("The site output is in " + outputFolder);
        // Adding some debug info temporarily
        Information("======================== Files in the /output folder");
        var outputFiles = System.IO.Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories);
        foreach (var file in outputFiles)
        {
            Information(file);
        }
        client.RequestHandler = a =>
        {
            Information("======================== The request:");
            Information(a);
        };
        client.ResponseHandler = x =>
        {
            Information("======================== The response:");
            Information(x);
        };

        client.UpdateSite($"yansklyarenko.netlify.com", outputFolder).SendAsync().Wait();
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Preview");

Task("BuildServer")
    .IsDependentOn("Build")
    .IsDependentOn("Netlify");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);