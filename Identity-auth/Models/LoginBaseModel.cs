using System.ComponentModel.DataAnnotations;

namespace Identity_auth.Models
{
    public class LoginBaseModel
    {
        public string Email { get; set; }

        public string? Password { get; set; }

        public bool RememberLogin { get; set; }

        public string ReturnUrl { get; set; }
    }
}