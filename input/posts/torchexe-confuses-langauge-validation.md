---
Title: "Torch.exe confuses the language validation and ProductCode validation"
Published: 2010-05-07 19:34
Tags:
    - MSI
    - torch
    - WiX
    - Windows Installer
RedirectFrom: "blog/2010/05/07/torchexe-confuses-langauge-validation"
---

This week I faced with another issue with `torch.exe`. As you might know, there's a `type` option (`-t`) to apply a predefined set of validation flags to the generated transform. If you'd like to generate a language transform, you should use `-t language`. It should suppress all the errors plus validate that language in both MSI packages corresponds. But it doesn’t...

The reason is just a simple bug in the tool. When you set `-t language` in the command line, this option is mapped to the `TransformFlags.LanguageTransformDefault` value. It is a combination of atomic values (those you can set via `–serr` and `-val`), and it mistakenly takes "validate product code" bit instead of "validate language bit". I've never noticed this unless my installation uses both instance transforms and language transforms.

The workaround is quite simple: use literally `–serr` and `–val` to achieve the same result. For instance, for language transform it should be:

```BAT
torch.exe … –serr a –serr b –serr c –serr d –serr e –serr f –val l ...
```

[By the way, does it look too long just for me? I would prefer `–serr abcdef` :-)]

I've also filed [an issue](https://sourceforge.net/tracker/?func=detail&aid=2998229&group_id=105970&atid=642714) to the WiX toolset. Hope this can help somebody.
