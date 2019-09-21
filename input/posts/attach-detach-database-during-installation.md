---
Title: "Attach / Detach database during installation"
Published: 2009-01-10 18:19
Tags:
    - MSI
    - WiX
    - detach
    - Windows Installer
    - attach
    - SQL Server
RedirectFrom: "blog/2009/01/10/attach-detach-database-during"
---

It seems I have finally managed to implement full database support in my installation program. And it also seems that I stepped on every rake one could imagine in this area. But, the harder the battle, the sweeter the victory.

I had the following requirements: the application is distributed with the MDF/LDF files, which must be attached during installation and detached during uninstallation. Both Windows and SQL authentication must be supported.

Fortunately, the kind WiX developers implemented a wonderful SQL extension. So, let’s take advantage of the `sql:SqlDatabase` element. The documentation says, it can be placed either under Component, or under Fragment/Module/Product. In the first case the database will always be created when the component is being installed. This doesn’t suite our needs with attach, so let’s stick with another option:

```XML
<fragment>
    <sql:sqldatabase id=”SqlMasterDBWinAuth” server=”[SQL_SERVER]” database=”master” />
</fragment>
```

As you can see, we specify the standard Master database in this element. That’s because the database must exist on the target computer by the moment Windows Installer tries to connect. This syntax will instruct the custom action to open the connection using currently logged-on Windows account.

The next step is to provide the appropriate `sql:String` elements for attach/detach. It is better to put these elements inside the component which installs MDF/LDF files, but this is not the rule. And if you have different conditions for installing the files and running attach, you’ll have to move the scripts into a separate component.

```XML
<Component DiskId=”1” Id=”MSSQLCore” Guid=”YOURGUID-4E94-4B28-B995-DCBFD50B9F07”>
    <Condition>YOUR CONDITION GOES HERE</Condition>

    <File Id=”MSSQLCoreFile” Name=”$(var.CoreFileName)” KeyPath=”yes” />
    <File Id=”MSSQLCoreLogFile” Name=”$(var.CoreFileLogName)” />

    <sql:SqlString Id=”DetachCore” Sequence=”1” ContinueOnError=”yes” ExecuteOnUninstall=”yes” SqlDb=”SqlMasterDBWinAuth” SQL=”EXEC master.dbo.sp_detach_db @dbname = N’[INSTANCENAME]Core’, @skipchecks=N’true’” />

    <sql:SqlString Id=”AttachCore” Sequence=”2” ContinueOnError=”no” ExecuteOnInstall=”yes” SqlDb=”SqlMasterDBWinAuth” SQL=”CREATE DATABASE [\[][INSTANCENAME]Core[\]] ON ( FILENAME = N’[DB_FOLDER]$(var.CoreFileName)’ ), ( FILENAME = N’[DB_FOLDER]$(var.CoreFileLogName)’ ) FOR ATTACH” />

</Component>
```

At this point I should mention one reef. An `SqlString` string element also has an attribute `SQLUser`. If you provide both `SqlDb` attribute, pointing to the "WinAuth" definition of the database, and `SqlUser` attribute, pointing to the “sa user”, it will lead to unpredictable and very strange behavior. I would avoid this.

Ok, now we should take care about the rollback actions: during install and uninstall correspondently. It is obvious that `RollbackOnInstall` should detach databases, if they got installed before failure, and `RollbackOnUnistall` should attach the databases back, if the failure occurred during uninstall.

