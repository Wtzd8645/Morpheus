// http://unlicense.org
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Morpheus.Core.Collection
{

    /// <summary>
    /// A dictionary object that allows rapid hash lookups using keys, but also
    /// maintains the key insertion order so that values can be retrieved by
    /// key index.
    /// </summary>
    public class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>
    {

        #region Fields/Properties

        private KeyedCollection2<TKey, KeyValuePair<TKey, TValue>> keyedCollection;

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key associated with the value to get or set.</param>
        public TValue this[TKey key]
        {
            get
            {
                return GetValue(key);
            }
            set
            {
                SetValue(key, value);
            }
        }

        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <param name="index">The index of the value to get or set.</param>
        public TValue this[int index]
        {
            get
            {
                return GetItem(index).Value;
            }
            set
            {
                SetItem(index, value);
            }
        }

        public int Count
        {
            get { return keyedCollection.Count; }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return keyedCollection.Select(x => x.Key).ToList();
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return keyedCollection.Select(x => x.Value).ToList();
            }
        }

        public IEqualityComparer<TKey> Comparer
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        public OrderedDictionary()
        {
            Initialize();
        }

        public OrderedDictionary(IEqualityComparer<TKey> comparer)
        {
            Initialize(comparer);
        }

        public OrderedDictionary(IOrderedDictionary<TKey, TValue> dictionary)
        {
            Initialize();
            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                keyedCollection.Add(pair);
            }
        }

        public OrderedDictionary(IOrderedDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            Initialize(comparer);
            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                keyedCollection.Add(pair);
            }
        }

        #endregion

        #region Methods

        private void Initialize(IEqualityComparer<TKey> comparer = null)
        {
            Comparer = comparer;
            keyedCollection = comparer != null
                ? new KeyedCollection2<TKey, KeyValuePair<TKey, TValue>>(x => x.Key, comparer)
                : new KeyedCollection2<TKey, KeyValuePair<TKey, TValue>>(x => x.Key);
        }

        public void Add(TKey key, TValue value)
        {
            keyedCollection.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public void Clear()
        {
            keyedCollection.Clear();
        }

        public void Insert(int index, TKey key, TValue value)
        {
            keyedCollection.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
        }

        public int IndexOf(TKey key)
        {
            return keyedCollection.Contains(key) ? keyedCollection.IndexOf(keyedCollection[key]) : -1;
        }

        public bool ContainsValue(TValue value)
        {
            return Values.Contains(value);
        }

        public bool ContainsValue(TValue value, IEqualityComparer<TValue> comparer)
        {
            return Values.Contains(value, comparer);
        }

        public bool ContainsKey(TKey key)
        {
            return keyedCollection.Contains(key);
        }

        public KeyValuePair<TKey, TValue> GetItem(int index)
        {
            return index < 0 || index >= keyedCollection.Count
                ? throw new ArgumentException(String.Format("The index was outside the bounds of the dictionary: {0}", index))
                : keyedCollection[index];
        }

        /// <summary>
        /// Sets the value at the index specified.
        /// </summary>
        /// <param name="index">The index of the value desired</param>
        /// <param name="value">The value to set</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the index specified does not refer to a KeyValuePair in this object
        /// </exception>
        public void SetItem(int index, TValue value)
        {
            if (index < 0 || index >= keyedCollection.Count)
            {
                throw new ArgumentException($"The index is outside the bounds of the dictionary: {index}");
            }
            KeyValuePair<TKey, TValue> kvp = new KeyValuePair<TKey, TValue>(keyedCollection[index].Key, value);
            keyedCollection[index] = kvp;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return keyedCollection.GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            return keyedCollection.Remove(key);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= keyedCollection.Count)
            {
                throw new ArgumentException(String.Format("The index was outside the bounds of the dictionary: {0}", index));
            }
            keyedCollection.RemoveAt(index);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key associated with the value to get.</param>
        public TValue GetValue(TKey key)
        {
            if (keyedCollection.Contains(key) == false)
            {
                throw new ArgumentException($"The given key is not present in the dictionary: {key}");
            }
            KeyValuePair<TKey, TValue> kvp = keyedCollection[key];
            return kvp.Value;
        }

        /// <summary>
        /// Sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key associated with the value to set.</param>
        /// <param name="value">The the value to set.</param>
        public void SetValue(TKey key, TValue value)
        {
            KeyValuePair<TKey, TValue> kvp = new KeyValuePair<TKey, TValue>(key, value);
            int idx = IndexOf(key);
            if (idx > -1)
            {
                keyedCollection[idx] = kvp;
            }
            else
            {
                keyedCollection.Add(kvp);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (keyedCollection.Contains(key))
            {
                value = keyedCollection[key].Value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        #endregion

        #region sorting
        public void SortKeys()
        {
            keyedCollection.SortByKeys();
        }

        public void SortKeys(IComparer<TKey> comparer)
        {
            keyedCollection.SortByKeys(comparer);
        }

        public void SortKeys(Comparison<TKey> comparison)
        {
            keyedCollection.SortByKeys(comparison);
        }

        public void SortValues()
        {
            Comparer<TValue> comparer = Comparer<TValue>.Default;
            SortValues(comparer);
        }

        public void SortValues(IComparer<TValue> comparer)
        {
            keyedCollection.Sort((x, y) => comparer.Compare(x.Value, y.Value));
        }

        public void SortValues(Comparison<TValue> comparison)
        {
            keyedCollection.Sort((x, y) => comparison(x.Value, y.Value));
        }
        #endregion

        #region IDictionary<TKey, TValue>

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            Add(key, value);
        }

        bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
        {
            return ContainsKey(key);
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get { return Keys; }
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            return Remove(key);
        }

        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
        {
            return TryGetValue(key, out value);
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get { return Values; }
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get
            {
                return this[key];
            }
            set
            {
                this[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey, TValue>>

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            keyedCollection.Add(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            keyedCollection.Clear();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return keyedCollection.Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            keyedCollection.CopyTo(array, arrayIndex);
        }

        int ICollection<KeyValuePair<TKey, TValue>>.Count
        {
            get { return keyedCollection.Count; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return keyedCollection.Remove(item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey, TValue>>

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IOrderedDictionary

        IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator<TKey, TValue>(this);
        }

        void IOrderedDictionary.Insert(int index, object key, object value)
        {
            Insert(index, (TKey)key, (TValue)value);
        }

        void IOrderedDictionary.RemoveAt(int index)
        {
            RemoveAt(index);
        }

        object IOrderedDictionary.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (TValue)value;
            }
        }

        #endregion

        #region IDictionary

        void IDictionary.Add(object key, object value)
        {
            Add((TKey)key, (TValue)value);
        }

        void IDictionary.Clear()
        {
            Clear();
        }

        bool IDictionary.Contains(object key)
        {
            return keyedCollection.Contains((TKey)key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator<TKey, TValue>(this);
        }

        bool IDictionary.IsFixedSize
        {
            get { return false; }
        }

        bool IDictionary.IsReadOnly
        {
            get { return false; }
        }

        ICollection IDictionary.Keys
        {
            get { return (ICollection)Keys; }
        }

        void IDictionary.Remove(object key)
        {
            Remove((TKey)key);
        }

        ICollection IDictionary.Values
        {
            get { return (ICollection)Values; }
        }

        object IDictionary.this[object key]
        {
            get
            {
                return this[(TKey)key];
            }
            set
            {
                this[(TKey)key] = (TValue)value;
            }
        }

        #endregion

        #region ICollection

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)keyedCollection).CopyTo(array, index);
        }

        int ICollection.Count
        {
            get { return ((ICollection)keyedCollection).Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { return ((ICollection)keyedCollection).IsSynchronized; }
        }

        object ICollection.SyncRoot
        {
            get { return ((ICollection)keyedCollection).SyncRoot; }
        }

        #endregion
    }

    public class KeyedCollection2<TKey, TItem> : KeyedCollection<TKey, TItem>
    {
        private const string DelegateNullExceptionMessage = "Delegate passed cannot be null";
        private Func<TItem, TKey> getKeyForItemDelegate;

        public KeyedCollection2(Func<TItem, TKey> getKeyForItemDelegate)
            : base()
        {
            if (getKeyForItemDelegate == null)
            {
                throw new ArgumentNullException(DelegateNullExceptionMessage);
            }

            this.getKeyForItemDelegate = getKeyForItemDelegate;
        }

        public KeyedCollection2(Func<TItem, TKey> getKeyForItemDelegate, IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
            if (getKeyForItemDelegate == null)
            {
                throw new ArgumentNullException(DelegateNullExceptionMessage);
            }

            this.getKeyForItemDelegate = getKeyForItemDelegate;
        }

        protected override TKey GetKeyForItem(TItem item)
        {
            return getKeyForItemDelegate(item);
        }

        public void SortByKeys()
        {
            Comparer<TKey> comparer = Comparer<TKey>.Default;
            SortByKeys(comparer);
        }

        public void SortByKeys(IComparer<TKey> keyComparer)
        {
            Comparer2<TItem> comparer = new Comparer2<TItem>((x, y) => keyComparer.Compare(GetKeyForItem(x), GetKeyForItem(y)));
            Sort(comparer);
        }

        public void SortByKeys(Comparison<TKey> keyComparison)
        {
            Comparer2<TItem> comparer = new Comparer2<TItem>((x, y) => keyComparison(GetKeyForItem(x), GetKeyForItem(y)));
            Sort(comparer);
        }

        public void Sort()
        {
            Comparer<TItem> comparer = Comparer<TItem>.Default;
            Sort(comparer);
        }

        public void Sort(Comparison<TItem> comparison)
        {
            Comparer2<TItem> newComparer = new Comparer2<TItem>((x, y) => comparison(x, y));
            Sort(newComparer);
        }

        public void Sort(IComparer<TItem> comparer)
        {
            List<TItem> list = base.Items as List<TItem>;
            if (list != null)
            {
                list.Sort(comparer);
            }
        }
    }

    public class Comparer2<T> : Comparer<T>
    {
        //private readonly Func<T, T, int> compareFunction;
        private readonly Comparison<T> compareFunction;

        #region Constructors

        public Comparer2(Comparison<T> comparison)
        {
            if (comparison == null)
            {
                throw new ArgumentNullException("comparison");
            }

            compareFunction = comparison;
        }

        #endregion

        public override int Compare(T arg1, T arg2)
        {
            return compareFunction(arg1, arg2);
        }
    }

    public class DictionaryEnumerator<TKey, TValue> : IDictionaryEnumerator, IDisposable
    {
        private readonly IEnumerator<KeyValuePair<TKey, TValue>> impl;
        public void Dispose() { impl.Dispose(); }
        public DictionaryEnumerator(IDictionary<TKey, TValue> value)
        {
            impl = value.GetEnumerator();
        }
        public void Reset() { impl.Reset(); }
        public bool MoveNext() { return impl.MoveNext(); }
        public DictionaryEntry Entry
        {
            get
            {
                KeyValuePair<TKey, TValue> pair = impl.Current;
                return new DictionaryEntry(pair.Key, pair.Value);
            }
        }
        public object Key => impl.Current.Key;
        public object Value => impl.Current.Value;
        public object Current => Entry;
    }
}