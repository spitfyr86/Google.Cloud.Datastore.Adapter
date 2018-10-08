using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Datastore.Adapter.Serialization;
using Google.Cloud.Datastore.V1;

namespace Google.Cloud.Datastore.Adapter
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
