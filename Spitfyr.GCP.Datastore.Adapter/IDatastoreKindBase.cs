using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Datastore.V1;
using Spitfyr.GCP.Datastore.Adapter.Serialization;

namespace Spitfyr.GCP.Datastore.Adapter
{
    public interface IDatastoreKindBase<TEntity, TKey>
        where TEntity : DatastoreEntity
    {
        Task<TKey> InsertOneAsync(TEntity entity);
        Task<TKey[]> InsertAsync(TEntity[] dalEntities);

        Task FindOneAndReplaceAsync(TEntity entity);

        Task<TEntity> FindAsync(TKey id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldComparisons"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> FindInAsync(IDictionary<string, dynamic> fieldComparisons);
        Task<IEnumerable<TEntity>> FindAsync(IQueryOptions<TEntity> queryOptions);
        Task<IEnumerable<TEntity>> FindAsync(Filter filter);
        Task<IEnumerable<TEntity>> FindAsync(Query query);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<Key>> FindKeysAsync(Filter filter);
        Task<long> CountAsync(IQueryOptions<TEntity> queryOptions);

        TKey InsertOne(TEntity entity);
        TKey[] Insert(TEntity[] dalEntities);
        
        IEnumerable<TEntity> Find(IQueryOptions<TEntity> queryOptions);
        IEnumerable<TEntity> Find(Filter filter);
        IEnumerable<TEntity> Find(Query query);
        IEnumerable<TEntity> GetAll();
        IEnumerable<Key> FindKeys(Filter filter);
        long Count(IQueryOptions<TEntity> queryOptions = null);

        Task DeleteOneAsync(TKey id);
        Task DeleteAsync(TKey[] ids);
        Task DeleteManyAsync(Filter filter);

        Task UpdateAsync(TEntity entity);
    }
}
