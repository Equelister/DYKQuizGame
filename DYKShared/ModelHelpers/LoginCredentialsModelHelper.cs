using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DYKShared.ModelHelpers
{
    public class LoginCredentialsModelHelper
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; }

        public LoginCredentialsModelHelper()
        {
        }

        public static LoginCredentialsModelHelper JsonToModelHelper(string json)
        {
            var jsonData = JsonSerializer.Deserialize<LoginCredentialsModelHelper>(json);
            return jsonData;
        }
    }
}
