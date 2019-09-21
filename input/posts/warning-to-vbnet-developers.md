---
Title: "A warning to VB.NET developers"
Published: 2009-08-16 23:43
Tags:
    - design guidelines
    - VB.NET
    - .NET
RedirectFrom: "blog/2009/08/16/warning-to-vbnet-developers"
---

Avoid defining methods with default arguments!

Today I have been exploring the “Member design” chapter of a great book of [Cwalina](http://blogs.msdn.com/kcwalina/)/[Abrams](http://blogs.msdn.com/brada/) "[Framework Design Guidelines](http://www.amazon.com/Framework-Design-Guidelines-Conventions-Development/dp/0321545613/ref=dp_ob_title_bk)", and found a guideline which shocked me a bit. No, the guideline itself is correct and valuable. I just was never thinking it works like this.

VB.NET has a language feature called default arguments. When you define a method in your class, you can specify default values to the optional parameters to be taken when this parameter is omitted. As far as I understand, this is a kind of alternative to the method overloading.

Consider the following code:

```VB.NET
Public Class DefaultValues
    Public Function Sum(ByVal a As Integer, Optional ByVal b As Integer = 100)
        Sum = a + b
    End Function
End Class
```

(I speak C# myself, so excuse me my poor VB ;-))

Let’s say we compile this code into a DLL and we have a client console application to utilize that library:

```VB.NET
Module TestDefaultValues
    Sub Main()
        Dim df As DefaultValues.DefaultValues = New DefaultValues.DefaultValues()
        Console.WriteLine(df.Sum(55))
    End Sub
End Module
```

Compile everything and run TestDefaultValues.exe. The result is predictable: **155**.

Now change the default value from 100 to 200 and compile only the library. DO NOT recompile the client application. Run it again, and it is still **155**!

This is why it is strongly not recommended to use default arguments instead of normal method overloading. And this issue is why C# doesn’t expose this technique.

Be careful, VB.NET developers! And long live C#! :-)
