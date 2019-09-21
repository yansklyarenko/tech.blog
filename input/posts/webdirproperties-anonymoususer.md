---
Title: "WebDirProperties: AnonymousUser attribute is not obligatory"
Published: 2009-08-18 23:46
Tags:
    - WiX
    - IIS extension
RedirectFrom: "blog/2009/08/18/webdirproperties-anonymoususer"
---

When you specify a `WebDirProperties` element to be used by the sites you install (configure) with WiX, you might also want to allow anonymous access to this site. Fortunately, there's an attribute `AnonymousAccess`, which being set to 'yes' allows anonymous access to IIS web site.

NOTE: If you don't address any property of "authorization" group (`AnonymousAccess`, `BasicAuthentication`, `DigestAuthentication`, `PassportAuthentication` or `WindowsAuthentication`) in your `WebDirProperties`, the site inherits those from w3svc root. If you set at least one explicitly, you need to set others the way you wish, because WiX defaults might not work for you.

The wix.chm states that "[When setting this (`AnonymousAccess`) to 'yes' you should also provide the user account using the AnonymousUser attribute, and determine what setting to use for the `IIsControlledPassword` attribute.](http://wix.sourceforge.net/manual-wix3/iis_xsd_webdirproperties.htm)" But it turns out you are not forced to provide the `AnonymousUser` attribute and I suppose you never wanted to – you should provide a password as well, but who knows the password of `IUSR` on a target machine?

Instead, just omit the `AnonymousUser` attribute and this part of IIS metabase will stay untouched. The username/password will inherit from higher node (again, w3svc). And yes, don't forget `IIsControlledPassword="yes"`.

Hope this helps you tuning the website during the installation.
