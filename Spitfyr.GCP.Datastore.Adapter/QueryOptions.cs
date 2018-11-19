using System;
using System.Linq.Expressions;
using Google.Cloud.Datastore.V1;
using Google.Protobuf.Collections;
using Spitfyr.GCP.Datastore.Adapter.Serialization;

namespace Spitfyr.GCP.Datastore.Adapter
{
    public class QueryOptions<TEntity> : IQueryOptions<TEntity> where TEntity : DatastoreEntity
    {
        public QueryOptions(IDatastoreKind<TEntity> kind, Filter filter, Options options)
        {
            Kind = kind;
            Filter = filter;
            Options = options;
        }

        public IDatastoreKind<TEntity> Kind { get; }
        public Filter Filter { get; }
        public Options Options { get; private set; }

        public IDatastoreKind<TEntity> GetKind()
        {
            return Kind;
        }

        public Filter GetFilter()
        {
            return Filter;
        }

        public Options GetOptions()
        {
            return Options;
        }

        //TODO: Handle multiple property sorts
        public IQueryOptions<TEntity> Sort<TProp>(Expression<Func<TEntity, TProp>> field, PropertyOrder.Types.Direction sortOrder)
        {
            var memberExpression = field.Body as MemberExpression;
            var propName = memberExpression?.Member.Name;

            Options = new Options(Options)
            {
                PropertyOrders = {{ propName, PropertyOrder.Types.Direction.Descending }}
            };
            return this;
        }

        public IQueryOptions<TEntity> Skip(int? skip)
        {
            Options.Skip = skip;
            return this;
        }

        public IQueryOptions<TEntity> Limit(int? limit)
        {
            Options.Limit = limit;
            return this;
        }
        
    }
    
    public class Options
    {
        public Options()
        {
        }

        public Options(Options newParams)
        {
            Limit = newParams.Limit;
            Skip = newParams.Skip;
            PropertyOrders = newParams.PropertyOrders;
        }

        public int? Limit { get; set; }
        public int? Skip { get; set; }
        public RepeatedField<PropertyOrder> PropertyOrders { get; set; }
    }
}
