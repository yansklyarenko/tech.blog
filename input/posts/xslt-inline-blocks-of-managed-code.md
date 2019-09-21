---
Title: "XSLT: inline blocks of managed code"
Published: 2009-07-19 23:24
Tags:
    - WiX
    - heat
    - sitecore
    - XSLT
RedirectFrom: "blog/2009/07/19/xslt-inline-blocks-of-managed-code"
---

It’s not a secret that XSLT supports [blocks of code](http://www.w3.org/TR/xslt11/#define-extension-functions), written in another language, to be used inside the stylesheet. It seems to have been there from the very beginning – at least, [XSLT 1.1](http://www.w3.org/TR/xslt11/) understands it.

However, Microsoft enriched this option with their own element, [msxsl:script](http://msdn.microsoft.com/en-us/library/ms256042.aspx). It offers pretty much the same functionality, but you can also write the code in C# or any other language of .NET platform. XSLT gurus might argue that it is superfluous stuff and it is unnecessary in 99% of cases. Well, as for me, XSLT lacks a number of useful functions in the standard library, such as ToLower/ToUpper, EndWith, etc. You never think about such low level things when programming C#, but you often have to invent a wheel trying to do the same with XSLT.

More details can be found in the [official documentation](http://msdn.microsoft.com/en-us/library/wxaw5z5e.aspx), but here is a brief extract:

- guess an extra prefix and let XSLT processor know about it (pay attention how msxsl prefix is defined – it is required to use msxsl:script syntax):

```XML
    <xsl:stylesheet version="1.0"
        xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt"
        xmlns:ext="http://my.domain.com/ext">
        ...
    </xsl:stylesheet>
```

- code your extension function:

```XML
    <msxsl:script language="C#" implements-prefix="ext">
        public string ToUpper(string inString)
        {
            return inString.ToUpper();
        }
    </msxsl:script>
```

- and finally use it:

```XML
    <xsl:value-of select="ext:ToUpper(@Name))"/>
```

Obviously, it is not a good idea to write lots of code this way. It makes the XSLT stylesheet larger and a bit harder to maintain. And, according to Microsoft, you should "[avoid script blocks from XSLT files, because they require loading the script engine multiple times](http://msdn.microsoft.com/en-us/library/ms256042.aspx)". Actually, if you created an XSLT stylesheet to fill it with tones of .NET code, you're definitely doing something wrong. But it seems to be good addition to small, but useful "one-line" operations.

## Sitecore and msxsl:script

If you plan to take advantage of inline blocks of C# code in Sitecore XSL rendering, you'll have to do one more step. By default, .NET API to handle the XSL transforms disables the possibility to use `msxsl:script`. It is probably done for security reason. But the web.config of your Sitecore solution contains the setting `EnableXslScripts`, which you can easily set to true and be happy:

```XML
<!–  ENABLE XSLT SCRIPTS
      Determine whether XSLT script support should be enabled.
      If script support is not enabled, it will be an error if the XSLT file contains script blocks.
      Default value: false.
–>
<setting name="EnableXslScripts" value="true" />
```

The performance seems to be the same for this simple code either written in msxsl:script block, or wrapped into [XSL extension](http://sdn.sitecore.net/upload/sitecore6/61/presentation_component_cookbook_sc61_a4.pdf). So, the choice is yours.

## WiX and msxsl:script

The [heat.exe](http://wix.sourceforge.net/manual-wix3/heat.htm) utility of the WiX toolset has an option to run the harvested authoring against XSLT transform. This is a checkpoint when you can mutate the output before it is done. INHO, it is the most powerful extension option of Heat, because you can do anything with the XML fragment in XSLT.

However, it was a bit disappointing to find out the scripts are disabled by default, and it is not customizable, and the easiest way to fix this is to patch Heat itself and prepare custom WiX build. It would be great if this option is available one day in the base, either as a command line argument, or a configuration setting.

That’s it. If you have some experience with this trick, knowing its pros and cons deeper, share it here. And as usual, any comments are welcome.
