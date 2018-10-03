using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Datastore.Adapter.Serialization;
using Google.Cloud.Datastore.V1;

namespace Google.Cloud.Datastore.Adapter
{
    public interface IDatastoreKind<TEntity, TKey> : IDatastoreKindBase<TEntity, TKey>
        where TEntity : DatastoreEntity
    {
        IQueryable<TEntity> AsQueryable();

        IEnumerable<TEntity> Find(Filter filter);

        Task FindOneAndReplaceAsync(TEntity entity);
        Task<IEnumerable<TEntity>> FindAsync(Filter filter);

        Task DeleteManyAsync(Filter filter);
    }
}
