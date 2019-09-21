---
Title: "Validating the source of TreeList"
Published: 2009-03-22 21:18
Tags:
    - TreeList
    - validation
    - sitecore
RedirectFrom: "blog/2009/03/22/validating-source-of-treelist"
---

Sitecore 6 validation was designed to validate the field values. Recently, I also found it useful to control the source of the complex field types, like TreeList. In this post, I'll explain this option taking the `TreeList` field type as an example.

I'm skipping the validation basics here, since this topic is covered by [Alexey Rusakov](http://alexeyrusakov.com/sitecoreblog/) in his [validation series](http://alexeyrusakov.com/sitecoreblog/2008/07/02/Sitecore+6+Validation+Part+1.aspx).

You can define a number of parameters in the source of `TreeList` field type. The complete list is described in the paragraph 2.4.2 "How to Control the List of Items in a Selection Field" of the [Data Definition cookbook](http://sdn5.sitecore.net/upload/sitecore6/datadefinitioncookbook-a4.pdf). These parameters can filter the available and visible items in the content tree (`IncludeTemplatesForSelection`, `ExcludeItemsForDisplay`, etc.), define the tree root (`DataSource`), control multiple selection (`AllowMultipleSelection`), etc.

But modifying this long list of parameters in a one-line edit field can lead to a simple typos, both in the parameters’ names and values. Let’s examine how this can be “solved” by introducing a source validator.

The `BaseValidator` class, the very root of the validator hierarchy in Sitecore API, has a protected method `GetField()`, which returns an instance of a `Field` - the one we validate. Hence, the `Source` property is also available. We want to validate only complex source here, thus skipping if it is an `ID` or an item path:

```csharp
protected override ValidatorResult Evaluate()
{
    ValidatorResult result = ValidatorResult.Valid;

    Field field = GetField();
    if (field != null)
    {
        string fieldSource = field.Source;
        if (!string.IsNullOrEmpty(fieldSource) && !ID.IsID(fieldSource)
            && !fieldSource.StartsWith("/", StringComparison.InvariantCulture))
        {
            result = EvaluateSourceParameters(fieldSource);
        }
    }

    return result;
}
```

Ok, let’s start the validation from just the verification if the source is "well-formed". It might happen that a certain parameter was left without a value, or a typo was introduced to the well-known name. Sitecore will never throw an error in such a case, but instead you may receive an orphaned field with nothing to choose from. Thus, the simplest validation includes these two checks, otherwise it keeps the name/value pairs for further analysis:

```csharp
ValidatorResult EvaluateSourceParameters(string fieldSource)
{
    SafeDictionary parameters = new SafeDictionary();
    string[] sourceParts = fieldSource.Split('&');
    foreach (string part in sourceParts)
    {
        if (string.IsNullOrEmpty(part))
        {
            continue;
        }
        if (!part.Contains("=") || part.EndsWith("="))
        {
            Text = string.Format("The value is not set for source parameter '{0}'", part.TrimEnd('='));
            return GetFailedResult(ValidatorResult.Error);
        }
        else
        {
            string parameterName = part.Substring(0, part.IndexOf('=')).ToLower();
            if (!sourceParameters.Contains(parameterName))
            {
                Text = string.Format("Unknown source parameter '{0}'", parameterName);
                return GetFailedResult(ValidatorResult.Error);
            }
            else
            {
                string parameterValue = part.Substring(part.IndexOf('=') + 1);
                parameters.Add(parameterName, parameterValue);
            }
        }
    }

    return EvaluateWellFormedParameters(parameters);
}
```

The further validation can go deeper and verify the presence of the specified template or item. The method `EvaluateWellFormedParameters` in this example just iterates the name/value pairs of parameters and applies a certain validation strategy, for instance:

```csharp
ValidatorResult EvaluateTemplates(string value, Database database)
{
    string[] templates = value.Split(new char[] { ',' });
    foreach (string template in templates)
    {
        if (!string.IsNullOrEmpty(template) && Query.SelectSingleItem(string.Format("/sitecore/templates//*[@@key='{0}']", template.ToLower()), database) == null)
        {
            Text = string.Format("The template '{0}' doesn't exist in the '{1}' database", template, database.Name);
            return ValidatorResult.Warning;
        }
    }

    return ValidatorResult.Valid;
}
```

I’m attaching the [full code of this example](https://gist.github.com/yansklyarenko/dd6e0cab47dd93bc8d1f9a1c80c035e5).

There are several notes to consider:

- The `DatabaseName` parameter is not validated, because Sitecore takes over this. Try specifying `DatabaseName=nosuchdb`, and press Save
- The parameter names are case-insensitive. This is because the parameters are extracted with the `StringUtil.ExtractParameter()` method, which ignores the case
- The `TreeList` field type doesn't "tolower" the values of `IncludeItemsForDisplay` and `ExcludeItemsForDisplay` parameters. Hence, be sure to specify an item key instead of an item name here
- The content tree filter is built out of the "ForDisplay" parameters using `and` operation. Thus, if `IncludeItemsForDisplay` contain items of other templates than those specified in `IncludeTemplatesForDisplay`, this results in an empty tree. This can also be a point of extension of this validator's functionality

Hope anyone finds this article useful. As usual, I would appreciate any comments.
