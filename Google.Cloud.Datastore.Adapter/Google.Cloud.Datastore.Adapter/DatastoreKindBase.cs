using Google.Cloud.Datastore.Adapter.Serialization;
using Google.Cloud.Datastore.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Value = Google.Cloud.Datastore.V1.Value;

namespace Google.Cloud.Datastore.Adapter
{
    public abstract class DatastoreKindBase<TEntity, TKey> : IDatastoreKindBase<TEntity, TKey>
        where TEntity : DatastoreEntity
    {
        private readonly KindKeyInfo _kindKeyInfo;
        private readonly TypesMetadataLoader _typesMetadataLoader;
        protected readonly string Kind;
        protected readonly DatastoreDb Database;

        protected DatastoreKindBase(DatastoreDb database)
        {
            _kindKeyInfo = GetKindKeyInfo();
            _typesMetadataLoader = new TypesMetadataLoader(typeof(TEntity));

            Database = database;
            Kind = GetKind();

        }
        
        public async Task<TKey> InsertOneAsync(TEntity dsEntity)
        {
            var entity = BuildEntity(dsEntity);
            var keys = await Database.InsertAsync(new[] { entity });
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
            var keys = await Database.InsertAsync(entities);
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
            return Database.UpdateAsync(BuildEntity(entity));
        }
        

        public async Task<TEntity> FindAsync(TKey id)
        {
            var key = BuildKey(id);
            var entity = await Database.LookupAsync(key);
            return entity != null ? BuildDalEntity(entity) : null;
        }

        public async Task<IEnumerable<TEntity>> FindInAsync(string field, dynamic[] values)
        {
            var entities = new List<TEntity>();

            foreach (var value in values)
            {
                var query = new Query(Kind)
                {
                    Filter = Filter.Equal(field, value)
                };
                var results = await Database.RunQueryAsync(query);
                entities.AddRange(results.Entities.Select(BuildDalEntity));
            }

            return entities;
        }

        public async Task<IEnumerable<TEntity>> FindAsync(IQueryOptions<TEntity> queryOptions)
        {
            var options = queryOptions.GetOptions();

            var query = new Query(Kind)
            {
                //TODO: Handle multiple property sorts
                Order = { options.PropertyOrders },
            };

            //if (!string.IsNullOrEmpty(options.Skip))
            //    query.StartCursor = ByteString.FromBase64(options.Skip);

            if (options.Limit != null)
                query.Limit = options.Limit;

            var results = await Database.RunQueryAsync(query);
            return results.Entities.Select(BuildDalEntity);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            var query = new Query(Kind);
            return await FindAsync(query);
        }

        public async Task<long> CountAsync(IQueryOptions<TEntity> queryOptions)
        {
            var query = new Query(Kind)
            {
                Projection = { "__key__" }
            };

            var results = await Database.RunQueryAsync(query);
            return results.Entities.Count;
        }

        public TKey InsertOne(TEntity dsEntity)
        {
            var entity = BuildEntity(dsEntity);
            var keys = Database.Insert(new[] { entity });
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
            var keys = Database.Insert(entities);
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
            var query = new Query(Kind)
            {
                Filter = filter
            };
            return await FindAsync(query);
        }
        
        public async Task<IEnumerable<TEntity>> FindAsync(Query query)
        {
            var results = await Database.RunQueryAsync(query);
            return results.Entities.Select(BuildDalEntity);
        }


        public IEnumerable<TEntity> GetAll()
        {
            var query = new Query(Kind);
            return Find(query);
        }

        public IEnumerable<Key> FindKeys(Filter filter)
        {
            var query = new Query(Kind)
            {
                Filter = filter,
                Projection = { "__key__" }
            };
            var results = Database.RunQuery(query);
            return results.Entities.Select(x => x.Key);
        }

        public IEnumerable<TEntity> Find(IQueryOptions<TEntity> queryOptions)
        {
            var options = queryOptions.GetOptions();

            var query = new Query(Kind)
            {
                //TODO: Handle multiple property sorts
                Order = { options.PropertyOrders },
            };

            //if (!string.IsNullOrEmpty(options.Skip))
            //    query.StartCursor = ByteString.FromBase64(options.Skip);

            if (options.Limit != null)
                query.Limit = options.Limit;

            var results = Database.RunQuery(query);
            return results.Entities.Select(BuildDalEntity);
        }

        public IEnumerable<TEntity> Find(Filter filter)
        {
            var query = new Query(Kind)
            {
                Filter = filter
            };
            return Find(query);
        }

        public IEnumerable<TEntity> Find(Query query)
        {
            var results = Database.RunQuery(query);
            return results.Entities.Select(BuildDalEntity);
        }


        public async Task DeleteOneAsync(TKey id)
        {
            var key = BuildKey(id);
            await Database.DeleteAsync(key);
        }
        
        public async Task DeleteAsync(TKey[] ids)
        {
            if (ids.Length > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(ids), "Bulk delete of more than 1000 entities is not allowed.");
            }
            var keys = ids.Select(BuildKey);
            await Database.DeleteAsync(keys);
        }
        
        public async Task DeleteManyAsync(Filter filter)
        {
            var keys = FindKeys(filter);
            await Database.DeleteAsync(keys);
        }

        public async Task UpdateAsync(TEntity obj)
        {
            var entity = BuildEntity(obj, true);
            await Database.UpdateAsync(entity);
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

            return Database.RunQuery(query).EndCursor?.ToBase64();
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
            var keyPropertyAccessors = GetKeyPropertyAccessors();
            if (_kindKeyInfo.IsAutoGenerated)
            {
                keyPropertyAccessors.Set(obj, entity.Key.Path.First().Id);
            }
            else
            {
                keyPropertyAccessors.Set(obj, entity.Key.Path.First().Name);
            }
            return obj as TEntity;
        }
        
