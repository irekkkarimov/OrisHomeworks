using System.Net;

namespace Homework8.Handlers;

public abstract class Handler
{
    public Handler Successor { get; set; }
    public abstract void HandleRequest(HttpListenerContext context);
}