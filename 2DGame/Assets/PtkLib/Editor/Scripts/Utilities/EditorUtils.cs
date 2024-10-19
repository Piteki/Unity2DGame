using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;


namespace Ptk.Editors
{
	public static class EditorUtils
	{
		/// <summary>
		/// SerializedProperty から MemberInfo を取得
		/// </summary>
		/// <param name="serializedProperty"></param>
		/// <returns></returns>
		public static MemberInfo GetPropertyOrFieldMemberInfo( this SerializedProperty serializedProperty )
		{
			if( serializedProperty == null ){ return null; }
			var serializedObject = serializedProperty.serializedObject;
			if( serializedObject == null ){ return null; }
			var targetObject = serializedObject.targetObject;
			if( targetObject == null ){ return null; }
			var type = targetObject.GetType();
			if( type == null ){ return null; }

			foreach (var element in serializedProperty.propertyPath.Split('.'))
			{
				var fieldInfo = type.GetField( element, (BindingFlags)(-1) );
				if (fieldInfo != null)
				{
					return fieldInfo;
				}

				var propertyInfo = type.GetProperty( element, (BindingFlags)(-1) );
				if (propertyInfo != null)
				{
					return propertyInfo;
				}
			}
			return null;
		}

		/// <summary>
		/// MemberInfo から GetCustomAttribute を取得
		/// </summary>
		/// <param name="serializedProperty"></param>
		/// <returns></returns>
		public static TAttribute GetCustomAttribute< TAttribute >( this MemberInfo memberInfo, bool inherit )
			where TAttribute : Attribute
		{
			if( memberInfo == null ){ return null; }
			var attributes = memberInfo.GetCustomAttributes( typeof( TAttribute ), inherit ) as TAttribute[];
			return attributes?.FirstOrDefault();
		}
	}

}