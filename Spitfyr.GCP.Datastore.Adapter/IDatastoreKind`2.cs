using System.Linq;
using Spitfyr.GCP.Datastore.Adapter.Serialization;

namespace Spitfyr.GCP.Datastore.Adapter
{
    public interface IDatastoreKind<TEntity, TKey> : IDatastoreKindBase<TEntity, TKey>
        where TEntity : DatastoreEntity
    {
        IQueryable<TEntity> AsQueryable();
    }
}
