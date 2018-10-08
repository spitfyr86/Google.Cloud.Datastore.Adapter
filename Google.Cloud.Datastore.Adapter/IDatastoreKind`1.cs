using Google.Cloud.Datastore.Adapter.Serialization;
using Google.Cloud.Datastore.V1;

namespace Google.Cloud.Datastore.Adapter
{
    public interface IDatastoreKind<TEntity> : IDatastoreKind<TEntity, long>
        where TEntity : DatastoreEntity
    {
        IQueryOptions<TEntity> Where(Filter filter);
    }
}
