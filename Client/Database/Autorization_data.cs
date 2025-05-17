using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PasswordManager.Database
{
    public class Autorization_data
    {
        public int Id { get; set; }
        public string Site { get; set; }
        [MaxLength(40)]
        public string Login { get; set; }
        [MaxLength(40)]
        public string Password { get; set; }
        public int AccountId { get; set; }
        [JsonIgnore]
        public Account Account { get; set; }
    }
}
