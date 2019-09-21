---
Title: "A simple batch script to dump the contents of the folder and its subfolders recursively"
Published: 2012-03-02 11:13
Tags:
    - batch script
RedirectFrom: "blog/2012/03/02/simple-batch-script-to-dump-contents-of"
---

This topic might seem too minor for a blog post. You can argue that it's covered by a simple call to a [`dir /s`](http://ss64.com/nt/dir.html) command. Well, that's true unless you need to perform some actions with each line in the list. In this case it could be tricky if you do not use BATCH files on a daily basis.

Imagine you need to dump the file paths in a folder and its subfolders to a plain list. Besides, you'd like to replace the absolute path prefix with UNC share prefix, because each path contains a shared folder and each file will be accessible from inside the network. So, here goes the script:

```BAT
@echo off
set _from=*repo
set _to=\\server\repo
FOR /F "tokens=*" %%G IN ('dir /s /b /a:-D /o:-D') DO (CALL :replace %%G) >> files.txt
GOTO :eof

:replace
 set _str=%1
 call set _result=%%_str:%_from%=%_to%%%
 echo %_result%
GOTO :eof
```

Let’s start from the [`FOR`](http://ss64.com/nt/for_cmd.html) loop. This version of the command loops through the output of another command, in this case, dir. Essentially, we ask dir to run recursively (`/s`), ignore directories (`/a:-D`), sort by date/time, newest first (`/o:-D`) and output just the basic information (`/b`). And the `FOR` command works on top of this, iterating all lines of dir output (`tokens=*`), calling a subroutine `:replace` for each line and streaming the final result into *files.txt*.

The subroutine does a very simple thing – it replaces one part of the string with another. Let's step through it anyway. First, it gets the input parameter (`%1`) and saves it into `_str` variable. I suppose `%1` could be used as is in the expression below, but the number of `%` signs drives me crazy even without it. The next line is the most important – it does the [actual replacement job](http://ss64.com/nt/syntax-replace.html). I'll try to explain all these `%` signs: the variable inside the expression must be wrapped with `%` (like `_from` and `_to`); the expression itself should go between `%` and `%` as if it's a variable itself. And the outermost pair of `%` is there for escaping purpose, I suppose – you will avoid it if you use just string literals for tokens in expression. Note also the usage of the [`CALL SET` statement](http://ss64.com/nt/call.html). Finally, the last line of the subroutine echoes the result.

There's one last point worth attention. The `_from` variable, which represents the token to replace, contains a `*` sign. It means *replace 'repo' and everything before it* in the replace expression.

The best resource I found on the topic is [SS64](http://ss64.com/nt/).
