---
Title: "Moving to dotNetInstaller: the odd Basic UI"
Published: 2011-02-24 18:36
Tags:
    - install
    - dotNetInstaller
    - bootstrapper
RedirectFrom: "blog/2011/02/24/moving-to-dotnetinstaller-odd-basic-ui"
---

In the [previous post](./moving-to-dotnetinstaller-launch), I've outlined how to emulate the launch conditions behavior in dotNetInstaller. In that article I have also emphasized the importance of turning the UI into the Basic mode. It is necessary in order to avoid extra dialogs which require user interaction. If you followed the scenario I described, you might notice a strange behavior of the `BasicUI` mode: **the message boxes disappear without any user participation**. I thought it's be a kind of a bug, but it was done on purpose. Take a look at this code (taken from dotNetInstaller sources):

```csharp
int DniMessageBox::Show(const std::wstring& p_lpszText, UINT p_nType /*=MB_OK*/, UINT p_nDefaultResult /*=MB_OK*/, UINT p_nIDHelp /*=0*/)
{
   int result = p_nDefaultResult;
   switch(InstallUILevelSetting::Instance->GetUILevel())
   {
   // basic UI, dialogs appear and disappea
   case InstallUILevelBasic:
       {
           g_hHook = SetWindowsHookEx(WH_CBT, CBTProc, NULL, GetCurrentThreadId());
           CHECK_WIN32_BOOL(NULL != g_hHook, L"Error setting CBT hook");
           result = AfxMessageBox(p_lpszText.c_str(), p_nType, p_nIDHelp);
           CHECK_BOOL(0 != result, L"Not enough memory to display the message box.");
           if (result == 0xFFFFFF) result = p_nDefaultResult;
       }
       break;

   // silent, no UI
   case InstallUILevelSilent:
       result = p_nDefaultResult;
       break;

   // full UI
   case InstallUILevelFull:
   default:
       result = AfxMessageBox(p_lpszText.c_str(), p_nType, p_nIDHelp);
       break;
   }

   return result;
}
```

So, as you can see, in Basic mode is shows the message box, and after some time (if you didn't catch the moment to press any button), it automatically emulates the pressing of default choice button. I was quite surprised when I understood it was designed to work like this – that's because I've never seen such a UI behavior…

But, anyway, I suspect that a user would like to know why the installation terminated - a certain prerequisite is not installed. As long as the mentioned behavior is hard-coded, the only option is to create a custom build of dotNetInstaller. It's obvious that the fix is trivial here – make the case for `InstallUILevelBasic` go the same branch as `InstallUILevelFull`, that is, just show the message box. Next step is to build the solution – see *Contributing to Source Code* chapter of dotNetInstaller.chm for instructions how to build.

Finally, install the custom build instead of the official one and make sure your setup project picks the changes up. That's it!

As usual, I would appreciate any comments and notes!
