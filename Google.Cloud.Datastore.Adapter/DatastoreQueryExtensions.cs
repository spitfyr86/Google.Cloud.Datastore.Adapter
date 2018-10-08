using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Datastore.Adapter.Serialization;
using Google.Cloud.Datastore.V1;

namespace Google.Cloud.Datastore.Adapter
{
    public static class DatastoreQueryExtensions
    {
        public static Task<IEnumerable<TEntity>> FindAsync<TEntity>(this IQueryOptions<TEntity> queryOptions) 
            where TEntity : DatastoreEntity
        {
            var kind = queryOptions.GetKind();
            return kind.FindAsync(queryOptions);
        }

        public static Task<long> CountAsync<TEntity>(this IQueryOptions<TEntity> queryOptions)
            where TEntity : DatastoreEntity
        {
            var kind = queryOptions.GetKind();
            return kind.CountAsync(queryOptions);
        }
    }
}
