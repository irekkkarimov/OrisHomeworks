namespace Homework8.Attributes;

public class ControllerAttribute : Attribute
{
    public string ControllerName { get; set; }

    public ControllerAttribute(string controllerName)
    {
        ControllerName = controllerName;
    }
}