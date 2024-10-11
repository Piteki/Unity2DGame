using System;
using System.Collections.Generic;
using System.Reflection;
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
			return IdStringDic.GetByString( str );
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
			this = IdStringDic.GetByString( mString );
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			// check only
			IdStringDic.GetByString( mString );
		}
	}

	static public class IdStringDic
	{
		static private bool sIsInitialized;
		static private List< IdString > sIdStringList = new();
		static private Dictionary< string, IdString > sIdStringDic = new();
		static private Dictionary< Type, IdString > sTypeStringDic = new();

		static public IdString GetByString( string str )
		{
			if( !TryGetByString( str, out var idString ) )
			{
				IdStringDic.LogWarning( $"IdString not registered. ({str})" );
			}
			return IdString.None;
		}

		static public bool TryGetByString( string str, out IdString idString )
		{
			Initialize();
			if( !sIdStringDic.TryGetValue( str, out idString ) )
			{
				idString = IdString.None;
				return false;
			}
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
		static private void Initialize()
		{
		   // if( sIsInitialized ) { return; } 
			sIsInitialized = true;

			sIdStringList.Clear();
			sIdStringDic.Clear();
			sTypeStringDic.Clear();

			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();

			var memberBindFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			memberBindFlags |= BindingFlags.SetField;
			memberBindFlags |= BindingFlags.SetProperty;
			memberBindFlags |= BindingFlags.DeclaredOnly;

			// DefinitionType を検索
			var typedic = new Dictionary< Type, Tuple< string, Assembly, IdStringDefinitionTypeAttribute > >();
			foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach( var type in assembly.GetTypes() )
				{
					if( typedic.ContainsKey( type ) )
					{
						IdStringDic.LogWarning( $"IdString IdStringDefinitionTypeAttribute type already exist. type: {type.FullName}, Assembly: {assembly.FullName}" );
						continue;
					}

					var typeAttr = type.GetCustomAttribute<IdStringDefinitionTypeAttribute>();
					if( typeAttr == null ){ continue; }

					if( type.IsGenericType )
					{
						IdStringDic.LogError( $"IdString IdStringDefinitionTypeAttribute type is generic type. type: {type.FullName}, Assembly: {assembly.FullName}" );
						continue;
					}

					var hierarchyName = type.Name;
					var parentType = type.ReflectedType;
					while( parentType != null )
					{
						hierarchyName = $"{parentType.Name}.{hierarchyName}";
						parentType = parentType.ReflectedType;
					}
					typedic.Add( type, Tuple.Create( hierarchyName, assembly, typeAttr ) );

				}
			}

			UnityEngine.Debug.Log( $" Initialize sprit {sw.Elapsed.TotalSeconds} sec." );

			foreach( var pair in typedic )
			{
				var type = pair.Key;
				var hierarchyName = pair.Value.Item1;
				var assembly = pair.Value.Item2;
				var classAttr = pair.Value.Item3;

				// TODO HierarchyName をオプションで制御
				// TODO namespace 付与オプションを追加
				// TODO name オプション指定機能追加
				// TODO 階層構造を解析可能な辞書を追加

				// DefinitionType 自体を登録
				{ 
					var id = sIdStringList.Count + 1;
					var idString = new IdString( hierarchyName, id );
					sIdStringList.Add( idString );
					sIdStringDic.Add( hierarchyName, idString );
					sTypeStringDic.Add( type, idString );
				} 

				foreach( var memberInfo in type.GetMembers( memberBindFlags ) )
				{
					var attr = memberInfo.GetCustomAttribute<IdStringAttribute>();
					if( attr == null ){ continue; }

					var name = $"{hierarchyName}.{memberInfo.Name}";
					if( sIdStringDic.ContainsKey( name ) ){ continue; }

					var id = sIdStringList.Count + 1;
					var idString = new IdString( name, id );

					try
					{
						if( memberInfo is PropertyInfo propertyInfo )
						{
							propertyInfo.SetValue( null, idString );
						}
						else if ( memberInfo is FieldInfo fieldInfo )
						{
							fieldInfo.SetValue( null, idString );
						}
						sIdStringList.Add( idString );
						sIdStringDic.Add( name, idString );
						UnityEngine.Debug.Log( $"{idString.Id} : {idString}" );
					}
					catch( Exception exception )
					{
						IdStringDic.LogError( $"IdString SetValue Failed. name: {name}, Assembly: {assembly.FullName}\nException: {exception.GetType().Name} Message: {exception.Message}" );
					}

				}
			}

			sw.Stop();
			UnityEngine.Debug.Log( $" Initialize {sw.Elapsed.TotalSeconds} sec." );

			foreach( var idS in sIdStringList )
			{
				UnityEngine.Debug.Log( $"{idS.Id} : {idS.ToString()}" );
			}
			UnityEngine.Debug.Log( "---------------------" );

			
			UnityEngine.Debug.Log( $"Sample.AAAA    = {Sample.AAAA.Id} : {Sample.AAAA.ToString()}" );
		   // UnityEngine.Debug.Log( $"Sample.BbB     = {Sample.BbB.Id} : {Sample.BbB.ToString()}" );
			UnityEngine.Debug.Log( $"Sample.TestStr = {Sample.TestStr.Id} : {Sample.TestStr.ToString()}" );

			//UnityEngine.Debug.Log( $"SampleBBB.AAAA    = {SampleBBB.AAAA.Id} : {SampleBBB.AAAA.ToString()}" );
			UnityEngine.Debug.Log( $"SampleBBB.BbB     = {SampleBBB.BbB.Id} : {SampleBBB.BbB.ToString()}" );
			UnityEngine.Debug.Log( $"SampleBBB.TestStr = {SampleBBB.TestStr.Id} : {SampleBBB.TestStr.ToString()}" );
			UnityEngine.Debug.Log( $"SampleBBB.FFFRDO = {SampleBBB.FFFRDO.Id} : {SampleBBB.FFFRDO.ToString()}" );

			UnityEngine.Debug.Log( $"SampleStructDerived.StruGGGDDDD = {SampleStructDerived.StruGGGDDDD.Id} : {SampleStructDerived.StruGGGDDDD.ToString()}" );
			UnityEngine.Debug.Log( $"ISampleItf.Itfffff = {ISampleItf.Itfffff.Id} : {ISampleItf.Itfffff.ToString()}" );

		}

	}

	public enum EIdStringParentNameType
	{
		None = 0,
		ShortName = 1,
		FullName = 2,
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,  AllowMultiple = false)]
	public class IdStringDefinitionTypeAttribute : Attribute
	{
		public EIdStringParentNameType ParentName { get; set; } = EIdStringParentNameType.FullName;
		
	}

   [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class IdStringAttribute : Attribute
	{

	}

	[IdStringDefinitionType()]
	static public class Sample
	{
		[IdString]
		static public IdString TestStr { get; private set; }

		[IdString]
		static public IdString AAAA { get; private set; }

		[IdString]
		static private IdString BbB { get; set; }
	}

	[IdStringDefinitionType]
	public class SampleDerivrd : SampleBBB
	{
		[IdString]
		static public IdString DerivedDDDD { get; private set; }
	}


	[IdStringDefinitionType]
	public struct SampleStruct
	{
		[IdString]
		static public IdString StructTTTT { get; private set; }
	}
	[IdStringDefinitionType]
	public interface ISampleItf
	{
		[IdString]
		static public IdString Itfffff { get; private set; }
	}
	[IdStringDefinitionType]
	public struct SampleStructDerived : ISampleItf
	{
		[IdString]
		static public IdString StruGGGDDDD { get; private set; }
	}

	// ※ Generic 型には使用できない -> exception: InvalidOperationException, error: Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true.
	[IdStringDefinitionType]
	public class GenSample< T >
	{
		[IdString]
		static public IdString GGGGG { get; private set; }
	}

	// ※ Generic 型には使用できない -> exception: InvalidOperationException, error: Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true.
	[IdStringDefinitionType]
	public class GenSampleWhere< TClass >
		where TClass : class
	{
		[IdString]
		static public IdString TCTC { get; private set; }
	}

	// ※ Generic 型には使用できない -> exception: InvalidOperationException, error: Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true.
	[IdStringDefinitionType]
	public class GenSampleWhereSt< TStruct >
		where TStruct : struct
	{
		[IdString]
		static public IdString TSTS { get; private set; }
	}



	[IdStringDefinitionType]
	public class SampleBBB
	{
		// ※ setter がない property には使用できない -> exception: ArgumentException, error: Set Method not found for 'TestStr'
		[IdString]
		public static IdString TestStr { get; }

		[IdString]
		private static IdString AAAA { get; set; }

		[IdString]
		public static IdString BbB { get; private set; }

		[IdString]
		public static readonly IdString FFFRDO;
	}

	
	[IdStringDefinitionType]
	public static class SampleCCCC
	{
		public static class Child
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
#endif