using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYKShared.Model
{
    public class UserModel
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int Total_games { get; set; }
        public bool IsReady { get; set; }
        public int GameScore { get; set; }

        public UserModel()
        {
            IsReady = false;
            GameScore = 0;
        }

        public UserModel(string username)
        {
            Username = username;
            IsReady = false;
            GameScore = 0;
        }
    }
}
