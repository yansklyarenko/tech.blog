---
Title: "NAnt <copy> task behaves differently in 0.92 and prior versions"
Published: 2013-01-03 11:35
Tags:
    - NAnt
RedirectFrom: "blog/2013/01/03/nant-task-behaves-differently-in-092"
---

If you need to copy a folder together with all its contents to another folder in NAnt, you would typically write something like this:

```XML
<copy todir="${target}">
  <fileset basedir="${source}" />
</copy>
```

It turns out this code works correctly in NAnt 0.92 Alpha and above. The output is expected:

> [copy] Copying 1 directory to '...'.

However, the same code doesn't work in prior versions of NAnt, for instance, 0.91. The output is as follows (only in `–debug+` mode):

> [copy] Copying 0 files to '...'.

Obviously, [the issue was fixed in 0.92](https://github.com/nant/nant/issues/11), so the best recommendation would be to upgrade NAnt toolkit. However, if this is not an option for some reason, the following code seems to work correctly for any version:

```XML
<copy todir="${target}">
  <fileset basedir="${source}">
    <include name="**/*" />
  </fileset>
</copy>
```

Hope this saves you some time.
