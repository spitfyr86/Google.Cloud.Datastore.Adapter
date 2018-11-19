using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Datastore.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Spitfyr.GCP.Datastore.Adapter.Serialization;
using Value = Google.Cloud.Datastore.V1.Value;

namespace Spitfyr.GCP.Datastore.Adapter
{
    public abstract class DatastoreKindBase<TEntity, TKey> : IDatastoreKindBase<TEntity, TKey>
        where TEntity : DatastoreEntity
    {
        private readonly string _kind;
        private readonly KindKeyInfo _kindKeyInfo;
        private readonly DatastoreDb _database;

        protected DatastoreKindBase(DatastoreDb database, string entityPrefix)
        {
            _kindKeyInfo = GetKindKeyInfo();

            _database = database;
            _kind = GetKind(entityPrefix);
        }
        
        public async Task<TKey> InsertOneAsync(TEntity dsEntity)
        {
            var entity = BuildEntity(dsEntity);
            var keys = await _database.InsertAsync(new[] { entity });
            if (keys.Any() && keys[0] == null)
            {
                return GetId(dsEntity);
            }
            // Auto-generated key - set the key property under the entity
            var id = GetId(keys.FirstOrDefault());
            SetId(dsEntity, id);
            return id;
        }

        public async Task<TKey[]> InsertAsync(TEntity[] dalEntities)
        {
            var entities = dalEntities.Select(e => BuildEntity(e)).ToArray();
            var keys = await _database.InsertAsync(entities);
            if (keys.Any() && keys[0] == null)
            {
                return dalEntities.Select(GetId).ToArray();
            }

            // Auto-generated keys - set the keys properties under the entities
            var i = 0;
            var res = new List<TKey>();
            foreach (var key in keys)
            {
                var id = GetId(key);
                SetId(dalEntities[i], id);
                i++;
                res.Add(id);
            }
            return res.ToArray();
        }
        
        public Task FindOneAndReplaceAsync(TEntity entity)
        {
            return _database.UpdateAsync(BuildEntity(entity, true));
        }
        

        public async Task<TEntity> FindAsync(TKey id)
        {
            var key = BuildKey(id);
            var entity = await _database.LookupAsync(key);
            return entity != null ? BuildDalEntity(entity) : null;
        }

        public async Task<IEnumerable<TEntity>> FindInAsync(IDictionary<string, dynamic> fieldComparisons)
        {
            var gqlFilters = new List<string>();

            foreach (var comparison in fieldComparisons)
            {
                var field = comparison.Key;
                var values = comparison.Value;
                var localGqlFilters = new List<string>();

                if (gqlFilters.Count == 0)
                {
                    if (values is Array)
                    {
                        foreach (var val in values)
                        {
                            localGqlFilters.Add(val is string ? $"{field}='{val}'" : $"{field}={val}");
                        }
                    }
                    else
                    {
                        localGqlFilters.Add(values is string ? $"{field}='{values}'" : $"{field}={values}");
                    }
                }
                else
                {
                    foreach (var gql in gqlFilters)
                    {
                        if (values is Array)
                        {
                            foreach (var val in values)
                            {
                                localGqlFilters.Add(val is string ? $"{gql} AND {field}='{val}'" : $"{gql} AND {field}={val}");
                            }
                        }
                        else
                        {
                            localGqlFilters.Add(values is string ? $"{gql} AND {field}='{values}'" : $"{gql} AND {field}={values}");
                        }
                    }
                }

                gqlFilters = localGqlFilters;
            }

            var entities = new List<TEntity>();
            foreach (var filter in gqlFilters)
            {
                var q = new GqlQuery
                {
                    QueryString = $"SELECT * FROM {_kind} WHERE {filter}",
                    AllowLiterals = true
                };
                var results = await _database.RunQueryAsync(q);
                entities.AddRange(results.Entities.Select(BuildDalEntity));
            }

            return entities;
        }

        public async Task<IEnumerable<TEntity>> FindAsync(IQueryOptions<TEntity> queryOptions)
        {
            var options = queryOptions.GetOptions();

            var query = new Query(_kind)
            {
                //TODO: Handle multiple property sorts
                Order = { options.PropertyOrders },
            };

            //if (!string.IsNullOrEmpty(options.Skip))
            //    query.StartCursor = ByteString.FromBase64(options.Skip);

            if (options.Limit != null)
                query.Limit = options.Limit;

            var results = await _database.RunQueryAsync(query);
            return results.Entities.Select(BuildDalEntity);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            var query = new Query(_kind);
            return await FindAsync(query);
        }

        public async Task<IEnumerable<Key>> FindKeysAsync(Filter filter)
        {
            var query = new Query(_kind)
            {
                Filter = filter,
                Projection = { "__key__" }
            };
            var results = await _database.RunQueryAsync(query);
            return results.Entities.Select(x => x.Key);
        }

        public async Task<long> CountAsync(IQueryOptions<TEntity> queryOptions)
        {
            var query = new Query(_kind)
            {
                Projection = { "__key__" }
            };

            if (queryOptions != null)
            {
                var options = queryOptions.GetOptions();

                query = new Query(_kind)
                {
                    Filter = queryOptions.GetFilter(),
                    Projection = {"__key__"},
                    Order = {options.PropertyOrders},
                    Limit = options.Limit
                };
            }

            var results = await _database.RunQueryAsync(query);
            return results.Entities.Count;
        }

        public TKey InsertOne(TEntity dsEntity)
        {
            var entity = BuildEntity(dsEntity);
            var keys = _database.Insert(new[] { entity });
            if (keys.Any() && keys[0] == null)
            {
                return GetId(dsEntity);
            }
            // Auto-generated key - set the key property under the entity
            var id = GetId(keys.FirstOrDefault());
            SetId(dsEntity, id);
            return id;
        }

        public TKey[] Insert(TEntity[] dalEntities)
        {
            var entities = dalEntities.Select(e => BuildEntity(e)).ToArray();
            var keys = _database.Insert(entities);
            if (keys.Any() && keys[0] == null)
            {
                return dalEntities.Select(GetId).ToArray();
            }

            // Auto-generated keys - set the keys properties under the entities
            var i = 0;
            var res = new List<TKey>();
            foreach (var key in keys)
            {
                var id = GetId(key);
                SetId(dalEntities[i], id);
                i++;
                res.Add(id);
            }
            return res.ToArray();
        }

        public async Task<IEnumerable<TEntity>> FindAsync(Filter filter)
        {
            var query = new Query(_kind)
            {
                Filter = filter
            };
            return await FindAsync(query);
        }
        
        public async Task<IEnumerable<TEntity>> FindAsync(Query query)
        {
            var results = await _database.RunQueryAsync(query);
            return results.Entities.Select(BuildDalEntity);
        }


        public IEnumerable<TEntity> GetAll()
        {
            var query = new Query(_kind);
            return Find(query);
        }

        public IEnumerable<Key> FindKeys(Filter filter)
        {
            var query = new Query(_kind)
            {
                Filter = filter,
                Projection = { "__key__" }
            };
            var results = _database.RunQuery(query);
            return results.Entities.Select(x => x.Key);
        }

        public long Count(IQueryOptions<TEntity> queryOptions)
        {
            var query = new Query(_kind)
            {
                Projection = { "__key__" }
            };

            if (queryOptions != null)
            {
                var options = queryOptions.GetOptions();

                query = new Query(_kind)
                {
                    Filter = queryOptions.GetFilter(),
                    Projection = { "__key__" },
                    Order = { options.PropertyOrders },
                    Limit = options.Limit
                };
            }

            var results = _database.RunQuery(query);
            return results.Entities.Count;
        }

        public IEnumerable<TEntity> Find(IQueryOptions<TEntity> queryOptions)
        {
            var options = queryOptions.GetOptions();

            var query = new Query(_kind)
            {
                //TODO: Handle multiple property sorts
                Order = { options.PropertyOrders },
                Limit = options.Limit
            };

            //if (!string.IsNullOrEmpty(options.Skip))
            //    query.StartCursor = ByteString.FromBase64(options.Skip);

            if (options.Limit != null)
                query.Limit = options.Limit;

            var results = _database.RunQuery(query);
            return results.Entities.Select(BuildDalEntity);
        }

        public IEnumerable<TEntity> Find(Filter filter)
        {
            var query = new Query(_kind)
            {
                Filter = filter
            };
            return Find(query);
        }

        public IEnumerable<TEntity> Find(Query query)
        {
            var results = _database.RunQuery(query);
            return results.Entities.Select(BuildDalEntity);
        }


        public async Task DeleteOneAsync(TKey id)
        {
            var key = BuildKey(id);
            await _database.DeleteAsync(key);
        }
        
        public async Task DeleteAsync(TKey[] ids)
        {
            if (ids.Length > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(ids), "Bulk delete of more than 1000 entities is not allowed.");
            }
            var keys = ids.Select(BuildKey);
            await _database.DeleteAsync(keys);
        }
        
