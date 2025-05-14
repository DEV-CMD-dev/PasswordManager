using PasswordManager.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ServerMessage
    {
        public Account Account { get; set; }
        public string Message { get; set; }
        public List<Autorization_data> Autorization_Data { get; set; }
        public int Code2FA { get; set; }
        public bool Is2FAEnabled { get; set; } = false;
        public Autorization_data NewPassword { get; set; }

        public ServerMessage()
        {
            Account = new Account();
            Autorization_Data = new List<Autorization_data>();
        }
        public ServerMessage(Account account) // send to login
        {
            Account = account;
        }

        public void AddPassword(Autorization_data password)
        {
            Autorization_data autorization_Data = new Autorization_data();
            autorization_Data.Login = password.Login;
            autorization_Data.Password = password.Password;
            autorization_Data.Site = password.Site;
        }
    }
}
