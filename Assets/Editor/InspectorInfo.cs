using System;
using System.Reflection;

public class InspectorInfo : IComparable
{
    public bool CanWrite { get; private set; }
    public string Name { get; private set; }
    public Type InspectorType { get; private set; }
    private readonly PropertyInfo _propertyInfo;
    private readonly FieldInfo _fieldInfo;

    public InspectorInfo(PropertyInfo propertyInfo)
    {
        _propertyInfo = propertyInfo;

        CanWrite = propertyInfo.CanWrite;
        Name = propertyInfo.Name;
        InspectorType = propertyInfo.PropertyType;
    }
    public InspectorInfo(FieldInfo fieldInfo)
    {
        _fieldInfo = fieldInfo;

        CanWrite = true;
        Name = fieldInfo.Name;
        InspectorType = fieldInfo.FieldType;
    }
    public object GetValue(object obj)
    {
        return _propertyInfo != null ? _propertyInfo.GetValue(obj, null) : _fieldInfo.GetValue(obj);
    }
    public void SetValue(object obj, object value)
    {
        if (_propertyInfo != null)
            _propertyInfo.SetValue(obj, value, null);
        else
            _fieldInfo.SetValue(obj, value);
    }
    public int CompareTo(object a)
    {
        var c = (InspectorInfo)a;
        return string.Compare(Name, c.Name);
    }
}