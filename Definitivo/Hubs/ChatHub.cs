using Definitivo.Data;
using Definitivo.Models;
using Definitivo.Models.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using NuGet.Protocol.Plugins;
using System.Collections.Concurrent;

namespace Definitivo.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatService _chatService;
        public ApplicationDbContext _context { get; }

        public ChatHub(ApplicationDbContext context, ChatService chatService)
        {
            _context = context;
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                _chatService.AddUser(Context.ConnectionId, userId);
            }
            await Clients.All.SendAsync("ReceiveMessage", userId, true, "statusUpdate");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = _chatService.GetUserIdByConnectionId(Context.ConnectionId);

            _chatService.RemoveUser(Context.ConnectionId);
            await Clients.All.SendAsync("ReceiveMessage", userId, false, "statusUpdate");
            await base.OnDisconnectedAsync(exception);
        }

    }
}