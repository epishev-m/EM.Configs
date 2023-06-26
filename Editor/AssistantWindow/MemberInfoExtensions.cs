namespace EM.Configs.Editor
{

using System;
using System.Reflection;

public static class MemberInfoExtensions
{
	public static Type GetValueType(this MemberInfo memberInfo)
	{
		switch (memberInfo.MemberType)
		{
			case MemberTypes.Field:
			{
				var fieldInfo = (FieldInfo) memberInfo;

				return fieldInfo.FieldType;
			}
			case MemberTypes.Property:
			{
				var propertyInfo = (PropertyInfo) memberInfo;

				return propertyInfo.PropertyType;
			}
			case MemberTypes.All:
			case MemberTypes.Constructor:
			case MemberTypes.Custom:
			case MemberTypes.Event:
			case MemberTypes.Method:
			case MemberTypes.NestedType:
			case MemberTypes.TypeInfo:
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public static object GetValue(this MemberInfo memberInfo,
		object obj)
	{
		switch (memberInfo.MemberType)
		{
			case MemberTypes.Field:
			{
				var fieldInfo = (FieldInfo) memberInfo;

				return fieldInfo.GetValue(obj);
			}
			case MemberTypes.Property:
			{
				var propertyInfo = (PropertyInfo) memberInfo;

				return propertyInfo.GetValue(obj);
			}
			case MemberTypes.All:
			case MemberTypes.Constructor:
			case MemberTypes.Custom:
			case MemberTypes.Event:
			case MemberTypes.Method:
			case MemberTypes.NestedType:
			case MemberTypes.TypeInfo:
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public static void SetValue(this MemberInfo memberInfo,
		object obj,
		object value)
	{
		switch (memberInfo.MemberType)
		{
			case MemberTypes.Field:
			{
				var fieldInfo = (FieldInfo) memberInfo;
				fieldInfo.SetValue(obj, value);
				
				break;
			}
			case MemberTypes.Property:
			{
				var propertyInfo = (PropertyInfo) memberInfo;
				propertyInfo.SetValue(obj, value);
				
				break;
			}
			case MemberTypes.All:
			case MemberTypes.Constructor:
			case MemberTypes.Custom:
			case MemberTypes.Event:
			case MemberTypes.Method:
			case MemberTypes.NestedType:
			case MemberTypes.TypeInfo:
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
}

}