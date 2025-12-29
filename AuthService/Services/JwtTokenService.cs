using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace AuthService.Services
    {
    public class JwtTokenService
        {
        private const string SecretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLong1234567890";
        private const string Issuer = "PayrollSystem";
        private const string Audience = "PayrollSystemUsers";

        public string GenerateToken(string username)
            {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Name, username)
            };

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
            }

        public bool ValidateToken(string token)
            {
            try
                {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = Issuer,
                    ValidateAudience = true,
                    ValidAudience = Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                return true;
                }
            catch
                {
                return false;
                }
            }
        }
    }
