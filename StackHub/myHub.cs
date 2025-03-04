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

        // Función para unirse a un grupo
        public async Task<RoomJoinResult> JoinRoom(string roomName, string playerName, bool creator)
        {
            bool playerJoined = false;
            String message = "";
            RoomJoinResult roomJoinResult = new RoomJoinResult();
            string connectionId = Context.ConnectionId;

            // Comprobar si el grupo existe
            if (!_rooms.ContainsKey(roomName))
            {
                // Si es el creador, crea el grupo
                if (creator) {
                    _rooms[roomName] = new clsRoom { Name = roomName };

                    // Añadimos el jugador al grupo
                    _rooms[roomName].Players[connectionId] = playerName;

                    await Groups.AddToGroupAsync(connectionId, roomName);

                    playerJoined = true;
                } else
                {
                    message = "La sala no existe";
                }
            } else
            {
                // Comprobamos que el otro jugador no se llame igual
                String opponentName =  await GetOpponentName(roomName);

                if (!opponentName.ToLower().Equals(playerName.ToLower()))
                {
                    // Comprobar si ya hay dos jugadores en el grupo
                    if (_rooms.ContainsKey(roomName) && _rooms[roomName].Players.Count < 2)
                    {
                        // Añadir el jugador al grupo
                        _rooms[roomName].Players[connectionId] = playerName;
                        Console.WriteLine($"{playerName} joined room {roomName}.");

                        await Groups.AddToGroupAsync(connectionId, roomName);

                        playerJoined = true;

                        // Comprobar si el grupo ahora tiene dos jugadores
                        await CheckRoomPlayerCount(roomName);
                    }
                    else
                    {
                        message = "El grupo ya está completo";
                    }
                } else
                {
                    playerJoined = false;
                    message = "El otro jugador se llama igual";
                }
            }

            roomJoinResult.Success = playerJoined;
            roomJoinResult.Message = message;
            return roomJoinResult;
        }

        // Función para comprobar la cantidad de jugadores en un grupo
        public async Task CheckRoomPlayerCount(string roomName)
        {
            if (_rooms.ContainsKey(roomName) && _rooms[roomName].Players.Count == 2)
            {
                // Cuando haya dos jugadores, enviar una señal a todos los jugadores del grupo
                Console.WriteLine($"Room {roomName} has two players. Sending signal.");
                await Clients.Group(roomName).SendAsync("PlayersReady", roomName); // Enviar la señal de que los jugadores están listos
            }
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

        public async Task<string?> GetOpponentName(string roomName)
        {
            string opponentName = "";
            string connectionId = Context.ConnectionId;

            if (_rooms.ContainsKey(roomName))
            {
                // Buscar el oponente en la misma sala
                foreach (var player in _rooms[roomName].Players)
                {
                    if (player.Key != connectionId)
                    {
                        // Devolver el nombre del oponente
                        opponentName = player.Value;
                    }
                }
            }

            return opponentName; // Si no hay oponente, retorna null
        }

        public async Task<bool> StopGameAndCheckTurn(string roomName)
        {

            clsRoom room = _rooms[roomName];

            // Obtener el jugador actual y el oponente
            string currentTurn = room.CurrentTurn;
            string opponent = room.Players.Keys.FirstOrDefault(p => p != currentTurn);

            if (opponent == null)
            {
                return false; // No hay oponente
            }

            // Cambiar turno al otro jugador
            room.CurrentTurn = opponent;

            // Notificar a los jugadores sobre el cambio de turno
            await Clients.Group(roomName).SendAsync("TurnChanged", room.CurrentTurn);

            return true; // Se ha detenido el juego y cambiado el turno
        }

        public Task<string?> GetCurrentTurnPlayer(string roomName)
        {
            if (_rooms.ContainsKey(roomName) && _rooms[roomName].CurrentTurn != null)
            {
                return Task.FromResult<string?>(_rooms[roomName].CurrentTurn);
            }
            return Task.FromResult<string?>(null);
        }

    }
}
