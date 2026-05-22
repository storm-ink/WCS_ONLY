using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs
{
    public class RedisOperationRepository
    {
        //var redisConfiguration = configuration["Redis:Configuration"];
        //var redis = ConnectionMultiplexer.Connect(redisConfiguration);
        private readonly ConnectionMultiplexer connection;
        private readonly IDatabase database;
        public RedisOperationRepository(ConnectionMultiplexer connection)
        {
            this.connection = connection;
            this.database = connection.GetDatabase();
        }
        private IServer GetServer()
        {
            var endpoint = connection.GetEndPoints();
            return connection.GetServer(endpoint.First());
        }

        public async Task Clear()
        {
            foreach (var endPoint in connection.GetEndPoints())
            {
                var server = GetServer();
                foreach (var key in server.Keys())
                {
                    await database.KeyDeleteAsync(key);
                }
            }
        }

        #region --KEY/VALUE存取--
        /// <summary>
        /// 单条存值
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool StringSet(string key, string value)
        {
            return database.StringSet(key, value);
        }
        /// <summary>
        /// 保存单个key value
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="value">保存的值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns></returns>
        public bool StringSet(string key, string value, TimeSpan? expiry = default)
        {
            return database.StringSet(key, value, expiry);
        }
        /// <summary>
        /// 保存多个key value
        /// </summary>
        /// <param name="arr">key</param>
        /// <returns></returns>
        public bool StringSet(KeyValuePair<RedisKey, RedisValue>[] arr)
        {
            return database.StringSet(arr);
        }
        /// <summary>
        /// 批量存值
        /// </summary>
        /// <param name="keysStr">key</param>
        /// <param name="valuesStr">The value.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool StringSetMany(string[] keysStr, string[] valuesStr)
        {
            var count = keysStr.Length;
            var keyValuePair = new KeyValuePair<RedisKey, RedisValue>[count];
            for (int i = 0; i < count; i++)
            {
                keyValuePair[i] = new KeyValuePair<RedisKey, RedisValue>(keysStr[i], valuesStr[i]);
            }

            return database.StringSet(keyValuePair);
        }

        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool SetStringKey<T>(string key, T obj, TimeSpan? expiry = default)
        {
            string json = JsonConvert.SerializeObject(obj);
            return database.StringSet(key, json, expiry);
        }
        /// <summary>
        /// 追加值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void StringAppend(string key, string value)
        {
            ////追加值，返回追加后长度
            long appendlong = database.StringAppend(key, value);
        }

        /// <summary>
        /// 获取单个key的值
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <returns></returns>
        public RedisValue GetStringKey(string key)
        {
            return database.StringGet(key);
        }
        /// <summary>
        /// 根据Key获取值
        /// </summary>
        /// <param name="key">键值</param>
        /// <returns>System.String.</returns>
        public string StringGet(string key)
        {
            return database.StringGet(key);
        }

        /// <summary>
        /// 获取多个Key
        /// </summary>
        /// <param name="listKey">Redis Key集合</param>
        /// <returns></returns>
        public RedisValue[] GetStringKey(List<RedisKey> listKey)
        {
            return database.StringGet(listKey.ToArray());
        }
        /// <summary>
        /// 批量获取值
        /// </summary>
        public string[] StringGetMany(string[] keyStrs)
        {
            var count = keyStrs.Length;
            var keys = new RedisKey[count];
            var addrs = new string[count];

            for (var i = 0; i < count; i++)
            {
                keys[i] = keyStrs[i];
            }

            var values = database.StringGet(keys);
            for (var i = 0; i < values.Length; i++)
            {
                addrs[i] = values[i];
            }
            return addrs;

        }
        /// <summary>
        /// 获取一个key的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetStringKey<T>(string key)
        {
            return JsonConvert.DeserializeObject<T>(database.StringGet(key));
        }

        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public bool HashRemove(string key, string dataKey)
        {

            return database.HashDelete(key, dataKey);
        }

        /// <summary>
        /// 获取Hash类型的值并序列化
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public string HashGet(string key, string dataKey)
        {
            return database.HashGet(key, dataKey);
        }

        /// <summary>
        /// 获取Hash类型的值并序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public T HashGet<T>(string key, string dataKey)
        {
            if (database.HashExists(key, dataKey))
                return JsonConvert.DeserializeObject<T>(database.HashGet(key, dataKey));
            return default;
        }

        /// <summary>
        /// 获取Hash类型的所有值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public HashEntry[] HashGetAll(string key)
        {
            return database.HashGetAll(key);
        }
        #endregion

        #region --删除设置过期--
        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>是否删除成功</returns>
        public bool KeyDelete(string key)
        {
            return database.KeyDelete(key);
        }
        /// <summary>
        /// 删除多个key
        /// </summary>
        /// <param name="keys">rediskey</param>
        /// <returns>成功删除的个数</returns>
        public long KeyDelete(RedisKey[] keys)
        {
            return database.KeyDelete(keys);
        }
        /// <summary>
        /// 判断key是否存储
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns></returns>
        public bool KeyExists(string key)
        {
            return database.KeyExists(key);
        }
        /// <summary>
        /// 重新命名key
        /// </summary>
        /// <param name="key">就的redis key</param>
        /// <param name="newKey">新的redis key</param>
        /// <returns></returns>
        public bool KeyRename(string key, string newKey)
        {
            return database.KeyRename(key, newKey);
        }
        /// <summary>
        /// 删除hasekey
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <returns></returns>
        public bool HaseDelete(RedisKey key, RedisValue hashField)
        {
            return database.HashDelete(key, hashField);
        }


        ///// <summary>
        ///// 移除hash中的某值
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key"></param>
        ///// <param name="dataKey"></param>
        ///// <returns></returns>
        //public bool HashAdd(string key, string dataKey,object value)
        //{
        //    return database.HashSet(key, dataKey,value);
        //}
        /// <summary>
        /// 设置缓存过期
        /// </summary>
        /// <param name="key"></param>
        /// <param name="datetime"></param>
        public void SetExpire(string key, DateTime datetime)
        {
            database.KeyExpire(key, datetime);
        }
        /// <summary>
        /// 设置缓存过期
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        public void SetExpire(string key, TimeSpan? expiry = null)
        {
            database.KeyExpire(key, expiry);
        }

        #endregion
    }
}
