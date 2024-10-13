using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Hierarchy;
using UnityEngine;

#if true
namespace Ptk
{
	[Serializable]
	public struct IdString : IEquatable< IdString >, ISerializationCallbackReceiver
	{
		public static readonly IdString None = new();

		public string String => mString;
		internal int Id => mId;

		[SerializeField] private string mString;
		private int mId;

		internal IdString( string str, int id )
		{
			mString = str;
			mId = id;
		}

		public override readonly string ToString()
		{
			return mString;
		}

		public override readonly int GetHashCode()
		{
			return mId;
		}

		public readonly bool Equals( IdString other )
		{
			return mId == other.mId;
		}

		public override readonly bool Equals( object obj )
		{
			if( obj is IdString hashTag )
			{
				return mId == hashTag.mId;
			}

			if( obj is string str )
			{
				return mString.Equals( str );
			}

			return false;
		}

		static public implicit operator IdString( string str )
		{
			return IdStringManager.GetByString( str );
		}

		static public bool operator ==( in IdString self, in IdString other )
		{
			return self.Equals( other );
		}
		static public bool operator !=( in IdString self, in IdString other )
		{
			return !self.Equals( other );
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			this = IdStringManager.GetByString( mString );
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			// check only
			IdStringManager.GetByString( mString );
		}

		static public IdString Get( string stringValue ) => IdStringManager.GetByString( stringValue );
		static public IdString Get< T >() => IdStringManager.GetByType< T >();
	}

	static public class IdStringManager
	{
		static private bool sIsInitialized;
		//static private List< IdString > sIdStringList = new();
		//static private Dictionary< string, IdString > sIdStringDic = new();
		//static private Dictionary< Type, IdString > sTypeStringDic = new();

		static private List< IdStringAttrData > sIdStringAttrList = new();
		static private Dictionary< IdString, IdStringAttrData > sIdAttrDataDic = new();
		static private Dictionary< string, IdStringAttrData > sStringAttrDataDic = new();
		static private Dictionary< Type, IdStringAttrData > sTypeAttrDataDic = new();
		static private List< IdStringAttrData > sAttrDataRoots = new();

		static public IdString GetByString( string str )
		{
			if( !TryGetByString( str, out var idString ) )
			{
				IdStringManager.LogWarning( $"IdString not registered. ({str})" );
			}
			return idString;
		}

		static public bool TryGetByString( string str, out IdString idString )
		{
			Initialize();
			if( !sStringAttrDataDic.TryGetValue( str, out var attrData ) )
			{
				idString = IdString.None;
				return false;
			}
			idString = attrData.IdString;
			return true;
		}

		static public IdString GetByType< T >()
		{
			return GetByType( typeof( T ) );
		}

		static public bool TryGetByType< T >( out IdString idString )
		{
			return TryGetByType( typeof( T ), out idString );
		}
		static public IdString GetByType( Type type )
		{
			if( !TryGetByType( type, out var idString ) )
			{
				IdStringManager.LogWarning( $"IdString not registered. type = ({type.FullName})" );
			}
			return idString;
		}

		static public bool TryGetByType( Type type, out IdString idString )
		{
			Initialize();
			if( !sTypeAttrDataDic.TryGetValue( type, out var attrData ) )
			{
				idString = IdString.None;
				return false;
			}
			idString = attrData.IdString;
			return true;
		}

