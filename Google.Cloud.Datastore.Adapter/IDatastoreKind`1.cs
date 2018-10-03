using Google.Cloud.Datastore.Adapter.Serialization;

namespace Google.Cloud.Datastore.Adapter
{
    public interface IDatastoreKind<TEntity> : IDatastoreKind<TEntity, long>
        where TEntity : DatastoreEntity
    {
    }
}
