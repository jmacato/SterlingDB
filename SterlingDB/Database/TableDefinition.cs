﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SterlingDB.Exceptions;
using SterlingDB.Indexes;
using SterlingDB.Keys;

namespace SterlingDB.Database
{
    /// <summary>
    ///     The definition of a table
    /// </summary>
    internal class TableDefinition<T, TKey> : ITableDefinition where T : class, new()
    {
        private readonly ISterlingDriver _driver;
        private readonly Func<TKey, T> _resolver;
        private Predicate<T> _isDirty;

        /// <summary>
        ///     Construct
        /// </summary>
        /// <param name="driver">Sterling driver</param>
        /// <param name="resolver">The resolver for the instance</param>
        /// <param name="key">The resolver for the key</param>
        public TableDefinition(ISterlingDriver driver, Func<TKey, T> resolver, Func<T, TKey> key)
        {
            _driver = driver;
            FetchKey = key;
            _resolver = resolver;
            _isDirty = obj => true;
            KeyList = new KeyCollection<T, TKey>(driver, resolver);
            Indexes = new Dictionary<string, IIndexCollection>();
        }

        /// <summary>
        ///     Function to fetch the key
        /// </summary>
        public Func<T, TKey> FetchKey { get; }

        /// <summary>
        ///     The key list
        /// </summary>
        public KeyCollection<T, TKey> KeyList { get; }

        /// <summary>
        ///     Get a new dictionary (creates the generic)
        /// </summary>
        /// <returns>The new dictionary instance</returns>
        public IDictionary GetNewDictionary()
        {
            return new Dictionary<TKey, int>();
        }

        /// <summary>
        ///     The index list
        /// </summary>
        public Dictionary<string, IIndexCollection> Indexes { get; }

        /// <summary>
        ///     Key list
        /// </summary>
        public IKeyCollection Keys => KeyList;

        /// <summary>
        ///     Table type
        /// </summary>
        public Type TableType => typeof(T);

        /// <summary>
        ///     Key type
        /// </summary>
        public Type KeyType => typeof(TKey);

        /// <summary>
        ///     Refresh key list
        /// </summary>
        public async Task RefreshAsync()
        {
            await KeyList.RefreshAsync().ConfigureAwait(false);

            foreach (var index in Indexes.Values) await index.RefreshAsync().ConfigureAwait(false);
        }

        /// <summary>
        ///     Fetch the key for the instance
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <returns>The key</returns>
        public object FetchKeyFromInstance(object instance)
        {
            return FetchKey((T) instance);
        }

        /// <summary>
        ///     Is the instance dirty?
        /// </summary>
        /// <returns>True if dirty</returns>
        public bool IsDirty(object instance)
        {
            return _isDirty((T) instance);
        }

        public void RegisterDirtyFlag(Predicate<T> isDirty)
        {
            _isDirty = isDirty;
        }

        /// <summary>
        ///     Registers an index with the table definition
        /// </summary>
        /// <typeparam name="TIndex">The type of the index</typeparam>
        /// <param name="name">A name for the index</param>
        /// <param name="indexer">The function to retrieve the index</param>
        public void RegisterIndex<TIndex>(string name, Func<T, TIndex> indexer)
        {
            if (Indexes.ContainsKey(name))
                throw new SterlingDuplicateIndexException(name, typeof(T), _driver.DatabaseInstanceName);

            var indexCollection = new IndexCollection<T, TIndex, TKey>(name, _driver, indexer, _resolver);

            Indexes.Add(name, indexCollection);
        }

        /// <summary>
        ///     Registers an index with the table definition
        /// </summary>
        /// <typeparam name="TIndex1">The type of the first index</typeparam>
        /// <typeparam name="TIndex2">The type of the second index</typeparam>
        /// <param name="name">A name for the index</param>
        /// <param name="indexer">The function to retrieve the index</param>
        public void RegisterIndex<TIndex1, TIndex2>(string name, Func<T, Tuple<TIndex1, TIndex2>> indexer)
        {
            if (Indexes.ContainsKey(name))
                throw new SterlingDuplicateIndexException(name, typeof(T), _driver.DatabaseInstanceName);

            var indexCollection = new IndexCollection<T, TIndex1, TIndex2, TKey>(name, _driver, indexer, _resolver);

            Indexes.Add(name, indexCollection);
        }
    }
}