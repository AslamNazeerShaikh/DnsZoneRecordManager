using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace DnsZoneRecordManager.Data
{
    /// <summary>Generic repository over one entity set (part of the Unit of Work).</summary>
    /// <typeparam name="T">Entity type.</typeparam>
    public interface IGenericRepository<T>
        where T : class
    {
        /// <summary>Finds an entity by primary key.</summary>
        /// <param name="id">Primary key.</param>
        /// <returns>The entity, or null when missing.</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>Lists entities with an optional filter and eager-loaded navigations.</summary>
        /// <param name="predicate">Optional filter (null lists everything).</param>
        /// <param name="includes">Navigations to eager-load.</param>
        /// <returns>Matching entities.</returns>
        Task<List<T>> FindAsync(
            Expression<Func<T, bool>>? predicate = null,
            params Expression<Func<T, object>>[] includes
        );

        /// <summary>Stages a new entity for insert.</summary>
        /// <param name="entity">Entity to insert.</param>
        /// <returns>A task for the operation.</returns>
        Task AddAsync(T entity);

        /// <summary>Marks an entity as modified.</summary>
        /// <param name="entity">Entity to update.</param>
        void Update(T entity);

        /// <summary>Marks an entity for deletion.</summary>
        /// <param name="entity">Entity to delete.</param>
        void Remove(T entity);
    }

    /// <summary>EF Core implementation of <see cref="IGenericRepository{T}"/>.</summary>
    /// <typeparam name="T">Entity type.</typeparam>
    public class GenericRepository<T> : IGenericRepository<T>
        where T : class
    {
        private readonly DbSet<T> _set;

        /// <summary>Creates a repository over the context's set for <typeparamref name="T"/>.</summary>
        /// <param name="context">EF Core context.</param>
        public GenericRepository(AppDbContext context)
        {
            _set = context.Set<T>();
        }

        /// <inheritdoc />
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _set.FindAsync(id);
        }

        /// <inheritdoc />
        public async Task<List<T>> FindAsync(
            Expression<Func<T, bool>>? predicate = null,
            params Expression<Func<T, object>>[] includes
        )
        {
            IQueryable<T> query = _set;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            if (predicate is not null)
            {
                query = query.Where(predicate);
            }

            return await query.ToListAsync();
        }

        /// <inheritdoc />
        public async Task AddAsync(T entity)
        {
            await _set.AddAsync(entity);
        }

        /// <inheritdoc />
        public void Update(T entity)
        {
            _set.Update(entity);
        }

        /// <inheritdoc />
        public void Remove(T entity)
        {
            _set.Remove(entity);
        }
    }
}
