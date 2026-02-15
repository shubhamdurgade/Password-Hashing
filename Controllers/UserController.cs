using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Password_Hashing.Data;
using Password_Hashing.Models;

namespace Password_Hashing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly UserDbContext _context;

        public UserController(UserDbContext context)
        {
            _context = context;
        }

        //POST: api/user/register
        [HttpPost("register")]
        public async Task<IActionResult> Regsiter([FromBody] RegistrationDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    return BadRequest("Email already exists.");
                }

                PasswordHasher.CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);

                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok("User registered successfully.");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Login Failed");
            }
        }

        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if(user == null)
                {
                    return Unauthorized("Invalid email or password.");
                }

                if(!PasswordHasher.VerifyPasswordHash(model.Password,user.PasswordHash,user.PasswordSalt))
                {
                    return Unauthorized("Invalid email or password.");   
                }

                return Ok(new { Message = "User logged in successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Login Failed");
            }
        }
    }
}
