using System.Collections.Generic;

namespace System.Collections.Concurrent
{
  public class ConcurrentList<T> : IList<T>, IList
  {
    #region Properties

    private readonly List<T> _underlyingList = new List<T>();
    private readonly object _syncRoot = new object();
    private readonly ConcurrentQueue<T> _underlyingQueue;
    private bool _requiresSync;

    #endregion

    #region Constructor

    public ConcurrentList()
    {
      _underlyingQueue = new ConcurrentQueue<T>();
    }

    public ConcurrentList(IEnumerable<T> items)
    {
      _underlyingQueue = new ConcurrentQueue<T>(items);
    }

    #endregion

    #region IList interface implementation
    public IEnumerator<T> GetEnumerator()
    {
      lock (_syncRoot)
      {
        UpdateList();
        return _underlyingList.GetEnumerator();
      }
    }

    public void Add(T item)
    {
      if (_requiresSync)
      {
        lock (_syncRoot)
        {
          _underlyingQueue.Enqueue(item);
        }
      }
      else
      {
        _underlyingQueue.Enqueue(item);
      }
    }

    public int Add(object value)
    {
      CheckObjectType(value);

      Add((T)value);

      lock (_syncRoot)
      {
        UpdateList();
        return _underlyingList.IndexOf((T)value);
      }
    }

    public bool Contains(object value)
    {
      CheckObjectType(value);

      lock (_syncRoot)
      {
        UpdateList();
        return _underlyingList.Contains((T)value);
      }
    }

    public int IndexOf(object value)
    {
      CheckObjectType(value);

      lock (_syncRoot)
      {
        UpdateList();
        return _underlyingList.IndexOf((T)value);
      }
    }

    public void Insert(int index, object value)
    {
      CheckObjectType(value);

      lock (_syncRoot)
      {
        UpdateList();
        _underlyingList.Insert(index, (T)value);
      }
    }

    public void Remove(object value)
    {
      CheckObjectType(value);

      lock (_syncRoot)
      {
        UpdateList();
        _underlyingList.Remove((T)value);
      }
    }

    public void RemoveAt(int index)
    {
      lock (_syncRoot)
      {
        UpdateList();
        _underlyingList.RemoveAt(index);
      }
    }

    public T this[int index]
    {
      get { return _underlyingList[index]; }
      set { _underlyingList[index] = value; }
    }

    public void Clear()
    {
      lock (_syncRoot)
      {
        UpdateList();
        _underlyingList.Clear();
      }
    }

    public bool Contains(T item)
    {
      lock (_syncRoot)
      {
        UpdateList();
        return _underlyingList.Contains(item);
      }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      lock (_syncRoot)
      {
        UpdateList();
        _underlyingList.CopyTo(array, arrayIndex);
      }
    }

    public bool Remove(T item)
    {
      lock (_syncRoot)
      {
        UpdateList();
        return _underlyingList.Remove(item);
      }
    }

    public void CopyTo(Array array, int index)
    {
      lock (_syncRoot)
      {
        UpdateList();
        _underlyingList.CopyTo((T[])array, index);
      }
    }

    public int Count
    {
      get
      {
        lock (_syncRoot)
        {
          UpdateList();
          return _underlyingList.Count;
        }
      }
    }

    public int IndexOf(T item)
    {
      lock (_syncRoot)
      {
        UpdateList();
        return _underlyingList.IndexOf(item);
      }
    }

    public void Insert(int index, T item)
    {
      lock (_syncRoot)
      {
        UpdateList();
        _underlyingList.Insert(index, item);
      }
    }

    public object SyncRoot
    {
      get { return _syncRoot; }
    }

    public bool IsSynchronized
    {
      get { return true; }
    }

    public bool IsReadOnly
    {
      get { return false; }
    }

    public bool IsFixedSize
    {
      get { return false; }
    }
    #endregion

    #region Explicit interface implementation

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    T IList<T>.this[int index]
    {
      get
      {
        lock (_syncRoot)
        {
          UpdateList();
          return _underlyingList[index];
        }
      }

      set
      {
        lock (_syncRoot)
        {
          UpdateList();
          _underlyingList[index] = value;
        }
      }
    }

    object IList.this[int index]
    {
      get { return ((IList)this)[index]; }
      set { ((IList)this)[index] = (T)value; }
    }
    #endregion

    #region Helpers

    /// <summary>
    /// Moves concurrent queue items into the background generic list
    /// </summary>
    private void UpdateList()
    {
      lock (_syncRoot)
      {
        _requiresSync = true;
        T temp;
        while (_underlyingQueue.TryDequeue(out temp))
          _underlyingList.Add(temp);
        _requiresSync = false;
      }
    }

    /// <summary>
    /// Checks the type of value object and throws a more detailed exepction instead of InvalidCastException
    /// </summary>
    private void CheckObjectType(object value)
    {
      if (!(value is T))
        throw new ArgumentException(string.Format("Invalid parameter type {0}, expected type is {1}", value.GetType().Name, typeof(T)));
    }
    #endregion
  }
}
