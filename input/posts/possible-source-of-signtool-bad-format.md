---
Title: "Possible source of the signtool 'bad format' 0x800700C1 problem"
Published: 2012-11-19 17:26
Tags:
    - signtool
    - 0x800700C1
    - bad format
RedirectFrom: "blog/2012/11/19/possible-source-of-signtool-bad-format"
---

Today I have faced with a weird problem. The operation to sign the EXE file (actually, an installation package) with a valid certificate failed with the following error:

```LOG
[exec] SignTool Error: SignedCode::Sign returned error: 0x800700C1
[exec] Either the file being signed or one of the DLL specified by /j switch is not a valid Win32 application.
[exec] SignTool Error: An error occurred while attempting to sign: D:\output\setup.exe
```

This kind of error is usually an indication of a format incompatibility, [when the bitness of the signtool.exe and the bitness of the EXE in question don’t correspond](http://technet.microsoft.com/en-us/library/cc782541(WS.10).aspx). However, this was not the case.

It turns out that the original EXE file was generated incorrectly because of the lack of disk space. That's why it was broken and was recognized by the signtool like a bad format file. After disk cleanup everything worked perfectly and the EXE file was signed correctly.

Hope this saves someone some time.
