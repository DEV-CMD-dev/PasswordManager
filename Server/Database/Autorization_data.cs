using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Database
{
    public class Autorization_data
    {
        public int Id { get; set; }
        [MaxLength(72)]
        public string Login { get; set; }
        [MaxLength(72)]
        public string Password { get; set; }
        public int AccountId { get; set; }
        // conn
        public Account Account { get; set; }
    }
}
