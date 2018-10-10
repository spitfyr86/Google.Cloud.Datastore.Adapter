using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Datastore.Adapter.Serialization;
using Google.Cloud.Datastore.V1;

namespace Google.Cloud.Datastore.Adapter
{
    internal class DatastoreKind<TEntity> : DatastoreKind<TEntity, long>, IDatastoreKind<TEntity> 
        where TEntity : DatastoreEntity
    {
        public DatastoreKind(DatastoreDb database)
            : base(database)
        {
        }

        public IQueryOptions<TEntity> Where(Filter filter)
        {
            return new QueryOptions<TEntity>(this, new Options());
        }
    }
}
