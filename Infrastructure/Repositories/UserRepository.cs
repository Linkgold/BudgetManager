using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<User> _dbSet;

        public UserRepository(ApplicationDbContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            _context = context;
            _dbSet = context.Set<User>();
        }

        // ==================== CONSULTAS ====================

        public async Task<User?> GetByIdAsync(int id, bool withTracking = false)
        {
            if (id <= 0) throw new ArgumentException("Invalid user ID", nameof(id));

            IQueryable<User> query = _dbSet;

            if (!withTracking)
            {
                query = query.AsNoTracking();
            }

            User? user = await query
                .FirstOrDefaultAsync(u => u.Id == id);

            return user;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty", nameof(email));

            User? user = await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Info.Email.ToLower() == email.ToLower());

            return user;
        }

        // ==================== VERIFICACIONES ====================

        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0)
                return false;

            bool exists = await _dbSet
                .AsNoTracking()
                .AnyAsync(u => u.Id == id);

            return exists;
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            bool exists = await _dbSet
                .AsNoTracking()
                .AnyAsync(u => u.Info.Email.ToLower() == email.ToLower());

            return exists;
        }

        // ==================== COMANDOS ====================

        public async Task AddAsync(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            await _dbSet.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            _dbSet.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid user ID", nameof(id));

            User? user = await _dbSet.FindAsync(id);

            if (user == null) throw new KeyNotFoundException($"User with ID {id} not found");

            _dbSet.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}