        public async Task DeleteManyAsync(Filter filter)
        {
            var keys = FindKeys(filter);
            await _database.DeleteAsync(keys);
        }

        public async Task UpdateAsync(TEntity obj)
        {
            var entity = BuildEntity(obj, true);
            await _database.UpdateAsync(entity);
        }
        
        public string GetPropertyName<TProp>(Expression<Func<TEntity, TProp>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            return memberExpression?.Member.Name;
        }


        #region Helper Methods

        private string CursorPaging(Query query, string pageCursor)
        {
            // [START datastore_cursor_paging]
            if (!string.IsNullOrEmpty(pageCursor))
                query.StartCursor = ByteString.FromBase64(pageCursor);

            return _database.RunQuery(query).EndCursor?.ToBase64();
            // [END datastore_cursor_paging]
        }

        public Entity BuildEntity(TEntity obj, bool isUpdate = false)
        {
            var entity = ToEntity(obj);
            entity.Key = GetKey(obj, isUpdate);
            return entity;
        }

        public TEntity BuildDalEntity(Entity entity)
        {
            var obj = FromEntity(entity, typeof(TEntity));

            if (_kindKeyInfo.IsAutoGenerated)
            {
                obj.GetType().GetProperty(_kindKeyInfo.KeyName)
                    .SetValue(obj, entity.Key.Path.First().Id);
            }
            else
            {
                obj.GetType().GetProperty(_kindKeyInfo.KeyName)
                    .SetValue(obj, entity.Key.Path.First().Name);
            }
            return obj as TEntity;
        }
        
