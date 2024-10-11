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
		static private Dictionary< string, IdString > sIdStringDic = new();
		static private List< IdString > sIdStringList = new();

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

			sIdStringDic.Clear();
			sIdStringList.Clear();

			var list = new List< string >();  
			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			//foreach( var attributeO in Attribute.GetCustomAttributes( typeof(IdStringDefinitionClassAttribute) ) {
			//{
			//        attributeO.
			//}
			

			var propertyBindFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			var FieldBindFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
#if false
			foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach( var type in assembly.GetTypes() )
				{
					// TODO ここでぐるぐる回って 親属性をたどる
					// TODO でも非効率なのでいったん class Type だけ出して処理したほうがよさそう
					var hierarchyName = type.Name;
					var parentType = type.ReflectedType;
					while( parentType != null )
					{
						hierarchyName = $"{parentType.Name}.{hierarchyName}";
						parentType = parentType.ReflectedType;
					}
					//UnityEngine.Debug.Log( $"hierarchyName = {hierarchyName}" );
#else
			// ↑を事前に済ますタイプ
			var typedic = new Dictionary< Type, Tuple< string, Assembly, IdStringDefinitionClassAttribute > >();
			foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach( var type in assembly.GetTypes() )
				{
					if( typedic.ContainsKey( type ) )
					{
						IdStringDic.LogWarning( $"IdString included type already exist. type: {type.FullName}, Assembly: {assembly.FullName}" );
						continue;
					}

					bool hasAttr = false;
					{
						foreach( var propertyInfo in type.GetProperties( propertyBindFlags ) )
						{
							var attr = propertyInfo.GetCustomAttribute<IdStringAttribute>();
							if( attr == null ){ continue; }
							hasAttr = true; 
							break;
						}
					}

					if( !hasAttr )
					{
						foreach( var firldInfo in type.GetFields( FieldBindFlags ) )
						{
							var attr = firldInfo.GetCustomAttribute<IdStringAttribute>();
							if( attr == null ){ continue; }
							hasAttr = true; 
							break;
						}
					}

					if( !hasAttr ){ continue; }

					var classAttr = type.GetCustomAttribute<IdStringDefinitionClassAttribute>();

					var hierarchyName = type.Name;
					var parentType = type.ReflectedType;
					while( parentType != null )
					{
						hierarchyName = $"{parentType.Name}.{hierarchyName}";
						parentType = parentType.ReflectedType;
					}
					UnityEngine.Debug.Log( $"hierarchyName = {hierarchyName}" );
					typedic.Add( type, Tuple.Create( hierarchyName, assembly, classAttr ) );

				}
			}

			UnityEngine.Debug.Log( $" Initialize sprit {sw.Elapsed.TotalSeconds} sec." );

			{
				foreach( var pair in typedic )
				{
					var type = pair.Key;
					var hierarchyName = pair.Value.Item1;
					var assembly = pair.Value.Item2;
					var classAttr = pair.Value.Item3;
#endif
					foreach( var propertyInfo in type.GetProperties( propertyBindFlags ) )
					{
						var attr = propertyInfo.GetCustomAttribute<IdStringAttribute>();
						if( attr == null ){ continue; }

						var refName = string.Empty;
						//switch( attr. )
						//{
						//    case 
						//}
						//refName = propertyInfo.ReflectedType.FullName; // namespace included
						//var refName = propertyInfo.ReflectedType.Name; // namespace exclude
						refName = hierarchyName;
						var name = $"{refName}.{propertyInfo.Name}";
						if( sIdStringDic.ContainsKey( name ) ){ continue; }
						var id = sIdStringList.Count + 1;
						var idString = new IdString( name, id );
						sIdStringList.Add( idString );
						sIdStringDic.Add( name, idString );

						//var setter = propertyInfo.GetSetMethod(true);
						//if( setter != null )
						//{
						//    setter.Invoke( null, new object[]{idString } );
						//}
						try
						{
							propertyInfo.SetValue( null, idString );
						}
						catch( Exception exception )
						{
							IdStringDic.LogError( $"IdString SetValue Failed. property name: {name}, Assembly: {assembly.FullName}\nException: {exception.GetType().Name} Message: {exception.Message}" );
						}
					}

				}
				//foreach (var attribute in assembly.GetCustomAttributes<IdStringAttribute>())
				//{
				//    try
				//    {
				//            attribute.
				//    }
				//    catch (Exception exception)
				//    {
				//        IdStringDic.LogError($"Failed to register tag {attribute.TagName} from assembly {assembly.FullName} with error: {exception.Message}");
				//    }
				//}
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
		}

	}

	public enum EIdStringParentNameType
	{
		None = 0,
		ShortName = 1,
		FullName = 2,
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class IdStringDefinitionClassAttribute : Attribute
	{
		public EIdStringParentNameType ParentName { get; set; } = EIdStringParentNameType.FullName;
		
	}

   [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class IdStringAttribute : Attribute
	{

	}

	[IdStringDefinitionClass()]
	static public class Sample
	{
		[IdString]
		static public IdString TestStr { get; private set; }

		[IdString]
		static public IdString AAAA { get; private set; }

		[IdString]
		static private IdString BbB { get; set; }
	}

	public class SampleDerivrd : SampleBBB
	{
		[IdString]
		static public IdString DerivedDDDD { get; private set; }
	}

	// ※ Generic 型には使用できない -> exception: InvalidOperationException, error: Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true.
	public class GenSample< T >
	{
		[IdString]
		static public IdString GGGGG { get; private set; }
	}

	// ※ Generic 型には使用できない -> exception: InvalidOperationException, error: Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true.
	public class GenSampleWhere< TClass >
		where TClass : class
	{
		[IdString]
		static public IdString TCTC { get; private set; }
	}

	// ※ Generic 型には使用できない -> exception: InvalidOperationException, error: Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true.
	public class GenSampleWhereSt< TStruct >
		where TStruct : struct
	{
		[IdString]
		static public IdString TSTS { get; private set; }
	}



	[IdStringDefinitionClass]
	public class SampleBBB
	{
		// ※ setter がない property には使用できない -> exception: ArgumentException, error: Set Method not found for 'TestStr'
		[IdString]
		public static IdString TestStr { get; }

		[IdString]
		private static IdString AAAA { get; set; }

		[IdString]
		public static IdString BbB { get; private set; }

		public static readonly IdString FFFRDO;
	}

	
	[IdStringDefinitionClass]
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

			public static readonly IdString FFFRDO;
		}

	}


}
#endif