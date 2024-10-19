using System;

namespace Ptk.IdStrings
{

	/// <summary>
	/// IdStringDefine Attribute
	/// </summary>
	/// <remarks>
	/// string 定数を int 型定数のように扱う IdString を定義するための属性。
	/// assembly に string 定数をすると IdString として登録される。
	/// IdString.Get( string ) で登録した定数を取得できる。
	/// </remarks>
	[AttributeUsage(
		AttributeTargets.Assembly 
		, AllowMultiple = true
	)]
	public class IdStringDefineAttribute : Attribute
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public bool HideInViewer { get; set; }
		public int Order { get; set; }
		public bool NonHierarchical { get; set; }

		public IdStringDefineAttribute( 
			string name, 
			string description = null,
			bool hideInViewer = false,
			int order = 0,
			bool nonHierarchical = false
		){
			Name = name;
			Description = description;
			HideInViewer = hideInViewer;
			Order = order;
			NonHierarchical = nonHierarchical;
		}
	}

	/// <summary>
	/// IdStringDefineMember Attribute
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
	public class IdStringDefineMemberAttribute : IdStringDefineAttribute
	{
		public IdStringDefineMemberAttribute()
			:base( name : null )
		{
		}

//		public EIdStringParentNameType ParentNameType { get; set; } = EIdStringParentNameType.UseParentPath;
		
		public EIdStringNamespaceType NamespaceType { get; set; } = EIdStringNamespaceType.UseParentNamespace;
	}

	/// <summary>
	/// IdString Namespace タイプ
	/// </summary>
	public enum EIdStringNamespaceType
	{
		/// <summary> なし </summary>
		None = 0,
		/// <summary> 親の設定を使用 </summary>
		UseParentNamespace,
		/// <summary> FullName </summary>
		FullName,
	};

	/// <summary>
	/// IdStringView Attribute
	/// </summary>
	/// <remarks>
	/// IdString の設定を行うための PopupView などの表示制御属性
	/// </remarks>
	[AttributeUsage(
		AttributeTargets.Property | AttributeTargets.Field
		, AllowMultiple = false
	)]
	public class IdStringViewAttribute : Attribute
	{
		/// <summary>
		/// Filter
		/// </summary>
		/// <remarks>
		/// StartWith で一致する要素のみを表示対象とする。
		/// FilterType とは併用不可。FilterType 指定よりも優先される。
		/// </remarks>
		public string Filter { get; set; }

		/// <summary>
		/// Filter Type
		/// </summary>
		/// <remarks>
		/// IdStringDefineMember で定義された Type の子孫を表示対象とする
		/// Filter とは併用不可。Filter 指定が優先される。
		/// </remarks>
		public Type FilterType { get; set; }

		/// <summary>
		/// Filter Type の Type 自身を含めるか
		/// </summary>
		/// <remarks>
		/// FilterType 定義時のみ有効。
		/// FilterType での Filter 結果に子孫だけでなく FilterType 自体も対象とする場合に true を設定する。
		/// </remarks>
		public bool IncludeFilterTypeItself { get; set; }

		/// <summary>
		/// HideInViewer を無視するか
		/// </summary>
		public bool IgnoreHideInViewer { get; set; }

		public IdStringViewAttribute( Type filterType = null, bool includeFilterTypeItself = false )
		{
			FilterType = filterType;
			IncludeFilterTypeItself = includeFilterTypeItself;
		}

		public IdStringViewAttribute( string filter )
		{
			Filter = filter;
		}

	}

}
