using Google.Cloud.Datastore.V1;
using Spitfyr.GCP.Datastore.Adapter.Serialization;

namespace Spitfyr.GCP.Datastore.Adapter
{
    public interface IDatastoreKind<TEntity> : IDatastoreKind<TEntity, long>
        where TEntity : DatastoreEntity
    {
        IQueryOptions<TEntity> Where(Filter filter);
    }
}
