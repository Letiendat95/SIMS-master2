using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;
using SIMS.Web.Models;
using BCrypt.Net;

namespace SIMS.Web.Services
{
    public class AuthenticationService
    {
        private readonly AppDbContext _context;

        public AuthenticationService(AppDbContext context)
        {
            _context = context;
        }

        // Hash "mồi" để khi username không tồn tại vẫn tốn đúng thời gian như khi tồn tại,
        // tránh lộ danh sách tài khoản qua chênh lệch thời gian phản hồi.
        private static readonly string DummyHash =
            BCrypt.Net.BCrypt.HashPassword("not-a-real-password");

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                BCrypt.Net.BCrypt.Verify(password ?? string.Empty, DummyHash);
                return null;
            }

            // Verify password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }

        public bool Authorize(User user, string roleName)
            => user.Role?.RoleName == roleName;

        public string HashPassword(string password)
            => BCrypt.Net.BCrypt.HashPassword(password);

        public async Task<bool> UsernameExistsAsync(string username)
            => await _context.Users.AnyAsync(u => u.Username == username);

        public async Task<Role?> GetRoleByNameAsync(string roleName)
            => await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
    }
}
