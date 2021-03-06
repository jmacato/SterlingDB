﻿using System;
using System.Collections.Generic;

namespace SterlingDB.Database
{
    /// <summary>
    ///     Helper class to resolve the types of elements stored in database tables.
    /// </summary>
    public class TableTypeResolver
    {
        private readonly Dictionary<string, Type> _resolvedTypes = new Dictionary<string, Type>();
        private readonly List<ISterlingTypeResolver> _typeResolvers = new List<ISterlingTypeResolver>();

        public void RegisterTypeResolver(ISterlingTypeResolver interceptor)
        {
            if (interceptor == null) throw new ArgumentNullException("interceptor");

            if (!_typeResolvers.Contains(interceptor)) _typeResolvers.Add(interceptor);
        }

        public Type ResolveTableType(string fullTypeName)
        {
            // TODO: searching for replacement type first makes unit testing possible, but isn't nice design
            var tableType = ResolveCachedType(fullTypeName) ??
                            ResolveReplacementType(fullTypeName) ??
                            ResolveOriginalType(fullTypeName);

            return tableType;
        }

        private Type ResolveCachedType(string fullTypeName)
        {
            Type result;
            _resolvedTypes.TryGetValue(fullTypeName, out result);
            return result;
        }

        private Type ResolveOriginalType(string fullTypeName)
        {
            Type result = null;

            try
            {
                result = Type.GetType(fullTypeName, false);
                CacheResolvedType(fullTypeName, result);
            }
            //catch (TypeLoadException) { }
            //catch (FileLoadException) { }
            catch (Exception)
            {
            }

            return result;
        }

        private Type ResolveReplacementType(string fullTypeName)
        {
            Type result = null;
            foreach (var typeResolver in _typeResolvers)
            {
                result = typeResolver.ResolveTableType(fullTypeName);
                if (result != null)
                {
                    CacheResolvedType(fullTypeName, result);
                    break;
                }
            }

            return result;
        }

        private void CacheResolvedType(string fullTypeName, Type resolvedType)
        {
            if (resolvedType != null) _resolvedTypes[fullTypeName] = resolvedType;
        }
    }
}