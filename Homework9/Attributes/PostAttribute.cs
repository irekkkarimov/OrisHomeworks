namespace Homework9.Attributes;

public class PostAttribute : Attribute, IHttpMethodAttribute
{
    public string ActionName { get; set; }

    public PostAttribute(string actionName)
    {
        ActionName = actionName;
    }
}