		static private void LogWarning( string msg )
		{
			UnityEngine.Debug.LogWarning( msg );
		}
		static private void LogError( string msg )
		{
			UnityEngine.Debug.LogError( msg );
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		static private void InitializeOnLoad()
		{
			sIsInitialized = false;
			Initialize();
		}

		static private void Initialize()
		{
		    if( sIsInitialized ) { return; } 
			sIsInitialized = true;

			sIdStringAttrList.Clear();
			sIdAttrDataDic.Clear();
			sStringAttrDataDic.Clear();
			sTypeAttrDataDic.Clear();
			sAttrDataRoots.Clear();

			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();

			var memberBindFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			memberBindFlags |= BindingFlags.SetField;
			memberBindFlags |= BindingFlags.SetProperty;
			memberBindFlags |= BindingFlags.DeclaredOnly;

			// DefinitionType を検索
			//var typedic = new Dictionary< Type, Tuple< string, Assembly, IdStringDefinitionTypeAttribute > >();
			foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach( var type in assembly.GetTypes() )
				{
					AddAttrData( type, true );
					IdStringAttrData AddAttrData( Type type, bool isFirst )
					{
						if( type == null ){ return null; }
					
						if( sTypeAttrDataDic.TryGetValue( type, out var attrData ) )
						{ return attrData; }

						var typeAttr = type.GetCustomAttribute<IdStringAttribute>();
						// 初回の場合は 属性ない時点で終了
						if( typeAttr == null
						 && isFirst
						){
							return null;
						}

						// parent を add
						var parent = AddAttrData( type.ReflectedType, false );
						// 属性なしは return
						if( typeAttr == null ){ return parent; }

						if( type.IsGenericType )
						{
							IdStringManager.LogError( $"IdStringAttribute type is generic type. {type.FullName}, Assembly: {assembly.FullName}" );
							return parent; 
						}


						string parentPath = null;
						if( parent != null )
						{
							if( !string.IsNullOrEmpty( parent.ParentPath ) )
							{
								parentPath = $"{parent.ParentPath}.";
							}
							parentPath += parent.ElementString;
						}

						attrData = new IdStringAttrData()
						{
							Description = typeAttr.Description,
							String = typeAttr.String,
							ParentNameType = typeAttr.ParentNameType,
							NamespaceType = typeAttr.NamespaceType,

							MemberName = type.Name,
							ParentPath = parentPath,

						};

						if( parent != null )
						{
							attrData.Hierarchy.SetParent( parent );
						}
						else
						{
							sAttrDataRoots.Add( attrData );
						}

						sTypeAttrDataDic.Add( type, attrData );
						return attrData;
					}
				}
			}

			UnityEngine.Debug.Log( $" Initialize sprit {sw.Elapsed.TotalSeconds} sec." );

			var idStringType = typeof( IdString );

			foreach( var pair in sTypeAttrDataDic )
			{
				var parentType = pair.Key;
				var parentTypeAttrData = pair.Value;

				

				// DefinitionType 自体を登録
				string parentString = string.Empty;
				string parentNamespace = string.Empty;
				var parentNamespaceType = parentTypeAttrData.GetNamespaceType();
				if( parentNamespaceType != EIdStringNamespaceType.None 
				 && !string.IsNullOrEmpty( parentType.Namespace )
				){
					parentNamespace = $"{parentType.Namespace}.";
					parentString = parentNamespace;
				}
				string parentPath = string.Empty;
				var parentParentNameType = parentTypeAttrData.GetParentNameType();
				if(	parentParentNameType != EIdStringParentNameType.None
				 && !string.IsNullOrEmpty( parentTypeAttrData.ParentPath ) 
				){
					parentPath = $"{parentTypeAttrData.ParentPath}.";
					parentString += parentPath;
				}
				parentString += parentTypeAttrData.ElementString;

				var parentTypeIdString = new IdString( parentString, sIdStringAttrList.Count + 1 );
				parentTypeAttrData.IdString = parentTypeIdString;
				sIdStringAttrList.Add( parentTypeAttrData );
				sIdAttrDataDic.Add( parentTypeIdString, parentTypeAttrData );
				sStringAttrDataDic.Add( parentString, parentTypeAttrData );

				parentPath += parentTypeAttrData.ElementString + ".";

				foreach( var memberInfo in parentType.GetMembers( memberBindFlags ) )
				{

					Type memberType = null;
					{
						if( memberInfo is PropertyInfo propertyInfo )
						{
							memberType = propertyInfo.PropertyType;
						}
						else if( memberInfo is FieldInfo fieldInfo )
						{
							memberType = fieldInfo.FieldType;
						}
					}
					if( memberType != idStringType ){ continue; }

					var attr = memberInfo.GetCustomAttribute<IdStringAttribute>();
					if( attr == null ){ continue; }

					var pfx = string.Empty;
					var namespaceType = attr.NamespaceType != EIdStringNamespaceType.UseParentSetting
									  ? attr.NamespaceType
									  : parentNamespaceType;
					if( namespaceType != EIdStringNamespaceType.None 
					 && !string.IsNullOrEmpty( parentNamespace )
					){
						pfx = parentNamespace;
					}
					var parentNameType = attr.ParentNameType != EIdStringParentNameType.UseParentSetting
										? attr.ParentNameType
										: parentParentNameType;
					if(	parentNameType != EIdStringParentNameType.None
					 && !string.IsNullOrEmpty( parentPath ) 
					){
						pfx += parentPath;
					}

					var name = pfx + memberInfo.Name;
					bool exists = sStringAttrDataDic.TryGetValue( name, out var attrData );
					if( !exists )
					{
						var idString = new IdString( name, sIdStringAttrList.Count + 1 );
						attrData = new IdStringAttrData()
						{
							Description = attr.Description,
							String = attr.String,
							ParentNameType = attr.ParentNameType,
							NamespaceType = attr.NamespaceType,

							MemberName = memberInfo.Name,
							ParentPath = parentString,

							IdString = idString,

						};

						attrData.Hierarchy.SetParent(parentTypeAttrData);

						sIdStringAttrList.Add( attrData );
						sIdAttrDataDic.Add( attrData.IdString, attrData );
						sStringAttrDataDic.Add( name, attrData );
					}

					try
					{
						if( memberInfo is PropertyInfo propertyInfo )
						{
							propertyInfo.SetValue( null, attrData.IdString );
						}
						else if ( memberInfo is FieldInfo fieldInfo )
						{
							fieldInfo.SetValue( null, attrData.IdString );
						}
					}
					catch( Exception exception )
					{
						IdStringManager.LogError( $"IdString SetValue Failed. name: {name}, Assembly: {memberInfo.GetType().Assembly.FullName}\nException: {exception.GetType().Name} Message: {exception.Message}" );
					}

				}
			}


			sw.Stop();
			UnityEngine.Debug.Log( $" Initialize {sw.Elapsed.TotalSeconds} sec." );

			UnityEngine.Debug.Log( " --- List ---------------------" );
			foreach( var attr in sIdStringAttrList )
			{
				var idS = attr.IdString;
				UnityEngine.Debug.Log( $"{idS.Id} : {idS.ToString()}" );
			}
			UnityEngine.Debug.Log( "---------------------" );
			UnityEngine.Debug.Log( " --- Hierarchy ---------------------" );
			foreach( var rootAttr in sAttrDataRoots )
			{
				foreach( var node in rootAttr.Hierarchy )
				{
					var attr = node as IdStringAttrData;
					var idS = attr.IdString;
					UnityEngine.Debug.Log( $"{idS.Id} : {idS.ToString()}" );
				}
			}
			UnityEngine.Debug.Log( "---------------------" );

			
			//UnityEngine.Debug.Log( $"Sample.AAAA    = {Sample.AAAA.Id} : {Sample.AAAA.ToString()}" );
		 //  // UnityEngine.Debug.Log( $"Sample.BbB     = {Sample.BbB.Id} : {Sample.BbB.ToString()}" );
			//UnityEngine.Debug.Log( $"Sample.TestStr = {Sample.TestStr.Id} : {Sample.TestStr.ToString()}" );

			//UnityEngine.Debug.Log( $"SampleBBB.AAAA    = {SampleBBB.AAAA.Id} : {SampleBBB.AAAA.ToString()}" );
			//UnityEngine.Debug.Log( $"SampleBBB.BbB     = {SampleBBB.BbB.Id} : {SampleBBB.BbB.ToString()}" );
			//UnityEngine.Debug.Log( $"SampleBBB.TestStr = {SampleBBB.TestStr.Id} : {SampleBBB.TestStr.ToString()}" );
			//UnityEngine.Debug.Log( $"SampleBBB.FFFRDO = {SampleBBB.FFFRDO.Id} : {SampleBBB.FFFRDO.ToString()}" );

			//UnityEngine.Debug.Log( $"SampleStructDerived.StruGGGDDDD = {SampleStructDerived.StruGGGDDDD.Id} : {SampleStructDerived.StruGGGDDDD.ToString()}" );
			//UnityEngine.Debug.Log( $"ISampleItf.Itfffff = {ISampleItf.Itfffff.Id} : {ISampleItf.Itfffff.ToString()}" );



			var Itfffff = IdString.Get( "Ptk.TTestNs.ISampleItf.Itfffff" );
			UnityEngine.Debug.Log( $"Itfffff = {Itfffff.ToString()}" );
			var sampleDerivrd = IdString.Get<TTestNs.SampleDerivrd>();
			UnityEngine.Debug.Log( $"sampleDerivrd = {sampleDerivrd.ToString()}" );
		}

	}


