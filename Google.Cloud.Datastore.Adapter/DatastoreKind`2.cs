using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    }
}
