using System;
using System.Reflection;

namespace SterlingDB.Serialization
{
    /// <summary>
    ///     Abstraction of property or field
    /// </summary>
    public class PropertyOrField
    {
        private readonly FieldInfo _fieldInfo;
        private readonly ISterlingPlatformAdapter _platformAdapter;
        private readonly PropertyInfo _propertyInfo;

        public PropertyOrField(object infoObject, ISterlingPlatformAdapter platformAdapter)
        {
            if (infoObject == null) throw new ArgumentNullException("infoObject");

            if (infoObject is PropertyInfo)
                _propertyInfo = (PropertyInfo) infoObject;
            else if (infoObject is FieldInfo)
                _fieldInfo = (FieldInfo) infoObject;
            else
                throw new ArgumentException(string.Format("Invalid type: {0}", infoObject.GetType()), "infoObject");

            _platformAdapter = platformAdapter;
        }

        public Type PfType => _propertyInfo == null ? _fieldInfo.FieldType : _propertyInfo.PropertyType;

        public string Name => _propertyInfo == null ? _fieldInfo.Name : _propertyInfo.Name;

        public Type DeclaringType => _propertyInfo == null ? _fieldInfo.DeclaringType : _propertyInfo.DeclaringType;

        public Action<object, object> Setter
        {
            get
            {
                if (_propertyInfo != null)
                    return (obj, prop) => _platformAdapter.GetSetMethod(_propertyInfo).Invoke(obj, new[] {prop});

                return (obj, prop) => _fieldInfo.SetValue(obj, prop);
            }
        }

        public Func<object, object> Getter
        {
            get
            {
                if (_propertyInfo != null)
                    return obj => _platformAdapter.GetGetMethod(_propertyInfo).Invoke(obj, new object[] { });

                return obj => _fieldInfo.GetValue(obj);
            }
        }

        public object GetValue(object obj)
        {
            return _propertyInfo != null
                ? _platformAdapter.GetGetMethod(_propertyInfo).Invoke(obj, new object[] { })
                : _fieldInfo.GetValue(obj);
        }

        public override int GetHashCode()
        {
            return _propertyInfo == null ? _fieldInfo.GetHashCode() : _propertyInfo.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is PropertyOrField && ((PropertyOrField) obj).PfType.Equals(PfType);
        }

        public override string ToString()
        {
            return _propertyInfo == null ? _fieldInfo.ToString() : _propertyInfo.ToString();
        }
    }
}