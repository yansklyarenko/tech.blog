---
Title: "IIS extension: WebAppPool"
Published: 2009-01-20 22:06
categories:
    - WiX
    - Windows Installer
    - IIS extension
RedirectFrom: "blog/2009/01/20/iis-extension-webapppool"
---

Another challenge - another piece of fun with WiX. Imagine the following requirement: the installation program must install an application pool on IIS6+ environments; the multiple installed instances should use the same application pool. In other words, the application pool must be created with the first instance installation, and must be removed with the last instance uninstallation.

A special element for maintaining IIS AppPools in IIS extension is called `WebAppPool`. As usual, we’ll wrap it into a separate component, so that it is created on install. Later, we’ll create a special custom action to deceive the standard removing mechanism on uninstall:

```XML
<Component DiskId=”1” Id=”CreateIISAppPool” Guid=”{YOURGUID-6C5B-4980-AD0B-E32FA2DBC1F4}” Directory=”WebsiteFolder”>
    <Condition>IISMAJORVERSION <> “#5”</Condition>
    <iis:WebAppPool Id=”IISSiteAppPool6” Name=”[IISAPPPOOL_NAME]” MaxWorkerProcesses=”1” Identity=”networkService” />
    <RegistryKey Root=”HKLM” Key=”$(var.ParentKey)”>
        <RegistryValue Name=”IISAppPoolName” Type=”string” Value=”[IISAPPPOOL_NAME]” />
    </RegistryKey>
</Component>
```

As you can see, the component is installed once the target system has IIS 6+. It creates a `WebAppPool` with the name provided in `IISAPPPOOL_NAME` public property. It also writes this name into a registry value, which resides under the instance-specific registry key.

With this component included into the MSI package, the app pool is created when the first instance is installed, and nothing happens for second and subsequent instances.

Let’s examine the uninstall behavior. The MSI behaves natural - when it meets the component to uninstall, it removes the `WebAppPool` specified in it. But the IIS extension which performs the actual deletion of app pool, needs the name to be passed in it. So, the only thing we should do is to supply this action with a fake app pool name each time, except for the last instance uninstall.

Here is the algorithm:

 1. search the registry for the app pool name as usual
 2. schedule a special action on unistall after `AppSearch`, which detects if this is the last instance being uninstalled, and if not, “breaks” the app pool name into something non-existent

The first point is quite straight-forward:

```XML
<Property Id=”IISAPPPOOL_NAME”>
    <RegistrySearch Id=”IISAppPoolName” Root=”HKLM” Key=”$(var.ParentKey)” Name=”IISAppPoolName” Type=”raw” />
</Property>
```

The second one is not natural, like any hack:

```csharp
[CustomAction]
public static ActionResult ChangeWebAppPoolNameToDeceiveUninstall(Session session)
{
    int numberOfInstalled = 1;
    foreach (ProductInstallation product in ProductInstallation.GetRelatedProducts(session[“UpgradeCode”]))
    {
        if ((session[“ProductCode”] != product.ProductCode) && product.IsInstalled)
        {
            numberOfInstalled++;
            break;
        }
    }

    if (numberOfInstalled > 1)
    {
        session[“IISAPPPOOL_NAME”] += string.Format(“|{0}”, DateTime.Now.ToLongTimeString());
    }

    return ActionResult.Success;
}
```

It iterates the related products (those sharing the `UpgradeCode`), and if it finds others installed, except for itself, it changes the app pool name we retrieved from registry into something unique, for instance, appends a unique string.

Thus, the IIS custom action which is going to delete the app pool fails to find the one with the provided name, and does nothing. When, otherwise, it is the last instance being uninstalled, the retrieved app pool name remains unchanged, and the app pool is successfully removed.

Note that the mentioned action should be **immediate**, should occur **after** `AppSearch` on **uninstall**.

That’s it! I would appreciate any comments as usual.
