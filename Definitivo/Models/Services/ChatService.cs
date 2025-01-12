using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Definitivo.Models.Services
{
    public class ChatService
    {

        private readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<string, string> _onlineUsers = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<(string Sender, string Receiver), (bool, bool)> _calls = new ConcurrentDictionary<(string Sender, string Receiver), (bool, bool)>();
        
        //sender + receiver -> senderOnCall + receiverOnCall
        private readonly ConcurrentDictionary<(string Sender, string Receiver), string> _rooms = new ConcurrentDictionary<(string Sender, string Receiver), string>();
        //sender + receiver -> urlRoom

        public ChatService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (bool, bool) GetCallState(string sender, string receiver)
        {
            return _calls.TryGetValue((sender, receiver), out var states) ? states : (false, false);
        }

        // Inicia uma chamada entre dois utilizadores
        public bool StartCall(string sender, string receiver, string urlRoom)
        {
            if (!_rooms.ContainsKey((sender, receiver)))
            {
                _rooms.TryAdd((sender, receiver), urlRoom); // Adicionar room
            }

            // Verifica se já existe uma chamada ativa entre o sender e o receiver
            if (!_calls.ContainsKey((sender, receiver)))
            {
                _calls.TryAdd((sender, receiver), (true, false)); // O receiver ainda não entrou
                Console.WriteLine($"Chamada iniciada: {sender} -> {receiver}");
                return true;
            }

            Console.WriteLine($"Erro: Já existe uma chamada ativa envolvendo {sender} e {receiver}");
            return false;
        }

        // Aceita uma chamada
        public bool AcceptCall(string sender, string receiver)
        {
            if (_calls.TryGetValue((sender, receiver), out var states))
            {
                _calls[(sender, receiver)] =  (true, true); // Se existir uma chamada alguem tem que estar nela, por isso vao estar os dois
                Console.WriteLine($"{receiver} aceitou a chamada de {sender}");
                return true;
            }

            Console.WriteLine($"Erro: Não há chamada ativa para {receiver} de {sender}");
            return false;
        }

        public bool DeleteActiveCalls(string user)
        {
            var calls = _calls.Where(c => c.Key.Sender == user || c.Key.Receiver == user);
            foreach (var call in calls) 
            {
                _calls.TryRemove(call.Key, out _);
            }

            var rooms = _rooms.Where(r => r.Key.Sender == user || r.Key.Receiver == user);
            foreach (var room in rooms)
            {
                DeleteDailyRoom(room.Value);
                _rooms.TryRemove(room.Key, out _);
            }

            return true;
        }

        // Termina uma chamada ou marca como "não ativa"
        public void EndCall(string sender, string receiver, bool senderWantsToLeave = true)
        {
            // Verifica se a chamada existe em ambas as direções
            if (_calls.TryGetValue((sender, receiver), out var states))
            {
                // Atualiza o estado para o caso (user1 -> user2)
                _calls[(sender, receiver)] = senderWantsToLeave == true ? (false, states.Item2) : (states.Item1, false); //se for o sender a querer terminar ou o receiver

                // Verifica se ambos os estados são falsos
                var updatedStates = _calls[(sender, receiver)];
                if (!updatedStates.Item1 && !updatedStates.Item2)
                {
                    _calls.TryRemove((sender, receiver), out _);
                    DeleteDailyRoom(_rooms[(sender, receiver)]);
                    _rooms.TryRemove((sender, receiver), out _);
                    Console.WriteLine($"Chamada completamente terminada entre {sender} e {receiver}");
                }
                else
                {
                    Console.WriteLine($"Estado atualizado na chamada {sender} -> {receiver}: ({updatedStates.Item1}, {updatedStates.Item2})");
                }
                
            }
            else if(_calls.TryGetValue((receiver, sender), out states))
            {
                // Atualiza o estado para o caso (user1 -> user2)
                _calls[(receiver, sender)] = senderWantsToLeave == true ? (states.Item1, false) : (false, states.Item2); //se for o sender a querer terminar ou o receiver

                // Verifica se ambos os estados são falsos
                var updatedStates = _calls[(receiver, sender)];
                if (!updatedStates.Item1 && !updatedStates.Item2)
                {
                    _calls.TryRemove((receiver, sender), out _);
                    DeleteDailyRoom(_rooms[(receiver, sender)]);
                    _rooms.TryRemove((receiver, sender), out _);
                    Console.WriteLine($"Chamada completamente terminada entre {sender} e {receiver}");
                }
                else
                {
                    Console.WriteLine($"Estado atualizado na chamada {sender} -> {receiver}: ({updatedStates.Item2}, {updatedStates.Item1})");
                }
            }
            else
            {
                Console.WriteLine($"Erro: Não há chamada ativa entre {sender} e {receiver}");

            }
        }

        [HttpPost]
        public async Task<bool> DeleteDailyRoom(string roomUrl)
        {
            string ApiKey = _configuration["DailyCo:ApiKey"];
            string DailyApiBaseUrl = _configuration["DailyCo:DailyApiBaseUrl"];

            var roomName = roomUrl.Split('/').Last();
            if(roomName == "salafixe")
            {
                return false;
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
                var response = await client.DeleteAsync($"{DailyApiBaseUrl}/rooms/{roomName}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                return false;
            }
        }

        public async Task<string> GetExistingRoom(string senderUserId, string receiverUserId)
        {
            // Verificar se existe uma sala com os utilizadores
            return _rooms.FirstOrDefault(r =>
                (r.Key.Sender == senderUserId && r.Key.Receiver == receiverUserId) ||
                (r.Key.Sender == receiverUserId && r.Key.Receiver == senderUserId)).Value;
        }

        // Verifica se há uma chamada ativa envolvendo os utilizadores
        public bool CreateRoomByUsers(string user1, string user2, string url)
        {
            return _rooms.TryAdd((user1, user2), url);
        }

        // Verifica se há uma chamada ativa envolvendo os utilizadores
        public bool IsCallActive(string user1, string user2)
        {
            return _calls.Keys.Any(k => k.Sender == user1 && k.Receiver == user2);
        }

        // Retorna a URL da sala de chamada
        public bool GetRoomByUsers(string user1, string user2)
        {
            return _rooms.Keys.Any(k => k.Sender == user1 && k.Receiver == user2);
        }

        public bool IsUserOnCall(string senderId, string receiverId)
        {
            //ele esta em chamada com o User Logado se ele ligou e esta ativo ou recebeu e esta ativo
            return _calls.Any(call =>
                ((call.Key.Sender == senderId && call.Value.Item1) && (call.Key.Receiver == receiverId)) //se o sender ligou e esta ativo e eu sou o receiver
                || 
                ((call.Key.Sender == receiverId && call.Value.Item2) && (call.Key.Receiver == senderId))); // ou o receiver ligou e esta ativo e eu sou o sender

        }


        // Obtém o estado de aceitação da chamada
        /*public bool HasReceiverAccepted(string sender, string receiver)
        {
            return _calls.TryGetValue((sender, receiver), out var accepted) && accepted;
        }*/

        // Lista todas as chamadas ativas
        public void PrintActiveCalls()
        {
            Console.WriteLine("Chamadas Ativas:");
            foreach (var call in _calls)
            {
                Console.WriteLine($"- {call.Key.Sender} -> {call.Key.Receiver} (Aceito: {call.Value})");
            }
        }

        public void AddUser(string connectionId, string userId)
        {
            _onlineUsers.TryAdd(connectionId, userId);
        }

        public void RemoveUser(string connectionId)
        {
            _onlineUsers.TryRemove(connectionId, out _);
        }

        public bool IsUserOnline(string userId)
        {
            return _onlineUsers.Values.Contains(userId);
        }

        public string GetUserIdByConnectionId(string connectionId)
        {
            return _onlineUsers.TryGetValue(connectionId, out var userId) ? userId : null;
        }

        public List<string> GetOnlineUsers()
        {
            return _onlineUsers.Values.Distinct().ToList();
        }

        public int GetOnlineUsersCount()
        {
            return _onlineUsers.Values.Distinct().Count();
        }

        public bool GetIsOnlineUser(string userId)
        {
            return _onlineUsers.Values.Contains(userId);
        }
    }
}
