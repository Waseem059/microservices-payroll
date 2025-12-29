using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
    {
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
        {
        private readonly JwtTokenService _tokenService;

        public AuthController(JwtTokenService tokenService)
            {
            _tokenService = tokenService;
            }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Models.LoginRequest request)
            {
            // Simple validation
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Username and password required");

            // Mock user validation (in real app, check database)
            if (request.Username != "admin" || request.Password != "password123")
                return Unauthorized("Invalid credentials");

            var token = _tokenService.GenerateToken(request.Username);
            return Ok(new { token, message = "Login successful" });
            }

        [HttpPost("validate")]
        public IActionResult ValidateToken([FromHeader] string authorization)
            {
            if (string.IsNullOrEmpty(authorization))
                return BadRequest("Token required");

            var token = authorization.Replace("Bearer ", "");
            var isValid = _tokenService.ValidateToken(token);

            return isValid ? Ok(new { valid = true }) : Unauthorized("Invalid token");
            }
        }
    }
