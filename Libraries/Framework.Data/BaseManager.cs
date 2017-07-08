using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Linq;

using Framework.Common.Json;
using Framework.Common;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;

namespace Framework.Data
{
    /// <summary>
    /// 实体类的增删改查基本操作
    /// </summary>
    /// <typeparam name="TModel">实体类型</typeparam>
    public abstract class BaseManager<TDbContext, TModel, TKey> : IDisposable
        where TModel : BaseModel<TKey>, new()
        where TDbContext : DbContext, new()
    {
        protected TDbContext Db { get; set; }

        /// <summary>
        /// 增加方法
        /// </summary>
        /// <param name="item">实体对象</param>
        /// <returns></returns>
        public virtual async Task<bool> SaveAsync(IEnumerable<TModel> items)
        {
            using (Db = new TDbContext())
            {
                Db.Set<TModel>().AddRange(items);
                int result = await Db.SaveChangesAsync();
                return result > 0;
            }
        }
        /// <summary>
        /// 增加方法
        /// </summary>
        /// <param name="item"></param>
        /// <param name="id">指定Id值，默认为Guid.NewGuid()</param>
        /// <returns></returns>
        public virtual async Task<bool> SaveAsync(TModel item, TKey key = default(TKey))
        {
            if (item.ID == null || item.ID.ToString() == "0")
            {
                item.ID = key;
                return await CUDAsync(item, EntityState.Added);
            }
            else
            {
                return await CUDAsync(item, EntityState.Modified);
            }
        }

        /// <summary>
        ///  删除，默认只把记录状态标记为 DeleteTag，
        ///  如需物理删除，请设realDelete为true
        /// </summary>
        /// <param name="id"></param>
        /// <param name="realDelete"></param>
        /// <example>EntityException</example>
        /// <returns></returns>
        public virtual async Task<bool> DeleteAsync(TKey id, bool realDelete = false)
        {
            var item = await GetByIdAsync(id);
            if (item == null)
                throw new EntityException("删除的实体不存！");
            bool result = await CUDAsync(item, EntityState.Deleted, realDelete);
            return result;
        }
        /// <summary>
        ///  删除，默认只把记录状态标记为 DeleteTag，
        ///  如需物理删除，请设realDelete为true
        /// </summary>
        /// <param name="id"></param>
        /// <param name="realDelete"></param>
        /// <example>EntityException</example>
        /// <returns></returns>
        public virtual bool Delete(TKey id, bool realDelete = false)
        {
            var item = GetById(id);
            if (item == null)
                throw new EntityException("删除的实体不存！");
            bool result = CUD(item, EntityState.Deleted, realDelete);
            return result;
        }
        public virtual async Task<bool> DeleteAsync(TModel item, bool realDelete = false)
        {
            bool result = await CUDAsync(item, EntityState.Deleted, realDelete);
            return result;
        }
        /// <summary>
        /// 返回分页数据方法，如果需要按更多条件返回，请重写此方法
        /// </summary>
        /// <param name="id">实体主键</param>
        /// <param name="pageIndex">页码，必需参数，大于0</param>
        /// <param name="pageSize">页大小，必需参数</param>
        /// <param name="queryItem">对象查找条件</param>
        /// <returns>ResultModel 对象，需要参Data进行转换</returns>
        public virtual async Task<GeneralResponseModel<List<TModel>>> QueryAsync(int pageIndex, int pageSize, RecordStates state = RecordStates.AuditPass)
        {
            // 参数检查
            if (pageIndex < 1)
            {
                throw new ArithmeticException("pageIndex参数无效，必需大于0的整数");
            }
            if (pageSize < 1)
            {
                throw new ArithmeticException("pageSize 参数无效，必需大于0的整数");
            }
            using (Db = new TDbContext())
            {
                var items = Db.Set<TModel>().Where(m => true);

                var list = items
                    .OrderByDescending(f => f.CreateDate)
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize);

                var result = new GeneralResponseModel<List<TModel>>
                {
                    Data = list.ToList(),
                    TotalCount = await items.CountAsync()
                };

                return result;
            }
        }

        /// <summary>
        /// 返回分页数据方法，如果需要按更多条件返回，请重写此方法
        /// </summary>
        /// <returns>GeneralResponseModel<List<TModel>> 对象</returns>
        public virtual async Task<GeneralResponseModel<List<TModel>>> QueryAsync(Expression<Func<TModel, bool>> expression)
        {
            using (Db = new TDbContext())
            {
                var querys = Db.Set<TModel>().Where(expression);
                var result = new GeneralResponseModel<List<TModel>>
                {
                    Data = querys.ToList(),
                    TotalCount = await querys.CountAsync()
                };
                return result;
            }
        }

