using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialProject.Entities;
using SocialProject.WebUI.HUBS;
using SocialProject.WebUI.Models;

namespace SocialProject.WebUI.Controllers
{
    public class MessageController : Controller
    {
        private UserManager<CustomIdentityUser> _userManager;
        private CustomIdentityDbContext _context;
        private ChatHub _hubContext;

        public MessageController(UserManager<CustomIdentityUser> userManager, CustomIdentityDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> Message()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            ViewBag.User = new
            {
                ImageUrl = user.ImageUrl,
                Username = user.UserName,
                Email = user.Email
            };
            return View("Message");
        }
        public async Task<IActionResult> LiveChat()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var users = await _context.Users
                .Where(u => u.Id != currentUser.Id && u.IsOnline)
                .ToListAsync();
            ViewBag.User = new
            {
                Id=currentUser.Id,
                ImageUrl = currentUser.ImageUrl,
                Username = currentUser.UserName,
                Email = currentUser.Email
            };
           
            ViewBag.Users = new List<object>();
            foreach (var item in users)
            {
                ViewBag.Users.Add(new
                {
                    Id = item.Id,
                    ImageUrl = item.ImageUrl,
                    Username = item.UserName,
                    Email = item.Email
                });
            }

            var currentUserViewModel = new UserViewModel
            {
                Id = currentUser.Id,
                ImageUrl = currentUser.ImageUrl,
                Username = currentUser.UserName,
            };
            var userViewModels = users.Select(user => new UserViewModel
            {
                Id = user.Id,
                ImageUrl = user.ImageUrl,
                Username = user.UserName,
            }).ToList();
            var chatViewModel = new ChatViewModel
            {
                CurrentUser = currentUserViewModel,
                Users = userViewModels
            };
            return View("LiveChat",chatViewModel);
        }
        public async Task<IActionResult> GetChatHistory(string senderId, string receiverId)
        {
            try
            {
                var messages = await _context.Messages
                    .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                                (m.SenderId == receiverId && m.ReceiverId == senderId))
                    .OrderBy(m => m.DateTime)
                    .ToListAsync();

                return Json(messages);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("Message/AddMessage")]
        public async Task<IActionResult> AddMessage([FromBody] MessageModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var chat = await _context.Chats.FirstOrDefaultAsync(c =>
                    (c.SenderId == model.SenderId && c.ReceiverId == model.ReceiverId)
                    || (c.SenderId == model.ReceiverId && c.ReceiverId == model.SenderId));

                if (chat == null)
                {
                    chat = new Chat
                    {
                        SenderId = model.SenderId,
                        ReceiverId = model.ReceiverId
                    };
                    await _context.Chats.AddAsync(chat);
                    await _context.SaveChangesAsync();
                }

                var message = new Message
                {
                    ChatId = chat.Id,
                    Content = model.Content,
                    DateTime = DateTime.Now,
                    HasSeen = false,
                    IsImage = false,
                    ReceiverId = model.ReceiverId,
                    SenderId = model.SenderId,
                };

                await _context.Messages.AddAsync(message);
                await _context.SaveChangesAsync();

                // Send the message to the receiver via SignalR
                await _hubContext.Clients.User(model.ReceiverId).SendAsync("ReceiveMessage", model.SenderId, model.Content);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        private async Task<List<Message>> GetChatHistoryInternal(string senderId, string receiverId)
        {
            return await _context.Messages
                .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                            (m.SenderId == receiverId && m.ReceiverId == senderId))
                .OrderBy(m => m.DateTime)
                .ToListAsync();
        }
    }
}
