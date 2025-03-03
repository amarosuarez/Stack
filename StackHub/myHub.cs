using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using Models;

namespace StackHub
{
    public class myHub : Hub
    {
        // Diccionario para almacenar los grupos
        private static ConcurrentDictionary<string, clsRoom> _rooms = new();

        public async Task JoinRoom(string roomName, string playerName)
        {
            string connectionId = Context.ConnectionId;

            if (!_rooms.ContainsKey(roomName))
            {
                _rooms[roomName] = new clsRoom { Name = roomName };
                Console.WriteLine($"Room {roomName} created.");
            }

            _rooms[roomName].Players[connectionId] = playerName;

            await Groups.AddToGroupAsync(connectionId, roomName);
        }

        public Task<string?> GetMyRoom()
        {
            string connectionId = Context.ConnectionId;

            Console.WriteLine($"Searching for room of connectionId {connectionId}");

            foreach (var room in _rooms.Values)
            {
                if (room.Players.ContainsKey(connectionId))
                {
                    Console.WriteLine($"Player found in room {room.Name}");
                    return Task.FromResult<string?>(room.Name);
                }
            }

            Console.WriteLine("Player not found in any room.");
            return Task.FromResult<string?>(null);
        }



        public async Task LeaveRoom(string roomName)
        {
            string connectionId = Context.ConnectionId;

            if (_rooms.ContainsKey(roomName))
            {
                _rooms[roomName].Players.Remove(connectionId);

                if (_rooms[roomName].Players.Count == 0)
                {
                    _rooms.TryRemove(roomName, out _);
                }
            }

            await Groups.RemoveFromGroupAsync(connectionId, roomName);
        }

        public async Task ReceiveNamePlayer(string playerName, string roomName)
        {
            await Clients.Group(roomName).SendAsync("ReceiveNamePlayer", playerName);
        }

    }
}
