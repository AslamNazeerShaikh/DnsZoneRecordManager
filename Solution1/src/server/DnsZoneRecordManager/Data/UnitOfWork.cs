using DnsZoneRecordManager.Models;

namespace DnsZoneRecordManager.Data
{
    /// <summary>Unit of Work: one shared context behind the zone/record repositories.</summary>
    public interface IUnitOfWork
    {
        /// <summary>Zone repository.</summary>
        IGenericRepository<DnsZone> Zones { get; }

        /// <summary>Record repository.</summary>
        IGenericRepository<DnsRecord> Records { get; }

        /// <summary>Persists staged changes.</summary>
        /// <returns>Number of state entries written.</returns>
        Task<int> SaveAsync();
    }

    /// <summary>EF Core implementation of <see cref="IUnitOfWork"/>.</summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        /// <summary>Creates a unit of work over the given context.</summary>
        /// <param name="context">EF Core context shared by both repositories.</param>
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Zones = new GenericRepository<DnsZone>(context);
            Records = new GenericRepository<DnsRecord>(context);
        }

        /// <inheritdoc />
        public IGenericRepository<DnsZone> Zones { get; }

        /// <inheritdoc />
        public IGenericRepository<DnsRecord> Records { get; }

        /// <inheritdoc />
        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
