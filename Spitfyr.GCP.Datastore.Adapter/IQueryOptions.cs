using System;
using System.Linq.Expressions;
using Google.Cloud.Datastore.V1;
using Spitfyr.GCP.Datastore.Adapter.Serialization;

namespace Spitfyr.GCP.Datastore.Adapter
{
    public interface IQueryOptions<TEntity> where TEntity : DatastoreEntity
    {
        IDatastoreKind<TEntity> GetKind();

        Options GetOptions();

        IQueryOptions<TEntity> Sort<TProp>(Expression<Func<TEntity, TProp>> field, PropertyOrder.Types.Direction order);

        IQueryOptions<TEntity> Skip(int? skip);

        IQueryOptions<TEntity> Limit(int? limit);

        //long Count();

        //Task<long> CountAsync();
    }
}
