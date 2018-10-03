using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Datastore.Adapter.Serialization;
using Google.Cloud.Datastore.V1;

namespace Google.Cloud.Datastore.Adapter
{
    internal class DatastoreKind<TEntity, TKey> : DatastoreKindBase<TEntity, TKey>, IDatastoreKind<TEntity, TKey> 
        where TEntity : DatastoreEntity
    {
        public DatastoreKind(DatastoreDb database)
            : base(database) 
        {
        }

        public IQueryable<TEntity> AsQueryable()
        {
            return GetAll().AsQueryable();
        }

        public IEnumerable<TEntity> Find(Filter filter)
        {
            var query = new Query(Kind)
            {
                Filter = filter
            };

            var result = Database.RunQuery(query);
            return result.Entities.Select(BuildDalEntity);
        }

        public Task FindOneAndReplaceAsync(TEntity entity)
        {
            return Database.UpdateAsync(BuildEntity(entity));
        }

        public async Task<IEnumerable<TEntity>> FindAsync(Filter filter)
        {
            var query = new Query(Kind)
            {
                Filter = filter
            };

            var result = await Database.RunQueryAsync(query);
            return result.Entities.Select(BuildDalEntity);
        }

        public Task DeleteManyAsync(Filter filter)
        {
            throw new NotImplementedException();
        }
    }
}
