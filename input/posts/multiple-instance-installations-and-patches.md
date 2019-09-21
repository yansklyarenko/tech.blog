---
Title: "Multiple instance installations and patching"
Published: 2008-12-30 11:00
Tags:
    - MSI
    - WiX
    - patching
    - Windows Installer
    - multiple instance
RedirectFrom: "blog/2008/12/30/multiple-instance-installations-and"
---

Well, this is the first message to my first weblog, and it should be outstanding by default. :-)

Sometimes, when creating an installation program, it is necessary to support multiple instance installations of the product to one particular computer. This requirement immediately brings the complexity of the installation to the advanced level. A couple of most tricky things to look after are component rules and patching.

In order to make your installation “multi-instance-aware”, you should author a number of instance transforms in your source. The number of transforms is the number of instances you plan to support (except for the default one, of course). Fortunately, WiX provides very convenient way to do this:

```XML
<instancetransforms property=”ANY_PROPERTY”>
    <instance id=”InstanceId1” productcode=”{42A33A91-36B0-4700-A6F5-1289D22F358C}” />
    <instance id=”InstanceId2” productcode=”{68C62C01-D064-4CF0-9239-F5D2FF36BD9A}” />
    ...
</instancetransforms>
```

As always with Windows Installer, this is not the end. According to the [MSI documentation about authoring multiple instances](http://msdn.microsoft.com/en-us/library/aa367797(VS.85).aspx), “To keep the nonfile data of each instance isolated, the base package should collect nonfile data into sets of components for each instance”. This can be done by authoring a duplicate of each component for each instance, and install conditionally. But it becomes really complex to manage when you have much more than 2 instances, let’s say, 100.

I chose another way. Assuming the fact that each instance should contain the same set of components, but with different GUIDs, we can generate a number of transforms, one per each instance, which change just the GUIDs of all the components. So, the algorithm is similar to this:

- copy the MSI package
- use API to query and update the database with new GUIDs for each component
- generate a transform between the copy and original MSI
- drop the copy MSI
- repeat the steps about the number of times as many instances you plan to support
- embed all these transforms into the original MSI

Obviously, the number of such customization transforms must be equal to the number of instance transforms and the names should be convenient to use. For instance, if you did everything correctly, you should be able to run the installation of new instance as follows:

```BAT
msiexec /i YourPackage.msi MSINEWINSTANCE=1 TRANSFORMS=:InstanceId1;:ComponentGUIDTransform1.mst ...
```

This works like a charm in conjunction with an algorithm to detect next available instance and a bootstrapper.

Now let’s turn to the patching. When I browsed the internet for the info about multiple instance installs and patches, I found [a great post of Christopher Painter](http://blog.deploymentengineering.com/2006/10/multiple-instance-msis-and.html). As he says there, one should populate the Targets property of the patch summary info stream with product codes of all the instances, otherwise the patch detects just the default instance. That’s correct, but, yes, this is not the end of the story.

Let’s take a look at the [patch definition and its contents](http://msdn.microsoft.com/en-us/library/aa370578(VS.85).aspx): “Patches contain at a minimum, two database transforms and can contain patch files that are stored in the cabinet file stream of the patch package”. These two transforms contain the target product GUID and updated product GUID each. In the case of simple patching, it is just one GUID of the target product.

Hence, in order to be applied to each instance, the patch must contain a pair of transforms for each instance. Unfortunately, it is not supported by WiX torch+pyro approach and we should fall back to the powerful API:

```csharp
// dump transform and change its properties
string transformFileName = GetNextValidName(transformName, nameSuffix);
patch.ExtractTransform(transformName, transformFileName);
SummaryInfo info = new SummaryInfo(transformFileName, true);
info.RevisionNumber = info.RevisionNumber.Replace(originalProductCode, productCode);
info.Persist();
info.Close();
```

So, as you can see, we do the following (for each instance and for each of 2 transforms in default patch):

- extract transform from the patch package
- change the original product code to this instance product code in summary info

Afterwards, we must insert these newly created transforms into the _Storages table of the patch package:

```csharp
using (View insertView = patchForWrite.OpenView(“INSERT INTO `_Storages` (`Name`,`Data`) VALUES (‘{0}’, ?)”, transformFileName))
{
    using (Record record = new Record(1))
    {
        record.SetStream(1, new FileStream(transformFileName, FileMode.Open));
        insertView.Execute(record);
        patchForWrite.Commit();
    }
}
```

And finally, we should append the product GUID of each instance to the Template property of Summary info (it is shown as Targets with Orca) and the name of each transform to the LastSavedBy property of the Summary info (it is not shown with Orca). Something like this:

```csharp
// update patch properties
if (!patchForWrite.SummaryInfo.Template.Contains(productCode))
{
    patchForWrite.SummaryInfo.Template += “;” + productCode;
}
patchForWrite.SummaryInfo.LastSavedBy += “;:” + transformFileName;
patchForWrite.SummaryInfo.Persist();
```

That’s it! Afterwards, the following magic line should work correctly and patch the installed instance of your application:

```BAT
msiexec /p YourPatch.msp /n {YOURGUID-0002-0000-0000-624474736554} /qb
```

Good luck deploying! I would appreciate any comments on this.
