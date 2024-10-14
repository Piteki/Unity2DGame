using System;

namespace Ptk.IdStrings
{
	/// <summary>
	/// IdString Attribute
	/// </summary>
	/// <remarks>
	/// string 定数を int 型定数のように扱う IdString を定義するための属性。
	/// class や static property または static field に定義する事で Member Name を持つ IdString を定義することができる。
	/// 定義された IdString は Field または Property に格納される。
	/// また class の場合は class 名の IdString が定義され、実行時は IdString.Get< TClass >() で取得する事が可能。
	/// </remarks>
	[AttributeUsage(
		AttributeTargets.Property | AttributeTargets.Field
		| AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface
		, AllowMultiple = false
	)]
	public class IdStringAttribute : Attribute
	{
		public string Description { get; set; }
		public string Name { get; set; }
		public EIdStringParentNameType ParentNameType { get; set; } = EIdStringParentNameType.UseParentSetting;
		public EIdStringNamespaceType NamespaceType { get; set; } = EIdStringNamespaceType.UseParentSetting;
	}

	/// <summary>
	/// IdString 親階層名タイプ
	/// </summary>
	public enum EIdStringParentNameType
	{
		/// <summary> 親の設定を使用 </summary>
		UseParentSetting = 0,
		/// <summary> FullName </summary>
		FullName,
		/// <summary> なし </summary>
		None,
	};

	/// <summary>
	/// IdString Namespace タイプ
	/// </summary>
	public enum EIdStringNamespaceType
	{
		/// <summary> 親の設定を使用 </summary>
		UseParentSetting = 0,
		/// <summary> FullName </summary>
		FullName,
		/// <summary> なし </summary>
		None,
	};

	/// <summary>
	/// IdString Attribute Data 
	/// </summary>
	internal class IdStringAttrData : HierarchyNodeBase
	{
		public static EIdStringParentNameType DefaultParentNameType = EIdStringParentNameType.FullName;
		public static EIdStringNamespaceType DefaultNamespaceType = EIdStringNamespaceType.None;

		public string Description { get; set; }
		public string AttrName { get; set; }
		public bool HasHideInInspectorAttribute { get; set; }
		public EIdStringParentNameType ParentNameType { get; set; } = EIdStringParentNameType.UseParentSetting;
		public EIdStringNamespaceType NamespaceType { get; set; } = EIdStringNamespaceType.UseParentSetting;

		public string MemberName { get; set; }
		public string ParentPath { get; set; }

		public string ElementName => !string.IsNullOrEmpty( AttrName ) ? AttrName : MemberName;
		public string ParentFullPath { get; set; }

		public bool IsHideInInspector { get; set; }
		

		public IdString IdString { get; set; }

		public EIdStringParentNameType GetParentNameType()
		{
			var value = ParentNameType;
			var elem = this;
			while( value == EIdStringParentNameType.UseParentSetting )
			{
				elem = elem.Hierarchy.Parent as IdStringAttrData;
				if( elem == null )
				{
					value = DefaultParentNameType;
					break;
				}
				value = elem.ParentNameType;
			}
			return value;
		}
		public EIdStringNamespaceType GetNamespaceType()
		{
			var value = NamespaceType;
			var elem = this;
			while( value == EIdStringNamespaceType.UseParentSetting )
			{
				elem = elem.Hierarchy.Parent as IdStringAttrData;
				if( elem == null )
				{
					value = DefaultNamespaceType;
					break;
				}
				value = elem.NamespaceType;
			}
			return value;
		}

		public bool GetIsHideInInspector()
		{
			bool value = HasHideInInspectorAttribute;
			var elem = this;
			while( !value )
			{
				elem = elem.Hierarchy.Parent as IdStringAttrData;
				if( elem == null )
				{
					value = false;
					break;
				}
				value = elem.HasHideInInspectorAttribute;
			}
			return value;
		}
	}

}
