using System.Linq;
using Google.Cloud.Datastore.V1;
using Spitfyr.GCP.Datastore.Adapter.Serialization;

namespace Spitfyr.GCP.Datastore.Adapter
{
    internal class DatastoreKind<TEntity, TKey> : DatastoreKindBase<TEntity, TKey>, IDatastoreKind<TEntity, TKey> 
        where TEntity : DatastoreEntity
    {
        public DatastoreKind(DatastoreDb database, string entityPrefix)
            : base(database, entityPrefix) 
        {
        }

        public IQueryable<TEntity> AsQueryable()
        {
            return GetAll().AsQueryable();
        }
    }
}
