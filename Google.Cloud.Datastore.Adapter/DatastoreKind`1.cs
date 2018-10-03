using Google.Cloud.Datastore.Adapter.Serialization;
using Google.Cloud.Datastore.V1;

namespace Google.Cloud.Datastore.Adapter
{
    internal sealed class DatastoreKind<TEntity> : DatastoreKind<TEntity, long>, IDatastoreKind<TEntity> 
        where TEntity : DatastoreEntity
    {
        public DatastoreKind(DatastoreDb database)
            : base(database)
        {
        }
    }
}
