using Microsoft.EntityFrameworkCore;
using Repositories.MyDbContext;
using System;
using System.Linq.Expressions;

public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>依ID取得實體</summary>
    Task<TEntity?> GetByIdAsync<TId>(TId id) where TId : notnull;

    /// <summary>取得全部實體</summary>
    Task<IEnumerable<TEntity>> GetAllAsync();

    /// <summary>取得可組合查詢</summary>
    IQueryable<TEntity> GetQueryable();

    /// <summary>新增實體</summary>
    Task AddAsync(TEntity entity);

    /// <summary>批量新增</summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities);

    /// <summary>更新實體</summary>
    void Update(TEntity entity);

    /// <summary>刪除實體</summary>
    void Delete(TEntity entity);

    /// <summary>依ID刪除</summary>
    Task DeleteByIdAsync<TId>(TId id) where TId : notnull;

    /// <summary>檢查是否存在</summary>
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>保存變更</summary>
    Task<int> SaveChangesAsync();
}


public class MyRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly MyDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public MyRepository(MyDbContext context) 
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync<TId>(TId id) => await _dbSet.FindAsync(id);
    public async Task<IEnumerable<TEntity>> GetAllAsync() => await _dbSet.ToListAsync();
    public IQueryable<TEntity> GetQueryable() => _dbSet.AsQueryable();

    public async Task AddAsync(TEntity entity) => await _dbSet.AddAsync(entity);
    public async Task AddRangeAsync(IEnumerable<TEntity> entities) => await _dbSet.AddRangeAsync(entities);

    public void Update(TEntity entity) => _dbSet.Update(entity);
    public void Delete(TEntity entity) => _dbSet.Remove(entity);

    public async Task DeleteByIdAsync<TId>(TId id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null) Delete(entity);
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        => await _dbSet.AnyAsync(predicate);

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}