	// Ptk
	// [ ShortName ]
	// Ptk.ParentA
	// [ FullNameWithoutNamespace ]
	// Ptk.ParentA.B
	// [ FullName ]
	// Ptk.ParentA.B.C
	// 
	// Ptk.ParentA.B.C.D
	// [ None ]
	// Ptk.ParentA.B.C.D.E


	internal class IdStringAttrData : HierarchyNodeBase
	{
		public static EIdStringParentNameType DefaultParentNameType = EIdStringParentNameType.FullName;
		public static EIdStringNamespaceType DefaultNamespaceType = EIdStringNamespaceType.FullName;

		public string Description { get; set; }
		public string String { get; set; }
		public EIdStringParentNameType ParentNameType { get; set; } = EIdStringParentNameType.UseParentSetting;
		public EIdStringNamespaceType NamespaceType { get; set; } = EIdStringNamespaceType.UseParentSetting;

		public string MemberName { get; set; }
		public string ParentPath { get; set; }

		public string ElementString => !string.IsNullOrEmpty( String ) ? String : MemberName;
		

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

	}


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
		public string String { get; set; }
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









namespace TTestNs{




	[IdString()]
	static public class Sample
	{
		[IdString]
		static public IdString TestStr { get; private set; }

		[IdString]
		static public IdString AAAA { get; private set; }

