using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialProject.Entities
{
    public class CustomIdentityUser : IdentityUser<string>
    {
        public string? ImageUrl { get; set; }
        public bool IsFriend { get; set; }
        public bool IsOnline { get; set; }
        public bool HasRequestPending { get; set; }
        public DateTime DisconnectTime { get; set; } = DateTime.Now;
        public string ConnectTime { get; set; } = "";

        public List<Post>? Posts { get; set; } = new List<Post>();
        public List<Friend>? Friends { get; set; }= new List<Friend>();
        public List<FriendRequest> FriendRequests { get; set; } = new List<FriendRequest>();
        //public List<Chat> Chats { get; set; } = new List<Chat>();
        public List<Message> Messages { get; set; } = new List<Message>();
        //public virtual ICollection<Friend>? Friends { get; set; }
        //public virtual ICollection<FriendRequest>? FriendRequests { get; set; }
        public virtual ICollection<Chat>? Chats { get; set; }
        public CustomIdentityUser()
        {
            //Chats = new List<Chat>();
        }
    }
}
