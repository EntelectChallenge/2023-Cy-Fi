using Microsoft.AspNetCore.SignalR.Client;

namespace Engine.Communication;

public interface GameController
{
    public void Setup(HubConnection connection);
}