        private string GetKind(string entityPrefix)
        {
            var kindAttribute = typeof(TEntity).GetTypeInfo().GetCustomAttribute(typeof(KindAttribute));

            var kindName = kindAttribute != null 
                ? ((KindAttribute)kindAttribute).Kind :
                typeof(TEntity).Name;

            return !string.IsNullOrEmpty(entityPrefix)
                ? $"{entityPrefix}{kindName}"
                : kindName;
        }

        private KindKeyInfo GetKindKeyInfo()
        {
            var properties = typeof(TEntity).GetProperties();
            var propertiesWithKeyAttributeCount = 0;
            var kindKeyInfo = new KindKeyInfo();

            foreach (var property in properties)
            {
                var keyAttribute = property.GetCustomAttribute<KindKeyAttribute>();
                if (keyAttribute != null)
                {
                    kindKeyInfo = new KindKeyInfo
                    {
                        IsAutoGenerated = keyAttribute.IsAutoGenerated,
                        KeyName = property.Name
                    };
                    propertiesWithKeyAttributeCount++;
                }
            }

            if (propertiesWithKeyAttributeCount > 1)
            {
                var errorMsg = propertiesWithKeyAttributeCount > 1 ? "Too many keys defined - only one allowed." : "Missing required key attribute - make sure you've added KindKey attribute on one of the properties.";
                throw new Exception(errorMsg);
            }

            return kindKeyInfo;
        }

