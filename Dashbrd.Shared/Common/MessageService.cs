using System;

namespace Dashbrd.Shared.Common;

public class MessageService
{
    public event Action<object> OnMessage;

    public void SendMessage(object message)
    {
        OnMessage?.Invoke(message);
    }
}