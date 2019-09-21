---
Title: "Torch.exe throws scary error message unrelated to the real problem"
Published: 2010-04-14T15:35:00+03:00
Tags:
    - MSI
    - torch
    - WiX
    - Windows Installer
RedirectFrom: "blog/2010/04/14/torchexe-throws-scary-error-message"
---

Today I've been working on the localization of my installation project, and I had to create a number of language transforms. The following simple call of `torch.exe`

```BAT
torch -t language setup.msi setup_ru-ru.msi -out mst\ru-ru.mst
```

returned the scary error message:

> error TRCH0001 : The Windows Installer service failed to start. Contact your support personnel

I've seen this kind of errors a couple of times, and it was a serious problem with Windows Installer engine on the target machine in all cases. Once, it indicated that Windows Installer service is completely broken, and only OS reinstall helped (fortunately, it was virtual PC)... But mighty Google gave [a single, but exact hint](http://blogs.msdn.com/pmarcu/archive/2008/05/30/Patching-something-you-didnt-build-with-WiX-using-WiX-.aspx#8920333). It is just a single line, and one can miss the point since that's another problem which is discussed there.

So, the actual problem: if `–out` switch points to a folder which doesn't exist (`mst` in this case), `torch.exe` can't create it and returns the error. That's okay behavior to live with, but the error message should be changed to something more appropriate: *The folder `mst` can't be found. Make sure it exists before referencing in `–out` switch*. I've also created [an issue](https://sourceforge.net/tracker/?func=detail&aid=2987095&group_id=105970&atid=642714) to the WiX inbox at sourceforge.net.

Hope this info is helpful until the message text is fixed.
