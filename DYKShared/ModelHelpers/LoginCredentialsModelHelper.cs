using System.Text.Json;

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
