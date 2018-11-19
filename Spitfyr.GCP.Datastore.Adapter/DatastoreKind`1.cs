using Google.Cloud.Datastore.V1;
using Spitfyr.GCP.Datastore.Adapter.Serialization;

namespace Spitfyr.GCP.Datastore.Adapter
{
    internal class DatastoreKind<TEntity> : DatastoreKind<TEntity, long>, IDatastoreKind<TEntity> 
        where TEntity : DatastoreEntity
    {
        public DatastoreKind(DatastoreDb database, string entityPrefix)
            : base(database, entityPrefix)
        {
        }

        public IQueryOptions<TEntity> Where(Filter filter)
        {
            return new QueryOptions<TEntity>(this, filter, new Options());
        }
    }
}
