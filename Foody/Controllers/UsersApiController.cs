using Foody.Data;
using Foody.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Foody.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersApiController : ControllerBase
    {
        private readonly ApplicationDbcontext _applicationDbcontext;

        public UsersApiController(ApplicationDbcontext applicationDbcontext)
        {
            _applicationDbcontext = applicationDbcontext;
        }

        public static class PasswordHelper
        {
            public static string HashPassword(string password)
            {
                using (var sha256 = SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    return Convert.ToBase64String(hashedBytes);
                }
            }
        }

        // API to login a user
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _applicationDbcontext.Users.FirstOrDefault(u => u.Email == model.Email);
                if (user != null)
                {
                    var hashedPassword = PasswordHelper.HashPassword(model.Password);
                    if (user.Password == hashedPassword)
                    {
                        return Ok(new { message = "Login successful", userId = user.Id });
                    }
                    else
                    {
                        return Unauthorized(new { message = "Invalid Email or Password" });
                    }
                }
                else
                {
                    return NotFound(new { message = "User not found" });
                }
            }
            return BadRequest(new { message = "Invalid model" });
        }

        // API to sign up a new user
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_applicationDbcontext.Users.Any(u => u.Email == model.Email))
                {
                    return Conflict(new { message = "Email is already registered" });
                }

                var hashedPassword = PasswordHelper.HashPassword(model.Password);

                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = hashedPassword,
                    Created_at = DateTime.Now,
                    Description = model.Description,
                    ProfilePicture = null // Handle profile picture differently in API (file upload can be separate)
                };

                _applicationDbcontext.Users.Add(user);
                await _applicationDbcontext.SaveChangesAsync();

                return Ok(new { message = "Signup successful", userId = user.Id });
            }
            return BadRequest(new { message = "Invalid model" });
        }

        // API to retrieve user profile
        [HttpGet("profile/{id}")]
        public IActionResult Profile(int id)
        {
            var user = _applicationDbcontext.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }

        // API to edit user profile
        [HttpPut("editProfile/{id}")]
        public async Task<IActionResult> EditProfile(int id, [FromBody] EditProfileViewModel model)
        {
            var user = _applicationDbcontext.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            user.Name = model.Name;
            user.Email = model.Email;
            user.Description = model.Description;

            _applicationDbcontext.Users.Update(user);
            await _applicationDbcontext.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully" });
        }

        // API to logout (essentially doesn't do anything since session is not handled in API)
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // In API, session clearing is not applicable like in MVC. Usually, you would handle token invalidation here.
            return Ok(new { message = "Logout successful" });
        }

        // API to get all users (for testing or admin purposes)
        [HttpGet("getAllUsers")]
        public IActionResult GetAllUsers()
        {
            var users = _applicationDbcontext.Users.ToList();
            return Ok(users);
        }
    }
}
