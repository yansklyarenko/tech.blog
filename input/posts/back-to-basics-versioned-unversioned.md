---
Title: "Back to basics: Versioned, Unversioned and Shared fields"
Published: 2010-09-10 02:23
Tags:
    - sitecore
RedirectFrom: "blog/2010/09/10/back-to-basics-versioned-unversioned"
---

It is well-known that each field of a template can be versioned (default option), unversioned or shared. The Template Builder UI exposes the `Unversioned` and `Shared` properties as two independent checkboxes. And thus, despite it's a very basic Sitecore concept, it is sometimes asked [what’s the point of marking a field both shared and unversioned](http://sdn.sitecore.net/forum//ShowPost.aspx?PostID=29034). The answer is "a field marked both shared and unversioned is still a shared field". Think about "shared" as a superset of "unversioned" – the field can't be shared (between all versions of all languages) without being unversioned (between all versions of one language).

Let’s see how it works under the hood when the field "sharing" level is changed. Let's create a simple template with just a single field. We’ll keep the defaults so far (versioned). Now create a content item based on this template and fill in the field.

Sitecore fields are stored in three different tables inside the database: `VersionedFields`, `UnversionedFields` and `SharedFields`. The names are quite self-explanatory. Let's run the following SQL query:

```SQL
SELECT * FROM VersionedFields WHERE FieldId = '{GUID-GOES-HERE-...}'
```

As a result, one record is returned – the field information of the item we’ve just created is stored in the VersionedFields table. The similar queries for UnversionedFields and SharedFields give 0 records.

Now change the field to be Unversioned and run all 3 queries again – it will return 1 record for UnversionedFields table and 0 for others. Change the field to be both Shared and Unversioned and repeat the experiment – the field info now resides in SharedFields table. Now if you uncheck Unversioned and leave it just Shared, it will still show 1 record for SharedFields table and 0 for others. So, here’s the evidence!

NOTE: changing the field “sharing” level might result in a data loss (similar to type cast operation in C#), and Sitecore warns you about it.

You might think that two checkboxes are to be blamed for this confusion. Check out the hot VS extension called [Sitecore Rocks](http://visualstudiogallery.msdn.microsoft.com/en-us/44a26c88-83a7-46f6-903c-5c59bcd3d35b/view) – a brand new tool (CTP for now) for developers working with Sitecore projects in VS 2010. It seems to look more natural in this way, isn’t it?

![Sitecore Rocks](./images/201009_SCRocks.png "Sitecore Rocks")
