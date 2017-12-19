using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace DynAjax.Communication
{
  public class DictionaryCache<TKey, TData> : IEnumerable<TData> //where TKey : IEqualityComparer<TKey> does not work for string
  {
    private const int LifeSpanSeconds = 60;
    private ConcurrentList<ItemStore<TKey, TData>> Items { get; set; } = new ConcurrentList<ItemStore<TKey, TData>>();
    private List<TKey> Keys { get; set; } = new List<TKey>();
    public bool? Enabled { get; set; } = true;

    public int Count => Keys.Count;

    public bool? Expired
    {
      get
      {
        var rv = LastModified.HasValue ? DateTime.Now < LastModified.Value.AddSeconds(CacheLifeSpan) : (bool?)null;
        if (rv.HasValue && rv.Value) Reset();
        return rv;
      }
    }

    public DateTime? LastModified { get; set; }
    public int CacheLifeSpan { get; set; } = LifeSpanSeconds;
    public int ItemLifeSpan { get; set; } = LifeSpanSeconds;

    public void Reset()
    {
      LastModified = null;
      Items = new ConcurrentList<ItemStore<TKey, TData>>();
    }

    public void Add(TKey key, TData item)
    {
      var now = DateTime.Now;
      Items.Add(new ItemStore<TKey, TData>() { LifeSpan = ItemLifeSpan, LastModified = now, Item = item, ItemKey = key });
      LastModified = now;
      if (!Keys.Contains(key)) Keys.Add(key);
    }

    public void Update(TKey key, TData item)
    {
      var now = DateTime.Now;
      var store = Items.FirstOrDefault(i => i.ItemKey.Equals(key));
      if (store == null) throw new ArgumentOutOfRangeException(nameof(key), "Key is not found");
      store.Item = item;
      store.LastModified = now;
      LastModified = now;
      if (!Keys.Contains(key)) Keys.Add(key);
    }

    public TData this[int index]
    {
      get { return Items.FirstOrDefault(i => i.ItemKey.Equals(Keys[index])).Item; }
      set { SetItem(Keys[index], value); }
    }

    public bool ContainsKey(TKey key)
    {
      return Items.FirstOrDefault(i => i.ItemKey.Equals(key)) != null;
    }

    public int IndexOfKey(TKey key)
    {
      return Keys.IndexOf(key);
    }
    public int IndexOf(TData item)
    {
      if (!(item is IComparable)) return -1;
      var found = Items.FirstOrDefault(i => i.Item.Equals(item));
      return IndexOfKey(found.ItemKey);
    }

    public TData GetItem(TKey key)
    {
      // returns default if not found, Item property will be available
      return Items.FirstOrDefault(i => i.ItemKey.Equals(key)).Item;
    }
    public void SetItem(TKey key, TData item)
    {
      var store = Items.FirstOrDefault(i => i.ItemKey.Equals(key));
      if (store == null) Add(key, item);
      else Update(key, item);
    }
    public bool ItemExpired(TKey key)
    {
      return Items.FirstOrDefault(i => i.ItemKey.Equals(key))?.Expired ?? true;
    }

    #region Implementation of IEnumerable
    IEnumerator<TData> IEnumerable<TData>.GetEnumerator()
    {
      foreach (var item in Items)
      {
        yield return item.Item;
      }
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      foreach (var item in Items)
      {
        yield return item.Item;
      }
    }
    #endregion
  }
}