Thanks to the hint of Rob Mensching in [one of his replies to the WiX mailinglist](http://www.mail-archive.com/wix-users@lists.sourceforge.net/msg18212.html), I managed to overcome another trick. Right after the database is attached, there is sometimes a connection left to this database. I can see this by opening the SQL Management studio and looking at the database status (Normal). If you detach the database in this moment, it flushes the permissions on a physical file to a logon account only. I didn’t dig very deep into this, it probably corresponds to the [rules of permissions change during attach/detach](http://msdn.microsoft.com/en-us/library/ms189128.aspx). As a result, the windows installer can’t access the file afterwards, and the uninstallation is rolled back.

To fix this, perform `SET OFFLINE` query before detaching the database and you’ll never face with this behavior again.

Thus, the final version will look similar to this:

```XML
<Component DiskId=”1” Id=”MSSQLCore” Guid=”YOURGUID-4E94-4B28-B995-DCBFD50B9F07”>
    <Condition>YOUR CONDITION GOES HERE</Condition>

    <File Id=”MSSQLCoreFile” Name=”$(var.CoreFileName)” KeyPath=”yes” />
    <File Id=”MSSQLCoreLogFile” Name=”$(var.CoreFileLogName)” />

    <sql:SqlString Id=”RollbackDetachCore” Sequence=”1” ContinueOnError=”yes” RollbackOnUninstall=”yes” SqlDb=”SqlMasterDBWinAuth” SQL=”CREATE DATABASE [\[][INSTANCENAME]Core[\]] ON ( FILENAME = N’[DB_FOLDER]$(var.CoreFileName)’ ), ( FILENAME = N’[DB_FOLDER]$(var.CoreFileLogName)’ ) FOR ATTACH” />
    <sql:SqlString Id=”OfflineCoreDatabase” Sequence=”2” ContinueOnError=”yes” ExecuteOnUninstall=”yes” SqlDb=”SqlMasterDBWinAuth” SQL=”ALTER DATABASE [\[][INSTANCENAME]Core[\]] SET OFFLINE WITH ROLLBACK IMMEDIATE” />
    <sql:SqlString Id=”DetachCore” Sequence=”3” ContinueOnError=”yes” ExecuteOnUninstall=”yes” SqlDb=”SqlMasterDBWinAuth” SQL=”EXEC master.dbo.sp_detach_db @dbname = N’[INSTANCENAME]Core’, @skipchecks=N’true’” />
    <sql:SqlString Id=”RollbackAttachCore” Sequence=”4” ContinueOnError=”yes” RollbackOnInstall=”yes” SqlDb=”SqlMasterDBWinAuth” SQL=”EXEC master.dbo.sp_detach_db @dbname = N’[INSTANCENAME]Core’, @skipchecks=N’true’” />
    <sql:SqlString Id=”AttachCore” Sequence=”5” ContinueOnError=”no” ExecuteOnInstall=”yes” SqlDb=”SqlMasterDBWinAuth” SQL=”CREATE DATABASE [\[][INSTANCENAME]Core[\]] ON ( FILENAME = N’[DB_FOLDER]$(var.CoreFileName)’ ), ( FILENAME = N’[DB_FOLDER]$(var.CoreFileLogName)’ ) FOR ATTACH” />

</Component>
```

Ok, but what about Sql Authentication? Well, this requires some kind of duplicating the code. The `SqlDb` attribute of the `SqlString` element can’t accept MSI properties, thus can’t be dinamically changed during runtime. We must author another element SqlDatabase for referencing it from another set of scripts.

```XML
<util:User Id=”SQLUser” Name=”[SC_SQL_SERVER_USER]” Password=”[SC_SQL_SERVER_PASSWORD]” />
<sql:SqlDatabase Id=”SqlMasterDBWinAuth” Server=”[SC_SQL_SERVER]” Database=”master” />
<sql:SqlDatabase Id=”SqlMasterDBSqlAuth” Server=”[SC_SQL_SERVER]” Database=”master” User=”SQLUser” />
```

The first element defines a user to connect to the database. In this example, the username and password are read from the public properties. The user is not created, it is just referenced. The second element should be familiar - it was described above. And the last one differs only in one attribute - `SQLUser`.
This does the trick: if you want Windows Authentication way to use, reference `SqlMasterDBWinAuth` in your scripts, otherwise - use `SqlMasterDBSqlAuth`. Obviously, you need another set of the similar `SqlString` elements in a different component.

Tired? The last thing.

If you implemented something similar to what I’ve described, you should have mentioned that in case of Sql Auth the database is attached as read-only. This happens because the SQL service account (`NETWORK SERVICE` in my case) doesn’t have enough permissions to the `[DB_FOLDER]` and files by the moment attach starts.
No problem, let’s assign the necessary rights. Put the following snippet into your component which contains the `SqlAuth` scripts:

```XML
<CreateFolder>
    <util:PermissionEx GenericAll=”yes” User=”NetworkService” />
</CreateFolder>
```

Note: Don’t forget to reference `WIX_ACCOUNT_NETWORKSERVICE` property.

But, wait, the `ShedSecureObjects` is scheduled after the InstallSqlData, this doesn’t help!
Right, the sequence should also be changed like this:

```XML
<InstallExecureSequence>
    <Custom Action=”InstallSqlData” After=”SchedSecureObjects”>NOT SKIPINSTALLSQLDATA AND VersionNT > 400</Custom>
</InstallExecuteSequence>
```

That’s it! I know, this can’t seem easy at first glance, but, as for me, it is much more controlled and customizable, than with InstallShield. I might be wrong, though.

Good luck! I would appreciate any comments and notes to this.
