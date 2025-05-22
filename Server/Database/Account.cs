using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PasswordManager.Database
{
    public class Account
    {
        public int Id { get; set; }
        [MaxLength(40)]
        public string Username { get; set; }
        public string Password { get; set; }
        [MaxLength(40)]
        public string Email { get; set; }
        public string AvatarPath { get; set; } = "default.png";
        public bool Is2FAEnabled { get; set; } = false;

        // conn
        [JsonIgnore]
        public ICollection<Autorization_data> ?Autorization_Data { get; set; }
    }
}
