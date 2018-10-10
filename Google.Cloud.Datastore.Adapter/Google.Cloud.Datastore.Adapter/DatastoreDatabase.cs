using Google.Cloud.Datastore.Adapter.Serialization;
using Google.Cloud.Datastore.Adapter.Validators;
using Google.Cloud.Datastore.V1;

namespace Google.Cloud.Datastore.Adapter
{
    public sealed class DatastoreDatabase : IDatastoreDatabase
    {
        private readonly DatastoreDb _datastoreDb;

        public DatastoreDatabase(DatastoreDb datastoreDb)
        {
            _datastoreDb = Ensure.IsNotNull(datastoreDb, nameof(datastoreDb));
        }

        public IDatastoreKind<TEntity> GetKind<TEntity>(string name)
            where TEntity : DatastoreEntity
        {
            Ensure.IsNotNullOrEmpty(name, nameof(name));

            return new DatastoreKind<TEntity>(_datastoreDb);
        }
    }
}
