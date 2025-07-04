using FormBuilder.Domain.Core;
using FormBuilder.Domain.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.DataAccess.Core;

public abstract class BaseRepository<TDbContext, TModel, TId> : IBaseRepository<TModel, TId>
       where TDbContext : DbContext
       where TModel : BaseEntity<TId>
{

    protected readonly TDbContext Db;

    protected BaseRepository(TDbContext db)
    {
        Db = db;

    }
    public async Task<bool> AnyAsync(Expression<Func<TModel, bool>> expression)
    {
        return await Db.Set<TModel>().AnyAsync(expression);
    }


    public async Task CreateAsync(TModel model)
    {
        await Db.Set<TModel>().AddAsync(model);
        await Db.SaveChangesAsync();
    }

    public async Task DeleteAsync(TModel model)
    {

        Db.Set<TModel>().Remove(model);
        await Db.SaveChangesAsync();
    }

    public async Task DeleteAsync(TId id)
    {
        var model = await Db.Set<TModel>().FindAsync(id);
        if (model != null)
        {
            Db.Set<TModel>().Remove(model);
            await Db.SaveChangesAsync();
        }
    }

    public async Task<TModel> FindAsync(TId id)
    {
        return await Db.Set<TModel>().FindAsync(id);
    }

    public async Task<TModel> FindAsync(Expression<Func<TModel, bool>> expression)
    {
        return await Db.Set<TModel>().FindAsync(expression);
    }

    public async Task<TModel> GetAsync(TId id)
    {
        var model = await Db.Set<TModel>().FindAsync(id);
        if (model == null)
        {
            throw new EntityNotFoundException();
        }
        return model;
    }

    public async Task<List<TModel>> GetListAsync()
    {
        return await (await QueryableAsync()).ToListAsync();

    }

    public async Task<List<TModel>> GetListAsync(Expression<Func<TModel, bool>> expression)
    {
        return await Db.Set<TModel>().Where(expression).ToListAsync();
    }

    public Task<IQueryable<TModel>> QueryableAsync()
    {
        return Task.FromResult(Db.Set<TModel>().AsQueryable());
    }

    public async Task UpdateAsync(TModel model)
    {
        Db.Entry(model).State = EntityState.Modified;
        await Db.SaveChangesAsync();
    }
}
