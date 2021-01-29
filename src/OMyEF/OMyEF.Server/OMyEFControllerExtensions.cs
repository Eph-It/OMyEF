using Microsoft.AspNet.OData;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMyEF.Server
{
    public abstract class OMyEFControllerExtensions<T>
        where T : class
    {
        public string RouteBase { get; set; }
        public DbContext DbContext { get; set; }
        public virtual IQueryable<T> Get()
        {
            return DbContext.Set<T>().AsNoTracking();
        }
        public virtual T Get(object key)
        {
            return DbContext.Set<T>().Find(key);
        }
        public virtual async Task<T> PostAsync(T obj)
        {
            DbContext.Set<T>().Add(obj);
            await DbContext.SaveChangesAsync();
            return obj;
        }
        public virtual async Task<T> PatchAsync(Delta<T> obj, object key)
        {
            var entity = await DbContext.Set<T>().FindAsync(key);
            if(entity == null) { return null; }

            obj.Patch(entity);
            await DbContext.SaveChangesAsync();
            return entity;
        }
        public virtual async Task<T> PutAsync(T obj)
        {
            DbContext.Entry(obj).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await DbContext.SaveChangesAsync();
            return obj;
        }
        public virtual async Task DeleteAsync(object key)
        {
            var item = await DbContext.Set<T>().FindAsync(key);
            if(item == null)
            {
                throw new KeyNotFoundException();
            }
            DbContext.Remove(item);
            await DbContext.SaveChangesAsync();
        }
    }
    public class OMyEFControllerExtensionDefaultImplementation<TItem> : OMyEFControllerExtensions<TItem>
        where TItem : class
    {

    }
}
