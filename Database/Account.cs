using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Database
{
    public class Account
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool Is2FAEnabled { get; set; } = false;
    }
}
