---
Title: "Default attribute values for custom NAnt tasks"
Published: 2012-08-15 16:35
Tags:
    - .NET
    - C#
    - NAnt
    - custom task
RedirectFrom: "blog/2012/08/15/default-attribute-values-for-custom"
---

When you create custom [NAnt](http://nant.sourceforge.net/) tasks, you can specify various task parameter characteristics, such as whether it is a required attribute, how it validates its value, etc. This is done via the custom attributes in .NET, for example:

```csharp
[TaskAttribute("param", Required = true), StringValidator(AllowEmpty = false)]
public string Param { get; set; }
```

It might be a good idea to be able to specify a default value for a task parameter the similar way, for instance:

```csharp
[TaskAttribute("port"), Int32Validator(1000, 65520), DefaultValue(16333)]
public int Port { get; set; }
```

Let's examine the way it can be implemented. First of all, let's define the custom attribute for the default value:

```csharp
/// <summary>
/// The custom attribute for the task attribute default value
/// </summary>
public class DefaultValueAttribute : Attribute
{
  public DefaultValueAttribute(object value)
  {
    this.Default = value;
  }

  public object Default { get; set; }
}
```

I suppose the [standard .NET `DefaultValueAttribute`](http://msdn.microsoft.com/en-us/library/system.componentmodel.defaultvalueattribute.aspx) can be used for this purpose as well, but the one above is very simple and is good for this sample. Note also that in this situation we could benefit from the generic custom attributes, [which unfortunately are not supported in C#, although are quite valid for CLR](http://stackoverflow.com/questions/294216/why-does-c-sharp-forbid-generic-attribute-types).

Now, when the attribute is defined, let's design the way default values will be applied at runtime. For this purpose we'll have to define a special base class for all our custom tasks we'd like to use default values technique:

```csharp
public abstract class DefaultValueAwareTask : Task
{
  protected override void ExecuteTask()
  {
    this.SetDefaultValues();
  }

  protected virtual void SetDefaultValues()
  {
    foreach (var property in GetPropertiesWithCustomAttributes<DefaultValueAttribute>(this.GetType()))
    {
      var attribute = (TaskAttributeAttribute)property.GetCustomAttributes(typeof(TaskAttributeAttribute), false)[0];
      var attributeDefaultValue = (DefaultValueAttribute)property.GetCustomAttributes(typeof(DefaultValueAttribute), false)[0];

      if (attribute.Required)
      {
        throw new BuildException("No reason to allow both to be set", this.Location);
      }

      if (this.XmlNode.Attributes[attribute.Name] == null)
      {
        property.SetValue(this, attributeDefaultValue.Default, null);
      }
    }
  }

  private static IEnumerable<PropertyInfo> GetPropertiesWithCustomAttributes<T>(Type type)
  {
    return type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).Where(property => property.GetCustomAttributes(typeof(T), false).Length > 0);
  }
}
```

Let's examine what this code actually does. The key method here is `SetDefaultValues()`. It iterates through the task parameters (the public properties marked with `DefaultValueAttribute` attribute) of the class it is defined in and checks whether the value carried by the `DefaultValueAttribute` should be set as a true value of the task parameter. It is quite simple: if the `XmlNode` of the NAnt task definition doesn't contain the parameter in question, it means a developer didn't set it explicitly, and it is necessary to set a default value. Moreover, if the task parameter is marked as `Required` and has a default value at the same time, this situation is treated as not appropriate and the exception is thrown.

Obviously, when a custom NAnt task derives from the `DefaultValueAwareTask`, it has to call `base.ExecuteTask()` at the very start of its `ExecuteTask()` method implementation for this technique to work.
