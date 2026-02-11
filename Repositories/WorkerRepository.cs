using Microsoft.EntityFrameworkCore;
using visionguard.Data;
using visionguard.Models;

namespace visionguard.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="IWorkerRepository"/>.
    /// Uses <see cref="VisionGuardDbContext"/> as the data source.
    /// </summary>
    public class WorkerRepository : IWorkerRepository
    {
        private readonly VisionGuardDbContext _context;

        public WorkerRepository(VisionGuardDbContext context)
        {
            _context = context;
        }

        public async Task<Worker?> GetByIdAsync(int id)
        {
            return await _context.Workers
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Worker?> GetByWorkerIdAsync(string workerId)
        {
            // Internally, WorkerId maps to the existing EmployeeId field.
            return await _context.Workers
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.EmployeeId == workerId);
        }

        public async Task<List<Worker>> GetAllAsync()
        {
            return await _context.Workers
                .AsNoTracking()
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        public async Task AddAsync(Worker worker)
        {
            await _context.Workers.AddAsync(worker);
        }

        public void Update(Worker worker)
        {
            _context.Workers.Update(worker);
        }

        public void Remove(Worker worker)
        {
            _context.Workers.Remove(worker);
        }

        public async Task<bool> ExistsByWorkerIdAsync(string workerId, int? excludeId = null)
        {
            var query = _context.Workers.AsQueryable();

            query = query.Where(w => w.EmployeeId == workerId);

            if (excludeId.HasValue)
            {
                query = query.Where(w => w.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

