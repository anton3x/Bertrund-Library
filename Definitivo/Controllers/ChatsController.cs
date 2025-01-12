using Definitivo.Data;
using Definitivo.Hubs;
using Definitivo.Models;
using Definitivo.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using System.Drawing.Printing;
using System.Security.Claims;
using System.Text;
using static Definitivo.Controllers.ChatsController;


namespace Definitivo.Controllers
{
    public class DailyRoomResponse
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public int max_participants { get; set; }
    }

    [Authorize]
    public class ChatsController : Controller
    {
        public ApplicationDbContext _context;
        private readonly UserManager<Perfil> _userManager; 
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager; 
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ChatService _chatService;

        public ChatsController(IConfiguration configuration, ApplicationDbContext context, ChatService chatService, UserManager<Perfil> userManager, RoleManager<IdentityRole> roleManager, IHubContext<ChatHub> hubContext) 
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
            _roleManager = roleManager;
            _chatService = chatService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string idUser)
        {

            if (!string.IsNullOrEmpty(idUser))
            {
                var user = await _context.Users.FindAsync(idUser);

                var messages = await _context.Messages
                    .Include(u => u.Sender)
                    .Include(u => u.Receiver)
                    .Where(m => (m.SenderId == User.FindFirstValue(ClaimTypes.NameIdentifier) && m.ReceiverId == idUser) ||
                                (m.SenderId == idUser && m.ReceiverId == User.FindFirstValue(ClaimTypes.NameIdentifier)))
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();

                var model = new ChatViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    UserImage = user.FotoNome ?? "userDefault.png",
                    Messages = messages
                };

                ViewData["ChatModel"] = model;
                ViewData["idUser"] = idUser;
            }

            string userId = _userManager.GetUserId(User); //id do utilizador logado

            if(userId == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            var chats = await _context.Messages
                .Include(u => u.Sender)
                .Include(u => u.Receiver)
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => new ChatsViewModel
                {
                    UserId = g.Key,
                    UserName = g.Key == g.First().SenderId ? g.First().Sender.UserName : g.First().Receiver.UserName,
                    UserImage = (g.Key == g.First().SenderId ? g.First().Sender.FotoNome : g.First().Receiver.FotoNome) ?? "userDefault.png",
                    LastMessage = g.OrderByDescending(m => m.Timestamp).First().Content,
                    LastMessageTimeStamp = g.Max(m => m.Timestamp),
                    UnreadMessages = g.Count(m => m.ReceiverId == userId && !m.Seen),
                    online = _chatService.GetIsOnlineUser(g.Key),
                    onCall = _chatService.IsUserOnCall(g.Key, userId) //se esta em call com o user em questao, tendo que estar na chamada com o user logado
                })
                .OrderByDescending(chat => chat.LastMessageTimeStamp)
                .Take(8)
                .ToListAsync();

            //existem users que podem ainda nao ter mensagens com o user logado, mas estao online e a ligar-lhe, por isso
            //vamos ao chatService buscar os users online e que estao em chamada

            var onlineUsers = _chatService.GetOnlineUsers();
            foreach (var user in onlineUsers)
            {
                if (!chats.Any(c => c.UserId == user) && (user != userId)) // se nao esta na lista chats e nem sou eu, pois nao vou falar comigo proprio
                {
                    if (_chatService.IsUserOnCall(user, userId)) //verifica nos 2 sentidos, se me ligou e esta na chamada ou se recebeu e esta na chamada
                    {
                        var userToAdd = await _context.Users.FindAsync(user);
                        chats.Add(new ChatsViewModel
                        {
                            UserId = userToAdd.Id,
                            UserName = userToAdd.UserName,
                            UserImage = userToAdd.FotoNome ?? "userDefault.png",
                            LastMessage = "Nova Conversa",
                            LastMessageTimeStamp = DateTime.Now,
                            UnreadMessages = 0,
                            online = true,
                            onCall = _chatService.IsUserOnCall(user, userId)
                        });
                    }
                }
            }

            ViewBag.CurrentPage = 1;

            return View("Chats", chats);
        }

        [HttpGet]
        public async Task<IActionResult> ChatUsers(string userId, int page = 1, int pageSize = 8)
        {
            if (userId == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            // Total de itens para calcular a paginação
            int totalItems = await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .CountAsync();

            // Lógica de paginação
            var chats = await _context.Messages
                .Include(u => u.Sender)
                .Include(u => u.Receiver)
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => new ChatsViewModel
                {
                    UserId = g.Key,
                    UserName = g.Key == g.First().SenderId ? g.First().Sender.UserName : g.First().Receiver.UserName,
                    UserImage = (g.Key == g.First().SenderId ? g.First().Sender.FotoNome : g.First().Receiver.FotoNome) ?? "userDefault.png",
                    LastMessage = g.OrderByDescending(m => m.Timestamp).First().Content,
                    LastMessageTimeStamp = g.Max(m => m.Timestamp),
                    UnreadMessages = g.Count(m => m.ReceiverId == userId && !m.Seen),
                    online = _chatService.GetIsOnlineUser(g.Key),
                    onCall = _chatService.IsUserOnCall(g.Key, userId)
                })
                .OrderByDescending(chat => chat.LastMessageTimeStamp)
                .Take(page * pageSize)
                .ToListAsync();

            var onlineUsers = _chatService.GetOnlineUsers();
            foreach (var user in onlineUsers)
            {
                if (!chats.Any(c => c.UserId == user) && user != userId)
                {
                    var userToAdd = await _context.Users.FindAsync(user);
                    chats.Add(new ChatsViewModel
                    {
                        UserId = userToAdd.Id,
                        UserName = userToAdd.UserName,
                        UserImage = userToAdd.FotoNome ?? "userDefault.png",
                        LastMessage = "Nova Conversa",
                        LastMessageTimeStamp = DateTime.Now,
                        UnreadMessages = 0,
                        online = true,
                        onCall = _chatService.IsUserOnCall(user, userId)
                    });
                }
            }


            // Informações de paginação para a View
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.CurrentPage = page <= ViewBag.TotalPages ? page : ViewBag.TotalPages;

            // Retornar a PartialView com os dados paginados
            return PartialView("_ChatUsers", chats);
        }


        [HttpGet]
        public async Task<IActionResult> OpenChat(string userId, int pageNumber = 1, int pageSize = 10)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null || userId == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            var messages = await _context.Messages
                .Include(u => u.Sender)
                .Include(u => u.Receiver)
                .Include(u => u.Reactions)
                .Where(m => (m.SenderId == User.FindFirstValue(ClaimTypes.NameIdentifier) && m.ReceiverId == userId) ||
                            (m.SenderId == userId && m.ReceiverId == User.FindFirstValue(ClaimTypes.NameIdentifier)))
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            // Calcula o total de mensagens
            var totalMessagesCount = messages.Count();

            // Calcula o número total de páginas
            var totalPages = (int)Math.Ceiling(totalMessagesCount / (double)pageSize);

            // Carrega as mensagens para a página atual
            messages = messages
                .Take(pageNumber * pageSize)                      // Retorna o número de mensagens da página
                .ToList();

            // Atualiza as mensagens não lidas
            var unreadMessages = messages.Where(m => m.SenderId == user.Id && !m.Seen).ToList();
            foreach (var message in unreadMessages)
            {
                message.Seen = true;
            }

            if (unreadMessages.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            var model = new ChatViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                UserImage = user.FotoNome ?? "userDefault.png",
                Messages = messages.OrderBy(m => m.Timestamp).ToList(),// Reordena para exibir na ordem correta
                CurrentPage = pageNumber <= totalPages ? pageNumber : totalPages,
                TotalPages = totalPages
            };

            ViewData["ChatModel"] = model;

            return PartialView("_ChatDetails", model);
        }

        public class EditMessageDto
        {
            public int MessageId { get; set; }
            public string NewContent { get; set; }
        }

        public class DelMessageDto
        {
            public int MessageId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> EditMessage([FromBody] EditMessageDto editMessageDto)
        {
            var message = _context.Messages.Find(editMessageDto.MessageId);
            if (message == null || message.SenderId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return BadRequest("Ação não permitida.");
            }

            message.Content = editMessageDto.NewContent;
            message.Edited = true;
            //message.Timestamp = DateTime.Now;
            _context.SaveChanges();

            await _hubContext.Clients.User(message.ReceiverId).SendAsync("EditMessage", User.Identity.Name, message.Id,message.Content,  message.Timestamp.ToString("dd/MM/yyyy HH:mm"));
            
            return Ok(new { success = true, updatedContent = editMessageDto.NewContent });
        }


        [HttpGet]
        public IActionResult Call()
        {
            // URL da sala Daily.co já criada
            string roomUrl = "https://bertrund.daily.co/josecamoesroom";

            ViewData["RoomUrl"] = roomUrl;
            return View("Call");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessage([FromBody] DelMessageDto delMessageDto)
        {
            var message = _context.Messages.Find(delMessageDto.MessageId);
            if (message == null || message.SenderId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return BadRequest("Ação não permitida.");
            }
            
            if(message.FileName != null)
            {
                //remover o ficheiro antes de excluir a mensagem
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files/uploads/", message.FileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Messages.Remove(message);
            _context.SaveChanges();

            //para remover as replys a essa mensagem e manter a mensagem sem reply
            var messages = await _context.Messages.Where(m => m.ReplyTo == delMessageDto.MessageId).ToListAsync();

            foreach (var msg in messages)
            {
                msg.ReplyTo = null;
            }

            _context.SaveChanges();

            var lastMessage = _context.Messages.Where(m => m.ReceiverId == message.ReceiverId && m.SenderId == message.SenderId || m.ReceiverId == message.SenderId && m.SenderId == message.ReceiverId).OrderByDescending(m => m.Timestamp).FirstOrDefault();
            var unreadMessages = _context.Messages.Where(m => m.ReceiverId == message.ReceiverId && m.SenderId == message.SenderId && !m.Seen).Count();

            await _hubContext.Clients.User(message.ReceiverId).SendAsync("DeleteMessage", message.SenderId, User.Identity.Name, message.Id, lastMessage == null ? "Nova Conversa" : lastMessage.Content, unreadMessages);

            return Ok(new { success = true, lastMessageSent = (lastMessage == null) ? "Nova Conversa" : lastMessage.Content});
        }

        public class ReactionDTO
        {
            public int messageId { get; set; }
            public string emoji { get; set; }
            public string userId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReaction([FromBody] ReactionDTO request)
        {
            // Verificação básica do ModelState
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Buscar a mensagem e incluir as reações relacionadas (async)
            var message = await _context.Messages
                                        .Include(m => m.Reactions)
                                        .FirstOrDefaultAsync(m => m.Id == request.messageId);

            if (message == null)
            {
                return NotFound("Mensagem não encontrada.");
            }

            // Verificar se a reação do usuário para este emoji já existe
            var existingReaction = message.Reactions
                                          .FirstOrDefault(r => r.Emoji == request.emoji && r.UserId == request.userId);

            // Se existir, remove; caso contrário, adiciona
            if (existingReaction != null)
            {
                // Remove a reação existente
                message.Reactions.Remove(existingReaction);
            }
            else
            {
                // Adiciona nova reação
                var newReaction = new Reaction
                {
                    Emoji = request.emoji,
                    UserId = request.userId,
                    MessageId = request.messageId
                };

                message.Reactions.Add(newReaction);
            }

            // Persistir as alterações no banco (somente uma vez)
            await _context.SaveChangesAsync();

            // Recalcular a lista atualizada de reações
            var reactionValues = message.Reactions
                                        .GroupBy(r => r.Emoji)
                                        .ToDictionary(g => g.Key, g => g.Count());

            // Determinar qual usuário receberá a notificação (sender ou receiver)
            var targetUserId = message.ReceiverId == request.userId
                ? message.SenderId
                : message.ReceiverId;

            // Enviar a atualização via SignalR
            await _hubContext.Clients.User(targetUserId)
                                   .SendAsync("AddReaction",
                                              request.messageId,
                                              request.emoji,
                                              request.userId,
                                              reactionValues);

            // Retornar a lista final de reações
            return Ok(reactionValues);
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage(string receiverId, string message, IFormFile? file, int? replyToMessageId)
        {
            if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(receiverId))
            {
                return BadRequest("Mensagem ou destinatário inválido.");
            }

            //mensagens que eu recebi
            var msgs = await _context.Messages
                .Include(u => u.Sender)
                .Include(u => u.Receiver)
                .Include(u => u.Reactions)
                .Where(m => m.SenderId == receiverId && m.ReceiverId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            //as nao vistas ficam a vistas
            var unreadMessages = msgs.Where(m => !m.Seen).ToList();
            foreach (var msg in unreadMessages)
            {
                msg.Seen = true;
            }

            // Guardar as alterações no contexto
            if (unreadMessages.Count > 0)
            {
                await _context.SaveChangesAsync();
            }


            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var receiver = await _context.Users.FindAsync(receiverId);

            if (senderId == null || receiver == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }


            string? fileName = null;

            // Se houver ficheiro, guarde no servidor
            if (file != null)
            {
                fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files/uploads/", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            // Criar a nova mensagem
            var newMessage = new Models.Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Sender = await _context.Users.FindAsync(senderId),
                Receiver = receiver,
                Content = message,
                Timestamp = DateTime.Now,
                FileName = fileName,
                Seen = false,
                Edited = false,
                ReplyTo = replyToMessageId
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            //encontrar o conteudo da mensagem a dar reply
            var messageToReply = await _context.Messages.Where(m => m.Id == replyToMessageId).FirstOrDefaultAsync();

            await _hubContext.Clients.User(receiverId).SendAsync("ReceiveMessage", message, newMessage.Id,  newMessage.Timestamp.ToString("HH:mm"), newMessage.SenderId, _chatService.GetIsOnlineUser(newMessage.SenderId), newMessage.Edited, newMessage.ReplyTo, replyToMessageId == null ? "" : messageToReply.Content.ToString(), newMessage.FileName, "chatMessage");


            //retornar ou partial view por ajax ou view normal
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return await OpenChat(receiverId);
            
	        return RedirectToAction("OpenChat", new { userId = receiverId });

        }

        public class TypingNotification
        {
            public string senderId { get; set; }
            public string receiverId { get; set; }
        }


        [HttpPost]
        public async Task<IActionResult> StartTyping([FromBody] TypingNotification notification)
        {
            // Lógica para notificar os outros usuários na sala que o usuário está digitando
            // Por exemplo, você pode usar SignalR para enviar a notificação a todos
            await _hubContext.Clients.User(notification.receiverId).SendAsync("StartTyping", User.Identity.Name);

            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> StopTyping([FromBody] TypingNotification notification)
        {
            // Lógica para notificar os outros usuários na sala que o usuário parou de digitar
            await _hubContext.Clients.User(notification.receiverId).SendAsync("StopTyping", User.Identity.Name);

            return Ok();
        }


        [HttpGet]
        public async Task<IActionResult> SearchUsers(string query)
        {
            var userLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userLogadoId == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            // Filtra os utilizadores com base no query, excluindo admins e o próprio utilizador
            var users = await _context.Users
                .Where(u => u.UserName.Contains(query) && u.Id != userLogadoId)
                .ToListAsync(); // Efetua a query para permitir chamadas adicionais que dependam de lógica externa

            // Remove os utilizadores com a role "Admin"
            var filteredUsers = users
                .Where(u => !_context.UserRoles
                    .Any(ur => ur.UserId == u.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Admin")))
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    FotoNome = u.FotoNome ?? "userDefault.png",
                    isOnline = _chatService.GetIsOnlineUser(u.Id),
                    onCall = _chatService.IsUserOnCall(u.Id, userLogadoId)
                })
                .ToList();


            return Json(filteredUsers);
        }

        // Modelo da requisição
        public class CallRequestModel
        {
            public string ReceiverUserId { get; set; }
            public string SenderUserId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> SendCallRequest([FromBody] CallRequestModel request)
        {
            if (string.IsNullOrEmpty(request.ReceiverUserId) || string.IsNullOrEmpty(request.SenderUserId))
            {
                return BadRequest(new { message = "Dados inválidos. ReceiverUserId e SenderUserId  são obrigatórios." });
            }

            if(_chatService.IsCallActive(request.SenderUserId, request.ReceiverUserId))
            {
                //ja existe uma chamada entre os dois utilizadores
                return await CallAccepted(request);
            }
            else if (_chatService.IsCallActive(request.ReceiverUserId, request.SenderUserId))
            {
                //ja existe uma chamada entre os dois utilizadores
                return await CallAccepted(new CallRequestModel { SenderUserId = request.ReceiverUserId, ReceiverUserId = request.SenderUserId});
            }

            // Verificar se existe uma sala com os utilizadores
            var urlRoom = await _chatService.GetExistingRoom(request.SenderUserId, request.ReceiverUserId);

            if (urlRoom == null)
            {
                urlRoom = await _chatService.GetExistingRoom(request.ReceiverUserId, request.SenderUserId);
                if(urlRoom == null)
                {
                    urlRoom = await CreateDailyRoom();
                }
            }

            if (string.IsNullOrEmpty(urlRoom))
            {
                return BadRequest(new { message = "Erro ao criar a sala de chamada." });
            }

            // Envia para o chatservice a requisição de chamada
            _chatService.StartCall(request.SenderUserId, request.ReceiverUserId, urlRoom);

            // Envia a notificação ao utilizador destino via SignalR
            await _hubContext.Clients.User(request.ReceiverUserId).SendAsync("ReceiveCallRequest", request.SenderUserId, User.Identity.Name);

            return Ok(new { message = "Call request sent successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> GetRoomInfo([FromBody] CallRequestModel request)
        {
            if (request == null || string.IsNullOrEmpty(request.SenderUserId) || string.IsNullOrEmpty(request.ReceiverUserId))
            {
                return BadRequest(new { success = false, message = "Dados inválidos." });
            }

            // Obter a URL da sala
            var urlRoom =  await _chatService.GetExistingRoom(request.SenderUserId, request.ReceiverUserId);

            if(urlRoom == null)
            {
                urlRoom = await _chatService.GetExistingRoom(request.ReceiverUserId, request.SenderUserId);
                if (urlRoom == null)
                {
                    urlRoom = await CreateDailyRoom();
                    _chatService.CreateRoomByUsers(request.SenderUserId, request.ReceiverUserId, urlRoom);
                }
            }

            return Json(new { success = true, roomUrl = urlRoom });
        }


        [HttpPost]
        public async Task<string> CreateDailyRoom()
        {
            string ApiKey = _configuration["DailyCo:ApiKey"];
            string DailyApiBaseUrl = _configuration["DailyCo:DailyApiBaseUrl"];

            using (var client = new HttpClient())
            {
                // Adicione o cabeçalho de autenticação
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");

                // Configuração da sala
                var roomData = new
                {
                    name = Guid.NewGuid().ToString(), // Nome único da sala
                    properties = new
                    {
                        exp = (int)(DateTime.UtcNow.AddHours(1).Subtract(new DateTime(1970, 1, 1))).TotalSeconds, // Expira em 1 hora
                        enable_chat = false,
                        enable_knocking = false,
                        enable_prejoin_ui = false, // Prejoin UI
                        enable_screenshare = false // Screen sharing
                    }
                };

                // Serializar os dados para JSON
                var content = new StringContent(JsonConvert.SerializeObject(roomData), Encoding.UTF8, "application/json");

                // Fazer o pedido POST para criar a sala
                var response = await client.PostAsync($"{DailyApiBaseUrl}/rooms", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var roomResponse = JsonConvert.DeserializeObject<DailyRoomResponse>(responseBody);

                    return roomResponse.url;
                }

                return null;
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDailyRoom(string roomName)
        {
            string ApiKey = _configuration["DailyCo:ApiKey"];
            string DailyApiBaseUrl = _configuration["DailyCo:DailyApiBaseUrl"];

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
                var response = await client.DeleteAsync($"{DailyApiBaseUrl}/rooms/{roomName}");

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Erro ao deletar a sala." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> CallAccepted([FromBody] CallRequestModel request)
        {
            if (string.IsNullOrEmpty(request.ReceiverUserId) || string.IsNullOrEmpty(request.SenderUserId))
            {
                return BadRequest(new { message = "Dados inválidos. ReceiverUserId e SenderUserId são obrigatórios." });
            }

            // Verifica se a chamada existe no serviço de chat
            var isCallActive = _chatService.IsCallActive(request.SenderUserId, request.ReceiverUserId);

            if (!isCallActive)
            {
                return BadRequest(new { message = "Não há uma chamada ativa entre os utilizadores." });
            }

            var callState = _chatService.GetCallState(request.SenderUserId, request.ReceiverUserId);
            var userSender = await _context.Users.FindAsync(request.SenderUserId);
            var userReceiver = await _context.Users.FindAsync(request.ReceiverUserId);

            // Marca a chamada como aceita
            var isAccepted = _chatService.AcceptCall(request.SenderUserId, request.ReceiverUserId);

            if (!isAccepted)
            {
                return BadRequest(new { message = "Erro ao aceitar a chamada." });
            }

            // Notifica o utilizador chamador via SignalR
            if (callState.Item1 == true)
                await _hubContext.Clients.User(userSender.Id).SendAsync("AcceptedCallRequest", userReceiver.Id, userReceiver.UserName);
            else if (callState.Item2 == true)
                await _hubContext.Clients.User(userReceiver.Id).SendAsync("AcceptedCallRequest", userSender.Id, userSender.UserName);

            return Ok(new { message = "Call accepted successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> EndCall([FromBody] CallRequestModel request)
        {
            if (string.IsNullOrEmpty(request.ReceiverUserId) || string.IsNullOrEmpty(request.SenderUserId))
            {
                return BadRequest(new { message = "Dados inválidos. ReceiverUserId e SenderUserId são obrigatórios." });
            }

            // Verifica se há uma chamada ativa entre os dois utilizadores
            var isCallActive1 = _chatService.IsCallActive(request.SenderUserId, request.ReceiverUserId);
            var isCallActive2 = _chatService.IsCallActive(request.ReceiverUserId, request.SenderUserId);

            if (!isCallActive1 && !isCallActive2)
            {
                return BadRequest(new { message = "Não há uma chamada ativa entre os utilizadores." });
            }


            // Finaliza a chamada no serviço de chat
            _chatService.EndCall(request.SenderUserId, request.ReceiverUserId); 

            var userSender = await _context.Users.FindAsync(request.SenderUserId);
            var userReceiver = await _context.Users.FindAsync(request.ReceiverUserId);

            if(userSender == null || userReceiver == null)
            {
                return BadRequest(new { message = "Utilizadores não encontrados." });
            }

            var receiverUserId = isCallActive1 ? userReceiver.Id : (isCallActive2 ? userReceiver.Id : userSender.Id);

            await _hubContext.Clients.User(receiverUserId).SendAsync("LeftCall", userSender.Id == receiverUserId ? userReceiver.Id : userSender.Id, userSender.Id == receiverUserId ? userReceiver.UserName : userSender.UserName);

            // Notifica ambos os utilizadores via SignalR que a chamada foi encerrada

            return Ok(new { message = "Call ended successfully." });
        }


        [HttpGet]
        public IActionResult GetUserInfo(string userId)
        {

            var userLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Procura o utilizador com base no userId
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (userLogadoId == null || user == null)
            {
                return NotFound(new { Message = "Utilizador não encontrado ou Nao Autorizado." });
            }

            var isOnline = _chatService.GetIsOnlineUser(userId);
            var isOnCall = _chatService.IsUserOnCall(userId, userLogadoId);

            // Retorna os dados do utilizador no formato JSON
            return Ok(new
            {
                user.Id,
                senderUserName = user.UserName,
                FotoNome = user.FotoNome ?? "userDefault.png",
                online = isOnline,
                onCall = isOnCall
            });

        }

    }
}
