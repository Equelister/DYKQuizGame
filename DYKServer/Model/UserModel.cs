using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYKServer.Model
{
    class UserModel
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int Total_games { get; set; }
    }
}
