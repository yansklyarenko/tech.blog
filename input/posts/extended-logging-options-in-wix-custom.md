---
Title: "Extended logging options in WiX custom actions"
Published: 2009-02-02 22:46
Tags:
    - MSI
    - CA
    - WiX
    - log
    - Windows Installer
    - custom action
    - verbose
RedirectFrom: "blog/2009/02/02/extended-logging-options-in-wix-custom"
---

The best and maybe the only way to find out what’s going wrong with the installation is investigating the MSI log file. Fortunately, the Windows Installer respects log writing very much. You can find the ways to enable logging and different logging options [here](http://msdn.microsoft.com/en-us/library/aa370536(VS.85).aspx).

The verbose log contains all the information MSI can generate. Though it is really useful when it comes to troubleshooting the failed installation, it is quite hard to read, especially for newbies. Again, fortunately, there’s a super chapter “Using the Windows Installer log” in a super book called [“The Definitive Guide to Windows Installer” by Phil Wilson](http://www.amazon.com/Definitive-Guide-Windows-Installer-Experts/dp/1590592972), which guides you through the basics of log file reading and understanding.

I used to generate the log file with `/L*v` options, which means verbose. But, if you use WiX custom actions, it turns out that you can make them logging even more verbose.

If you browse the WiX source code, you can find the lines like this in its custom actions:

```csharp
WcaLog(LOGMSG_STANDARD, "Error: Cannot locate User.User='%S'", wzUser);
```

The first argument is a logging level. It can be

- `LOGMSG_STANDARD`, which is “write to log whenever informational logging is enabled”, which in most cases means “always”
- `LOGMSG_TRACEONLY`, which is “write to log if this WiX build is a DEBUG build” (is often used internally to dump `CustomActionData` contents)
- `LOGMSG_VERBOSE`, which is “write to log when `LOGVERBOSE`”

Wait a minute, what does the last option means? I’ve already set the verbose logging by `/L*v`, but I don’t see more entries there. Here is the algorithm WiX CA use to know whether to write a log entry marked as `LOGMSG_VERBOSE` level:

- Check if the `LOGVERBOSE` property is set (can be set in the command-line since it is public)
- Otherwise, check if the `MsiLogging` property is set (MSI 4.0+)
- Otherwise, check the logging policy in the registry

So, the following is the easiest way in my opinion to make your MSI (WiX-based) log file even more verbose:

```BAT
msiexec /i package.msi … LOGVERBOSE=1 /L*v install.log
```

Hope this helps.

P.S. This is just a brief extract of what’s there in the source code. As usual, code is the best documentation ;-)
