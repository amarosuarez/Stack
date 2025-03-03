using Microsoft.AspNetCore.SignalR;

namespace StackHub
{
    public class myHub : Hub
    {

        public Task JoinRoom(string roomName, string playerName)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }

        public Task LeaveRoom(string roomName)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }

        public async Task ReceiveNamePlayer(string playerName, string roomName)
        {
            await Clients.Group(roomName).SendAsync("ReceiveNamePlayer", playerName);
            //await Clients.All.SendAsync("ReceiveMessage", mensajeUsuario);
        }
    }
}
