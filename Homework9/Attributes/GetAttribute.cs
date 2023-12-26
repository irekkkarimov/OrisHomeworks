namespace Homework9.Attributes;

public class GetAttribute : Attribute, IHttpMethodAttribute
{
    public string ActionName { get; set; }

    public GetAttribute(string actionName)
    {
        ActionName = actionName;
    }
}