﻿using System;
using System.Threading.Tasks;

namespace SterlingDB.Indexes
{
    /// <summary>
    ///     An individual table key
    /// </summary>
    /// <typeparam name="T">The class the key maps to</typeparam>
    /// <typeparam name="TIndex">The type of the index</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    public class TableIndex<T, TIndex, TKey> where T : class, new()
    {
        private readonly Func<TKey, T> _getter;
        private readonly int _hashCode;

        private Lazy<T> _lazyValue;

        //// <param name="getter">The getter</param>
        /// <summary>
        ///     Construct with how to get the key
        /// </summary>
        /// <param name="index">The index value</param>
        /// <param name="key">The associated key with the index</param>
        /// <param name="getter">Getter method for loading an instance</param>
        internal TableIndex(TIndex index, TKey key, Func<TKey, T> getter)
        {
            Index = index;
            Key = key;
            _hashCode = key.GetHashCode();
            _getter = getter;
            _lazyValue = new Lazy<T>(() => _getter(Key));
        }

        public TKey Key { get; }

        /// <summary>
        ///     Key
        /// </summary>
        public TIndex Index { get; internal set; }

        public Task<T> Value
        {
            get
            {
                lock (_getter)
                {
                    if (_lazyValue.IsValueCreated)
                        return Task.FromResult(_lazyValue.Value);
                    return Task<T>.Factory.StartNew(() => _lazyValue.Value);
                }
            }
        }

        /// <summary>
        ///     Refresh the lazy value
        /// </summary>
        public void Refresh()
        {
            lock (_getter)
            {
                _lazyValue = new Lazy<T>(() => _getter(Key));
            }
        }

        /// <summary>
        ///     Compares for equality
        /// </summary>
        /// <param name="obj">The object</param>
        /// <returns>True if equal</returns>
        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == _hashCode && ((TableIndex<T, TIndex, TKey>) obj).Key.Equals(Key);
        }

        /// <summary>
        ///     Hash code
        /// </summary>
        /// <returns>The has code of the key</returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>The key</returns>
        public override string ToString()
        {
            return string.Format("Index: [{0}][{1}]={2}", typeof(T).FullName, typeof(TIndex).FullName, Index);
        }
    }
}