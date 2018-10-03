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

        Task<TKey> UpdateAsync(Filter filter, TEntity entity);

        Task<TEntity> GetAsync(TKey id);
        Task<IEnumerable<TEntity>> GetAsync(Filter filter);
        Task<IEnumerable<TEntity>> GetAsync(Query query);
        Task<IEnumerable<TEntity>> GetAllAsync();

        TKey InsertOne(TEntity entity);
        TKey[] Insert(TEntity[] dalEntities);

        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> Get(Filter filter);
        IEnumerable<TEntity> Get(Query query);

        Task DeleteOneAsync(TKey id);
        Task DeleteAsync(TKey[] ids);

        Task UpdateAsync(TEntity entity);
    }
}
