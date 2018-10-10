using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Datastore.Adapter.Serialization;
using Google.Cloud.Datastore.V1;

namespace Google.Cloud.Datastore.Adapter
{
    public interface IDatastoreKindBase<TEntity, TKey>
        where TEntity : DatastoreEntity
    {
        Task<TKey> InsertOneAsync(TEntity entity);
        Task<TKey[]> InsertAsync(TEntity[] dalEntities);

        Task FindOneAndReplaceAsync(TEntity entity);

        Task<TEntity> FindAsync(TKey id);
        Task<IEnumerable<TEntity>> FindInAsync(string field, dynamic[] values);
        Task<IEnumerable<TEntity>> FindAsync(IQueryOptions<TEntity> queryOptions);
        Task<IEnumerable<TEntity>> FindAsync(Filter filter);
        Task<IEnumerable<TEntity>> FindAsync(Query query);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<long> CountAsync(IQueryOptions<TEntity> queryOptions);

        TKey InsertOne(TEntity entity);
        TKey[] Insert(TEntity[] dalEntities);

        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> Find(IQueryOptions<TEntity> queryOptions);
        IEnumerable<TEntity> Find(Filter filter);
        IEnumerable<TEntity> Find(Query query);
        IEnumerable<Key> FindKeys(Filter filter);

        Task DeleteOneAsync(TKey id);
        Task DeleteAsync(TKey[] ids);
        Task DeleteManyAsync(Filter filter);

        Task UpdateAsync(TEntity entity);
    }
}