        private string GetKind()
        {
            var kindAttribute = typeof(TEntity).GetCustomAttribute(typeof(KindAttribute));
            return kindAttribute != null ? ((KindAttribute)kindAttribute).Kind : nameof(TEntity);
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

            if (propertiesWithKeyAttributeCount != 1)
            {
                string errorMsg = propertiesWithKeyAttributeCount > 1 ? "Too many keys defined - only one allowed." : "Missing required key attribute - make sure you've added KindKey attribute on one of the properties.";
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
            var keyPropertyAccessors = GetKeyPropertyAccessors();
            var id = keyPropertyAccessors.Get(obj);
            return (TKey)id;
        }

        private void SetId(TEntity obj, TKey key)
        {
            var keyPropertyAccessors = GetKeyPropertyAccessors();
            keyPropertyAccessors.Set(obj, key);
        }

        private Key BuildKey(TKey id)
        {
            var keyFactory = Database.CreateKeyFactory(Kind);
            return typeof(TKey) == typeof(long) ? keyFactory.CreateKey(Convert.ToInt64(id)) : keyFactory.CreateKey(Convert.ToString(id));
        }

        private Key GetKey(TEntity obj, bool isUpdate = false)
        {
            Key key;
            if (_kindKeyInfo.IsAutoGenerated && !isUpdate)
            {
                key = Database.CreateKeyFactory(Kind).CreateIncompleteKey();
            }
            else
            {
                var keyPropertyAccessors = GetKeyPropertyAccessors();
                var id = keyPropertyAccessors.Get(obj);
                key = BuildKey((TKey)id);
            }
            return key;
        }

        private Accessors GetKeyPropertyAccessors()
        {
            var keyProperty = _typesMetadataLoader.GetTypeMetadata(typeof(TEntity)).PropertiesInfo[_kindKeyInfo.KeyName];
            return keyProperty;
        }

        private Entity ToEntity(object obj)
        {
            var entity = new Entity();

            var typeMetadata = _typesMetadataLoader.GetTypeMetadata(obj.GetType());

            foreach (var type in typeMetadata.PropertiesInfo)
            {
                var val = type.Value.Get(obj);
                if (type.Key != _kindKeyInfo.KeyName) // Not the key
                {
                    entity.Properties.Add(type.Key, CreateValueFromObject(val, IsExcludeColumnFromIndex(type.Value.CustomAttributes)));
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
            else if (type.IsEnum)
            {
                value.IntegerValue = (int)obj;
            }
            else if (typeof(ICollection).IsAssignableFrom(type))
            {
                var list = obj as ICollection;
                value.ArrayValue = new ArrayValue();
                var listType = _typesMetadataLoader.GetTypeMetadata(type);
                if (listType != null && list != null) //TODO: Otherwise should add warn logs
                {
                    foreach (var item in list)
                    {
                        value.ArrayValue.Values.Add(CreateValueFromObject(item));
                    }
                }
            }
            else if (type.IsClass)
            {
                value.EntityValue = new Entity();
                var classMetadata = _typesMetadataLoader.GetTypeMetadata(type);
                if (classMetadata != null)
                {
                    if (!string.IsNullOrEmpty(classMetadata.InheritedClassType))
                    {
                        value.EntityValue.Properties.Add(TypesMetadataLoader.InheritedTypePropertyName, classMetadata.InheritedClassType);
                    }

                    foreach (var property in classMetadata.PropertiesInfo)
                    {
                        var val = property.Value.Get(obj);
                        value.EntityValue.Properties.Add(property.Key, CreateValueFromObject(val, IsExcludeColumnFromIndex(property.Value.CustomAttributes))); // TODO: Should call ToEntity - but should have depth consideration in regarding to the key
                    }
                }
            }

            return value;
        }

        private object FromEntity(Entity entity, System.Type toType)
        {
            var typeMetadata = _typesMetadataLoader.GetTypeMetadata(toType);
            var obj = typeMetadata.Constructor();

            foreach (var property in entity.Properties)
            {
                var propertyName = property.Key;
                if (!typeMetadata.PropertiesInfo.ContainsKey(propertyName))
                {
                    continue;
                    //Warning
                }
                var propertyAccessors = typeMetadata.PropertiesInfo[propertyName];
                propertyAccessors.Set(obj, CreateObjectFromValue(property.Value, propertyAccessors.Type));
            }

            return obj;
        }

        private object CreateObjectFromValue(Value value, System.Type requestedType)
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
                if (requestedType.IsEnum)
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
                return value.TimestampValue.ToDateTime();
            }
            if (value.ValueTypeCase == Value.ValueTypeOneofCase.EntityValue)
            {
                if (value.EntityValue.Properties.ContainsKey(TypesMetadataLoader.InheritedTypePropertyName)) // Handle abstract class
                {
                    var inheritedTypeName = value.EntityValue.Properties[TypesMetadataLoader.InheritedTypePropertyName].StringValue;
                    var inheritedType = _typesMetadataLoader.GetTypeMetadata(inheritedTypeName);
                    return FromEntity(value.EntityValue, inheritedType);
                }
                return FromEntity(value.EntityValue, requestedType);
            }
            if (value.ValueTypeCase == Value.ValueTypeOneofCase.ArrayValue)
            {
                var typeMetadata = _typesMetadataLoader.GetTypeMetadata(requestedType) as ListMetadata;
                var obj = typeMetadata?.Constructor();
                foreach (var item in value.ArrayValue.Values)
                {
                    typeMetadata?.AddItem(obj, CreateObjectFromValue(item, typeMetadata.ItemsType));
                }

                return obj;
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
