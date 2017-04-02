using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Google.Apis.Util.Store;
using System.Threading.Tasks;
using Newtonsoft.Json;


public class GoogleMemoryAuthStore : IDataStore
{
    private static ConcurrentDictionary<string, string> s_Items = new ConcurrentDictionary<string, string>();

    public async Task ClearAsync()
    {
        //using (var context = new GoogleAuthContext())
        //{
        //    var objectContext = ((IObjectContextAdapter)context).ObjectContext;
        //    await objectContext.ExecuteStoreCommandAsync("TRUNCATE TABLE [Items]");
        //}

        s_Items.Clear();
    }

    public async Task DeleteAsync<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key MUST have a value");
        }

        //using (var context = new GoogleAuthContext())
        //{
        //    var generatedKey = GenerateStoredKey(key, typeof(T));
        //    var item = context.Items.FirstOrDefault(x => x.Key == generatedKey);
        //    if (item != null)
        //    {
        //        context.Items.Remove(item);
        //        await context.SaveChangesAsync();
        //    }
        //}

        string outVal;
        s_Items.TryRemove(key, out outVal);
    }

    public Task<T> GetAsync<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key MUST have a value");
        }

        //using (var context = new GoogleAuthContext())
        //{
        //    var generatedKey = GenerateStoredKey(key, typeof(T));
        //    var item = context.Items.FirstOrDefault(x => x.Key == generatedKey);
        //    T value = item == null ? default(T) : JsonConvert.DeserializeObject<T>(item.Value);
        //    return Task.FromResult<T>(value);
        //}
        string outVal;
        T value;
        value = s_Items.TryGetValue(key, out outVal) ? JsonConvert.DeserializeObject<T>(outVal) : default(T);
        return Task.FromResult<T>(value);
    }

    public async Task StoreAsync<T>(string key, T value)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key MUST have a value");
        }

        //using (var context = new GoogleAuthContext())
        //{
        //    var generatedKey = GenerateStoredKey(key, typeof(T));
        //    string json = JsonConvert.SerializeObject(value);

        //    var item = await context.Items.SingleOrDefaultAsync(x => x.Key == generatedKey);

        //    if (item == null)
        //    {
        //        context.Items.Add(new Item { Key = generatedKey, Value = json });
        //    }
        //    else
        //    {
        //        item.Value = json;
        //    }

        //    await context.SaveChangesAsync();
        //}
        string store = JsonConvert.SerializeObject(value);
        s_Items.AddOrUpdate(key, store, (x, y) => store);
    }

    private static string GenerateStoredKey(string key, Type t)
    {
        return string.Format("{0}-{1}", t.FullName, key);
    }
}