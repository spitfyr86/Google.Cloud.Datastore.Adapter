using System.Collections.Generic;
using System.Threading.Tasks;
using Spitfyr.GCP.Datastore.Adapter.Serialization;

namespace Spitfyr.GCP.Datastore.Adapter
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
