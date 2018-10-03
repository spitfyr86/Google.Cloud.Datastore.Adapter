using Google.Cloud.Datastore.Adapter.Serialization;

namespace Google.Cloud.Datastore.Adapter
{
    public interface IDatastoreDatabase
    {
        IDatastoreKind<TEntity> GetKind<TEntity>(string name) where TEntity : DatastoreEntity;
    }
}
