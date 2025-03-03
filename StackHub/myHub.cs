using Microsoft.AspNetCore.SignalR;

namespace StackHub
{
    public class myHub : Hub
    {

        public Task JoinRoom(string roomName, String playerName)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }

        public Task LeaveRoom(string roomName)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }
    }
}
