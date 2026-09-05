using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;
using SIMS.Web.Models;

namespace SIMS.Web.Services
{
    public class RoleService
    {
        private readonly AppDbContext _context;

        public RoleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Role>> GetAllRolesAsync()
            => await _context.Roles.Include(r => r.Users).ToListAsync();

        public async Task<Role?> GetRoleByIdAsync(int roleId)
            => await _context.Roles.Include(r => r.Users).FirstOrDefaultAsync(r => r.RoleId == roleId);

        public async Task<List<User>> GetUsersByRoleAsync(int roleId)
            => await _context.Users.Where(u => u.RoleId == roleId).ToListAsync();

        /// <summary>
        /// Đổi vai trò. Trả về null nếu thành công, hoặc thông báo lỗi.
        ///
        /// Lưu ý: User dùng TPH (cột phân biệt "UserType"), EF Core KHÔNG đổi được cột này.
        /// Nếu chỉ đổi RoleId, một Student sẽ mang vai trò Faculty nhưng bản ghi vẫn là Student:
        /// đăng nhập sẽ chuyển tới /Faculty/Detail và báo 404, tài khoản coi như hỏng.
        /// Vì vậy chỉ cho phép đổi vai trò khi khớp với kiểu bản ghi.
        /// </summary>
        public async Task<string?> ChangeUserRoleAsync(int userId, int newRoleId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return "User not found.";

            var newRole = await _context.Roles.FindAsync(newRoleId);
            if (newRole == null) return "Role not found.";

            string? requiredType = newRole.RoleName switch
            {
                "Student" => nameof(Student),
                "Faculty" => nameof(Faculty),
                "Admin" => nameof(Administrator),
                _ => null
            };

            string actualType = user switch
            {
                Student => nameof(Student),
                Faculty => nameof(Faculty),
                Administrator => nameof(Administrator),
                _ => "User"
            };

            if (requiredType != null && actualType != requiredType)
                return $"Cannot turn a {actualType} account into a {newRole.RoleName} account. " +
                       $"Please create a new {newRole.RoleName} account instead.";

            user.RoleId = newRoleId;
            await _context.SaveChangesAsync();
            return null;
        }
    }
}
