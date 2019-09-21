---
Title: "IIS extension: WebSite"
Published: 2009-01-19 22:04
Tags:
    - WiX
    - Windows Installer
    - IIS extension
RedirectFrom: "blog/2009/01/19/iis-extension-website"
---

Ok, it’s time for another portion of the installation fun, now about the IIS web sites.

The IIS extension in WiX is probably the most tricky and unobvious. That’s my personal impression, of course. But, anyway, it gives you an option to tweak any property of a website, virtual directory or web directory.

When installing a web application on Windows XP and thus IIS 5.1, it is natural to create an “ad hoc” virtual directory during install and remove it on uninstall. That’s basically quite common case, but what if the application requires to reside under the site root directly, not virtual directory?

In this case the root of the Default Web Site should just be switched to the installation directory - nothing is created on install and nothing is removed on uninstall. Let’s see how this can be done with WiX IIS extension.

The `iis:WebSite` element has two “modes”: if it resides under `Component` element, it is created during install, otherwise it is there just for reference from other elements. Fortunately, it has a special attribute `ConfigureIfExists`. Setting it to `'yes'` avoids an attempt to create a new site, configuring the existent one instead:

```XML
<Component DiskId=”1” Id=”ModifyIISSite5” Guid=”{YOURGUID-2023-4D19-90D2-EE9101C71E44}” Directory=”WebsiteFolder” Permanent=”yes”>
    <Condition>IISMAJORVERSION = “#5”</Condition>
    <iis:WebSite Id=”IISSite5” Description=”[IISSITE_NAME]” Directory=”WebsiteFolder” ConfigureIfExists=”yes”>
        <iis:WebAddress Id=”IISSiteAddress5” Port=”[IISSITE_PORT]” />
    </iis:WebSite>
</Component>
```

Note, that in this case you should make sure you specified the existent website data. The website is uniquely identified by the description, port and header. The first is an attribute of a WebSite element itself, others belong to the child mandatory element `WebAddress`.

The previous snippet highlights another attribute as bold - `Permanent=”yes”`. It makes the hosting component [permanent](http://msdn.microsoft.com/en-us/library/aa369530(VS.85).aspx), thus preventing it from being deleted on uninstall. Internally, the Windows Installer engine just keeps an extra reference to this component forever, thus it reference count is never equal to 0.

One last thing I’d like to point your attention to is a component condition. It uses the property called `IISMAJORVERSION`. This property, as well as another one called `IISMINORVERSION`, is brought by the IIS extension. They are populated from the target system registry during the AppSearch action. Before using them in your authoring make sure you add a couple of references:

```XML
<PropertyRef Id=”IISMAJORVERSION” />
<PropertyRef Id=”IISMINORVERSION” />
```

That’s it! As usual, any comments are highly appreciated.
