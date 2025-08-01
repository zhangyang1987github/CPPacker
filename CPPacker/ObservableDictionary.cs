using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CPPacker
{
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private Dictionary<TKey, TValue> _map = new Dictionary<TKey, TValue>();


        private ObservableCollection<KeyValuePair<TKey, TValue>> _items = new ObservableCollection<KeyValuePair<TKey, TValue>>();



        public TValue this[TKey key]
        {
            get
            {
                try
                {
                    return this._map[key];
                }
                catch
                {
                    return default(TValue);
                }
            }
            set
            {


                if (this._map.ContainsKey(key) && object.Equals(this._map[key], value))
                    return;

                bool isAdd = !this._map.ContainsKey(key);

                if (isAdd)
                {
                    this.Add(key, value);
                }
                else
                {
                    this._map[key] = value;



                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[]"));

                    for (int i = 0; i < this._items.Count; i++)
                    {
                        var item = this._items[i];
                        if (object.Equals(item.Key, key))
                        {
                            this._items[i] = new KeyValuePair<TKey, TValue>(item.Key, value);
                            break;
                        }

                    }


                }

            }
        }

        public ICollection<TKey> Keys => this._map.Keys;

        public ICollection<TValue> Values => this._map.Values;

        public int Count => this._map.Count;

        public bool IsReadOnly => false;

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                this._items.CollectionChanged += value;
            }
            remove
            {
                this._items.CollectionChanged -= value;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void Add(TKey key, TValue value)
        {
            this._map.Add(key, value);
            this._items.Add(new KeyValuePair<TKey, TValue>(key, value));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[]"));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Count"));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Keys"));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Values"));
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            if (this._map.Count > 0)
            {
                var oldList = this._map.ToArray();
                this._map.Clear();
                this._items.Clear();
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[]"));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Count"));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Keys"));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Values"));
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this._map.Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return this._map.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this._items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this._map.GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            foreach (var item in this._items)
            {
                if (object.Equals(item.Key, key))
                {
                    var ret = this._map.Remove(key);
                    if (ret)
                    {

                        this._items.Remove(item);
                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[]"));
                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Count"));
                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Keys"));
                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Values"));
                    }
                    return ret;
                }
            }
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.Remove(item.Key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this._map.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        // 辅助方法：转义键名中的特殊字符（如引号）
        private string EscapeKeyForBinding(string key)
        {
            // 处理包含双引号的键（XAML中需用单引号包裹）
            if (key.Contains("\""))
                return $"'{key}'";
            return $"\"{key}\"";
        }


    }

}
