using PasswordManager.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Client
{
    public class ServerMessage
    {
        public Account Account { get; set; }
        public string Action { get; set; }
        public string Message { get; set; }
        public List<Autorization_data> Autorization_Data { get; set; }
        public byte[] Image { get; set; }
        public string FileNameImage { get; set; }
        public int Code2FA { get; set; }
        public Autorization_data NewPassword { get; set; }
        

        //public ServerMessage()
        //{
        //    Account = new Account();
        //    Autorization_Data = new List<Autorization_data>();
        //}

        public string LoginJson(Account account) // send to login
        {
            Account = account;
            Action = "Login";
            return JsonSerializer.Serialize(this);
        }
        public string RegisterJson(Account account) // send to login
        {
            Account = account;
            Action = "Register";
            return JsonSerializer.Serialize(this);
        }

        //public string AddPasswordJson(Autorization_data password)
        //{
        //    NewPassword = password;
        //    Action = "AddPassword";
        //    return JsonSerializer.Serialize(this);
        //}
    }
}
