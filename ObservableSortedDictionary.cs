using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WpfApp.Collections
{
    /// <summary>
    /// Represents a collection of key-value pairs sorted by key,
    /// that raises change notifications (suitable for DataGrid binding in WPF).
    /// </summary>
    public class ObservableSortedDictionary<TKey, TValue> : 
        IDictionary<TKey, TValue>, 
        INotifyCollectionChanged, 
        INotifyPropertyChanged
    {
        private readonly SortedList<TKey, TValue> _dictionary;

        public ObservableSortedDictionary()
        {
            _dictionary = new SortedList<TKey, TValue>();
        }

        public ObservableSortedDictionary(IComparer<TKey> comparer)
        {
            _dictionary = new SortedList<TKey, TValue>(comparer);
        }

        #region IDictionary Implementation

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                if (_dictionary.TryGetValue(key, out var oldValue))
                {
                    if (EqualityComparer<TValue>.Default.Equals(oldValue, value)) return;
                    
                    _dictionary[key] = value;
                    int index = _dictionary.IndexOfKey(key);
                    
                    OnPropertyChanged(nameof(Values));
                    OnPropertyChanged("Item[]");
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, 
                        new KeyValuePair<TKey, TValue>(key, value), 
                        new KeyValuePair<TKey, TValue>(key, oldValue), 
                        index));
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public ICollection<TKey> Keys => _dictionary.Keys;
        public ICollection<TValue> Values => _dictionary.Values;
        public int Count => _dictionary.Count;
        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            int index = _dictionary.IndexOfKey(key);

            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(Keys));
            OnPropertyChanged(nameof(Values));
            OnPropertyChanged("Item[]");
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, 
                new KeyValuePair<TKey, TValue>(key, value), 
                index));
        }

        public bool Remove(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var value))
            {
                int index = _dictionary.IndexOfKey(key);
                if (_dictionary.Remove(key))
                {
                    OnPropertyChanged(nameof(Count));
                    OnPropertyChanged(nameof(Keys));
                    OnPropertyChanged(nameof(Values));
                    OnPropertyChanged("Item[]");
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove, 
                        new KeyValuePair<TKey, TValue>(key, value), 
                        index));
                    return true;
                }
            }
            return false;
        }

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);
        public void Clear()
        {
            _dictionary.Clear();
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(Keys));
            OnPropertyChanged(nameof(Values));
            OnPropertyChanged("Item[]");
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        #endregion

        #region ICollection Implementation

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        public bool Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (Contains(item))
            {
                return Remove(item.Key);
            }
            return false;
        }

        #endregion

        #region IEnumerable Implementation

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region INotify events

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
