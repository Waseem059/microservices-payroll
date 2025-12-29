using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Models
    {
    public class LoginRequest
        {
        public string Username { get; set; }
        public string Password { get; set; }
        }
    }