        /// <summary>
        /// 返回单个对象
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isNoTracking">没有ef跟踪的全新对象</param>
        /// <returns></returns>
        public virtual async Task<TModel> GetByIdAsync(TKey id, bool isNoTracking = false)
        {
            using (Db = new TDbContext())
            {
                if (isNoTracking)
                    return await Db.Set<TModel>().AsNoTracking().FirstOrDefaultAsync(m => m.ID.Equals(id));
                else
                    return await Db.Set<TModel>().FirstOrDefaultAsync(m => m.ID.ToString() == id.ToString());
            }
        }
        /// <summary>
        /// 返回单个对象
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isNoTracking">没有ef跟踪的全新对象</param>
        /// <returns></returns>
        public virtual TModel GetById(TKey id, bool isNoTracking = false)
        {
            using (Db = new TDbContext())
            {
                if (isNoTracking)
                    return Db.Set<TModel>().AsNoTracking().FirstOrDefault(m => m.ID.Equals(id));
                else
                    return Db.Set<TModel>().FirstOrDefault(m => m.ID.Equals(id));
            }
        }
        /// <summary>
        /// 返回单个对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<TModel> GetByExpreAsync(Expression<Func<TModel, bool>> expression)
        {
            using (Db = new TDbContext())
            {
                var item = await Db.Set<TModel>().FirstOrDefaultAsync(expression);
                return item;
            }
        }
        /// <summary>
        /// 返回表中所有的数据，如果数据量太大，请慎用此方法
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<TModel>> GetAllAsync()
        {
            using (Db = new TDbContext())
            {
                return await Db.Set<TModel>().ToListAsync();
            }
        }

        /// <summary>
        /// 返回多个对象
        /// </summary>
        /// <param name="ids">id数据</param>
        /// <returns></returns>
        public virtual async Task<List<TModel>> GetByIdsAsync(TKey[] ids)
        {
            using (Db = new TDbContext())
            {
                var items = Db.Set<TModel>()
                .Where(m => ids.Contains(m.ID))
                .ToListAsync();

                return await items;
            }
        }
        /// <summary>
        /// C：Create，D：Delete，U：Update增删改统一方法，
        /// 默认只把记录标识为删除状态
        /// </summary>
        /// <param name="item"></param>
        /// <param name="state"></param>
        /// <param name="realDelete">是否物理删除</param>
        /// <returns></returns>
        private async Task<bool> CUDAsync(TModel item, EntityState state, bool realDelete = false)
        {
            if (item == null)
            {
                throw new ArithmeticException($"{nameof(item)}参数无效，必需不能为null的实体对象。");
            }

            using (Db = new TDbContext())
            {
                if (state == EntityState.Added)
                {
                    Db.Entry(item).State = EntityState.Added;
                }
                else if (state == EntityState.Modified)
                {
                    Db.Entry(item).State = EntityState.Modified;
                }
                else if (state == EntityState.Deleted)
                {
                    if (realDelete)
                    {
                        Db.Entry(item).State = EntityState.Deleted;
                    }
                    else
                    {
                        item.RecordState = RecordStates.Deleted;
                        Db.Entry(item).State = EntityState.Modified;
                    }
                }
                else
                {
                    throw new Exception($"{state.ToString()}类型不受支持！");
                }
                return await Db.SaveChangesAsync() == 1;
            }
        }
        /// <summary>
        /// C：Create，D：Delete，U：Update增删改统一方法，
        /// 默认只把记录标识为删除状态
        /// </summary>
        /// <param name="item"></param>
        /// <param name="state"></param>
        /// <param name="realDelete">是否物理删除</param>
        /// <returns></returns>
        private bool CUD(TModel item, EntityState state, bool realDelete = false)
        {
            if (item == null)
            {
                throw new ArithmeticException($"{nameof(item)}参数无效，必需不能为null的实体对象。");
            }

            using (Db = new TDbContext())
            {
                if (state == EntityState.Added)
                {
                    Db.Entry(item).State = EntityState.Added;
                }
                else if (state == EntityState.Modified)
                {
                    Db.Entry(item).State = EntityState.Modified;
                }
                else if (state == EntityState.Deleted)
                {
                    if (realDelete)
                    {
                        Db.Entry(item).State = EntityState.Deleted;
                    }
                    else
                    {
                        item.RecordState = RecordStates.Deleted;
                        Db.Entry(item).State = EntityState.Modified;
                    }
                }
                else
                {
                    throw new Exception($"{state.ToString()}类型不受支持！");
                }
                return Db.SaveChanges() == 1;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Db != null)
                {
                    Db.Dispose();
                }
            }
        }
    }
}