		[IdString]
		static private IdString BbB { get; set; }
	}


	[IdString( String ="OvErRide!")]
	public class SampleDerivrd : TTestNs.TTestNsChild.SampleBBB
	{
		[IdString]
		static public IdString DerivedDDDD { get; private set; }
	}


	[IdString( ParentNameType =EIdStringParentNameType.None )]
	public struct SampleStruct
	{
		[IdString]
		static public IdString StructTTTT { get; private set; }
	}
	[IdString]
	public interface ISampleItf
	{
		[IdString]
		static public IdString Itfffff { get; private set; }
	}
	[IdString]
	public struct SampleStructDerived : ISampleItf
	{
		[IdString( ParentNameType =EIdStringParentNameType.None )]
		static public IdString StruGGGDDDD { get; private set; }
	}

	// ※ Generic 型には使用できない -> exception: InvalidOperationException, error: Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true.
	[IdString]
	public class GenSample< T >
	{
		[IdString]
		static public IdString GGGGG { get; private set; }
	}

	// ※ Generic 型には使用できない -> exception: InvalidOperationException, error: Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true.
	[IdString( NamespaceType =EIdStringNamespaceType.None )]
	public class GenSampleWhere< TClass >
		where TClass : class
	{
		[IdString]
		static public IdString TCTC { get; private set; }
	}

	// ※ Generic 型には使用できない -> exception: InvalidOperationException, error: Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true.
	[IdString]
	public class GenSampleWhereSt< TStruct >
		where TStruct : struct
	{
		[IdString]
		static public IdString TSTS { get; private set; }
	}

namespace TTestNsChild{


	//[IdStringDefinitionType( UseNamespace = true )] 
	public class SampleBBB 
	{
		static public bool BoolField = false;

		// ※ setter がない property には使用できない -> exception: ArgumentException, error: Set Method not found for 'TestStr'
		[IdString]
		public static IdString TestStr { get; }

		[IdString( NamespaceType =EIdStringNamespaceType.None )]
		private static IdString AAAA { get; set; }

		[IdString]
		public static IdString BbB { get; private set; }

		[IdString]
		public static readonly IdString FFFRDO;
	}

	
	[IdString( NamespaceType =EIdStringNamespaceType.None )]
	public static class SampleCCCC
	{
		[IdString]
		public static class Child
		{

		[IdString]
		public static class GrandChild
		{
			[IdString]
			public static IdString TestStr { get; private set; }

			[IdString]
			public static IdString AAAA { get; private set; }

			[IdString]
			public static IdString BbB { get; private set; }

			[IdString]
			public static readonly IdString FFFRDO;
		}
		}

	}
} }

}

	
	[Ptk.IdString]
	public static class NoNameSpaceSample
	{
		[Ptk.IdString]
		public static class Child
		{

		[Ptk.IdString]
		public static class GrandChild
		{
			[Ptk.IdString]
			public static Ptk.IdString TestStr { get; private set; }

			[Ptk.IdString]
			public static Ptk.IdString AAAA { get; private set; }

			[Ptk.IdString]
			public static Ptk.IdString BbB { get; private set; }

			[Ptk.IdString]
			public static readonly Ptk.IdString FFFRDO;
		}
		}

	}

#endif