        private TKey GetId(Key key)
        {
            object id;
            if (typeof(TKey) == typeof(long))
            {
                id = key.Path.First().Id;
            }
            else
            {
                id = key.Path.First().Name;
            }
            return (TKey)id;
        }

        private TKey GetId(TEntity obj)
        {
            var id = obj.GetType().GetProperties()
                .Where(x => x.Name == _kindKeyInfo.KeyName);

            return (TKey)id;
        }

        private void SetId(TEntity obj, TKey key)
        {
            var objType = obj.GetType();

            foreach (var toTypeProp in objType.GetProperties())
            {
                if (toTypeProp.Name == _kindKeyInfo.KeyName)
                {
                    toTypeProp.SetValue(obj, key, null);
                }
            }
        }

        private Key BuildKey(TKey id)
        {
            var keyFactory = _database.CreateKeyFactory(_kind);
            return typeof(TKey) == typeof(long) 
                ? keyFactory.CreateKey(Convert.ToInt64(id)) 
                : keyFactory.CreateKey(Convert.ToString(id));
        }

        private Key GetKey(TEntity obj, bool isUpdate = false)
        {
            Key key;
            if (_kindKeyInfo.IsAutoGenerated && !isUpdate)
            {
                key = _database.CreateKeyFactory(_kind).CreateIncompleteKey();
                return key;
            }
            else
            {
                var objType = obj.GetType();

                foreach (var toTypeProp in objType.GetProperties())
                {
                    if (toTypeProp.Name == _kindKeyInfo.KeyName)
                    {
                        var id = toTypeProp.GetValue(obj);
                        key = BuildKey((TKey)id);

                        return key;
                    }
                }
            }
            return null;
        }
        
        private Entity ToEntity(TEntity obj)
        {
            var entity = new Entity();

            foreach (var property in obj.GetType().GetRuntimeProperties())
            {
                var propName = property.Name;
                var propValue = property.GetValue(obj);

                if (propName != _kindKeyInfo.KeyName) // Not the key
                {
                    entity.Properties.Add(propName, CreateValueFromObject(propValue));
                }
            }
            
            return entity;
        }

        private Value CreateValueFromObject(object obj, bool isExcludeFromIndex = false)
        {
            var value = new Value { ExcludeFromIndexes = isExcludeFromIndex };
            if (obj == null)
            {
                value.NullValue = NullValue.NullValue;
                return value;
            }

            var type = obj.GetType();
            if (type == typeof(string))
            {
                value.StringValue = (string)obj;
            }
            else if (type == typeof(int))
            {
                value.IntegerValue = (int)obj;
            }
            else if (type == typeof(long))
            {
                value.IntegerValue = (long)obj;
            }
            else if (type == typeof(double))
            {
                value.DoubleValue = (double)obj;
            }
            else if (type == typeof(float))
            {
                value.DoubleValue = (float)obj;
            }
            else if (type == typeof(bool))
            {
                value.BooleanValue = (bool)obj;
            }
            else if (type == typeof(DateTime))
            {
                value.TimestampValue = ((DateTime)obj).ToTimestamp();
            }
            else if (type == typeof(DateTimeOffset))
            {
                value.TimestampValue = ((DateTimeOffset)obj).ToTimestamp();
            }
            else if (type.GetTypeInfo().IsEnum)
            {
                value.IntegerValue = (int)obj;
            }
            else if (typeof(ICollection).IsAssignableFrom(type))
            {
                var list = obj as ICollection;
                value.ArrayValue = new ArrayValue();

                if (list != null) //TODO: Otherwise should add warn logs
                {
                    foreach (var item in list)
                    {
                        value.ArrayValue.Values.Add(CreateValueFromObject(item));
                    }
                }
            }
            else if (type.GetTypeInfo().IsClass)
            {
                value.EntityValue = new Entity();

                foreach (var property in type.GetRuntimeProperties())
                {
                    var propName = property.Name;
                    var propValue = property.GetValue(obj);
                    value.EntityValue.Properties.Add(propName, CreateValueFromObject(propValue)); // TODO: Should call ToEntity - but should have depth consideration in regarding to the key
                }
            }

            return value;
        }

