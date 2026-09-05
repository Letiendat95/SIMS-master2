using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;
using SIMS.Web.Models;

namespace SIMS.Web.Services
{
    public class FacultyService
    {
        private readonly AppDbContext _context;

        public FacultyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Faculty>> GetAllFacultiesAsync()
            => await _context.Faculties.ToListAsync();

        public async Task<List<Faculty>> GetFacultiesByDepartmentAsync(string department)
            => await _context.Faculties
                .Where(f => f.Department == department)
                .ToListAsync();

        public async Task<Faculty?> GetFacultyAsync(int userId)
            => await _context.Faculties.FindAsync(userId);
    }
}
