using Spitfyr.GCP.Datastore.Adapter.Serialization;

namespace Spitfyr.GCP.Datastore.Adapter
{
    public interface IDatastoreDatabase
    {
        IDatastoreKind<TEntity> GetKind<TEntity>(string name) where TEntity : DatastoreEntity;
    }
}
