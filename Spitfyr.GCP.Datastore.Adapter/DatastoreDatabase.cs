using Google.Cloud.Datastore.V1;
using Spitfyr.GCP.Datastore.Adapter.Serialization;
using Spitfyr.GCP.Datastore.Adapter.Validators;

namespace Spitfyr.GCP.Datastore.Adapter
{
    public sealed class DatastoreDatabase : IDatastoreDatabase
    {
        private readonly string _entityPrefix;
        private readonly DatastoreDb _datastoreDb;

        public DatastoreDatabase(DatastoreDb datastoreDb, string entityPrefix)
        {
            _entityPrefix = entityPrefix;
            _datastoreDb = Ensure.IsNotNull(datastoreDb, nameof(datastoreDb));
        }

        public IDatastoreKind<TEntity> GetKind<TEntity>(string name)
            where TEntity : DatastoreEntity
        {
            Ensure.IsNotNullOrEmpty(name, nameof(name));

            return new DatastoreKind<TEntity>(_datastoreDb, _entityPrefix);
        }
    }
}