        private object FromEntity(Entity entity, System.Type toEntityType)
        {
            var obj = Activator.CreateInstance(toEntityType);

            foreach (var property in entity.Properties)
            {
                foreach (var toTypeProp in toEntityType.GetProperties())
                {
                    if (property.Key == toTypeProp.Name)
                    {
                        System.Type thisEntityType = null;
                        if (toTypeProp.PropertyType.GetTypeInfo().IsSubclassOf(typeof(DatastoreEntity)))
                        {
                            thisEntityType = toTypeProp.PropertyType;
                        }

                        var objectFromValue = CreateObjectFromValue(property.Value, toTypeProp.PropertyType, thisEntityType);
                        toTypeProp.SetValue(obj, objectFromValue, null);
                        break;
                    }
                }

            }

            return obj;
        }

        private object CreateObjectFromValue(Value value, System.Type requestedType, System.Type thisEntityType = null)
        {
            if (value.ValueTypeCase == Value.ValueTypeOneofCase.StringValue)
            {
                return value.StringValue;
            }
            if (value.ValueTypeCase == Value.ValueTypeOneofCase.IntegerValue)
            {
                if (requestedType == typeof(int) || requestedType == typeof(int?))
                {
                    return (int)value.IntegerValue;
                }
                if (requestedType.GetTypeInfo().IsEnum)
                {
                    return (int)value.IntegerValue;
                }
                return value.IntegerValue;
            }
            if (value.ValueTypeCase == Value.ValueTypeOneofCase.BooleanValue)
            {
                return value.BooleanValue;
            }
            if (value.ValueTypeCase == Value.ValueTypeOneofCase.DoubleValue)
            {
                return value.DoubleValue;
            }
            if (value.ValueTypeCase == Value.ValueTypeOneofCase.TimestampValue)
            {
                if (requestedType == typeof(DateTimeOffset))
                {
                    return value.TimestampValue.ToDateTimeOffset();
                }
                return value.TimestampValue.ToDateTime();
            }
            if (value.ValueTypeCase == Value.ValueTypeOneofCase.EntityValue)
            {
                if (thisEntityType != null)
                {
                    return FromEntity(value.EntityValue, thisEntityType);
                }
                return FromEntity(value.EntityValue, requestedType);
            }
            if (value.ValueTypeCase == Value.ValueTypeOneofCase.ArrayValue)
            {
                var requestedTypeInfo = requestedType.GetTypeInfo();
                var isDictionary = requestedTypeInfo.IsGenericType 
                                   && requestedTypeInfo.GetGenericTypeDefinition() == typeof(IDictionary<,>);

                if (isDictionary)
                {
                    var t1 = requestedType.GetGenericArguments()[0];
                    var t2 = requestedType.GetGenericArguments()[1];
                    var dictType = typeof(Dictionary<,>).MakeGenericType(t1, t2);
                    var dictionary = (IDictionary)Activator.CreateInstance(dictType);

                    // TODO: Implement mapping of Dicitionary collection here.
                    foreach (var item in value.ArrayValue.Values)
                    {
                    }

                    return dictionary;
                }

                var list = (IList)Activator.CreateInstance(requestedType);
                var listArgType = requestedType.GetGenericArguments()[0];
                foreach (var item in value.ArrayValue.Values)
                {
                    list.Add(CreateObjectFromValue(item, listArgType));
                }

                return list;
            }
            return null;
        }

        private bool IsExcludeColumnFromIndex(IEnumerable<CustomAttributeData> customAttributes)
        {
            return customAttributes.Any(a => a.AttributeType == typeof(ExcludeIndexAttribute));
        }

        #endregion
    }
}
