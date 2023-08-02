namespace B1ServiceLayer.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class SAPEntityAttribute : Attribute
{
    public string ResourceName { get; set; }

    public SAPEntityAttribute(string resourceName)
    {
        ResourceName = resourceName;
    }
}
