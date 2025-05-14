using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Database
{
    public class Autorization_data
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public Account Account { get; set; }
    }
}
