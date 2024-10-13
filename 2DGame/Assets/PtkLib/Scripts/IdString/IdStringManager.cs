using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Ptk.IdString
{
	/// <summary>
	/// IdString Manager
	/// </summary>
	/// <remarks>
	/// IdString の収集と管理を行うクラス。
	/// RuntimeInitializeOnLoadMethod で自動的に初期化される。
	/// </remarks>
	static public class IdStringManager
	{
		static private bool sIsInitialized;

		static private List< IdStringAttrData > sIdStringAttrList = new();
		static private Dictionary< IdString, IdStringAttrData > sIdAttrDataDic = new();
		static private Dictionary< string, IdStringAttrData > sStringAttrDataDic = new();
		static private Dictionary< Type, IdStringAttrData > sTypeAttrDataDic = new();
		static private List< IdStringAttrData > sAttrDataRoots = new();

		/// <summary>
		/// name で IdString を取得
		/// </summary>
		/// <param name="name"> name ( full path ) </param>
		/// <returns> IdString. 見つからない場合は IdString.None </returns>
		static public IdString GetByName( string name)
		{
			if( !TryGetByName( name, out var idString ) )
			{
				IdStringManager.LogWarning( $"IdString not registered. ({name})" );
			}
			return idString;
		}

		/// <summary>
		/// name で IdString を取得
		/// </summary>
		/// <param name="name"> name ( full path ) </param>
		/// <param name="idString"> IdString. 見つからない場合は IdString.None </param>
		/// <returns> 発見時 true </returns>
		static public bool TryGetByName( string name, out IdString idString )
		{
			Initialize();
			if( !sStringAttrDataDic.TryGetValue( name, out var attrData ) )
			{
				idString = IdString.None;
				return false;
			}
			idString = attrData.IdString;
			return true;
		}

		/// <summary>
		/// type で IdString を取得
		/// </summary>
		/// <typeparam name="T"> IdString Attribute を定義した Type </typeparam>
		/// <returns> IdString. 見つからない場合は IdString.None </returns>
		static public IdString GetByType< T >()
		{
			return GetByType( typeof( T ) );
		}

		/// <summary>
		/// type で IdString を取得
		/// </summary>
		/// <typeparam name="T"> IdString Attribute を定義した Type </typeparam>
		/// <param name="idString"> IdString. 見つからない場合は IdString.None </param>
		/// <returns> 発見時 true </returns>
		static public bool TryGetByType< T >( out IdString idString )
		{
			return TryGetByType( typeof( T ), out idString );
		}

		/// <summary>
		/// type で IdString を取得
		/// </summary>
		/// <param name="type"> IdString Attribute を定義した Type </param>
		/// <returns> IdString. 見つからない場合は IdString.None </returns>
		static public IdString GetByType( Type type )
		{
			if( !TryGetByType( type, out var idString ) )
			{
				IdStringManager.LogWarning( $"IdString not registered. type = ({type.FullName})" );
			}
			return idString;
		}

		/// <summary>
		/// type で IdString を取得
		/// </summary>
		/// <param name="type"> IdString Attribute を定義した Type </param>
		/// <param name="idString"> IdString. 見つからない場合は IdString.None </param>
		/// <returns> 発見時 true </returns>
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

		/// <summary>
		/// 初期化
		/// </summary>
		static public void Initialize()
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

			// IdString class を検索
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
							parentPath += parent.ElementName;
						}

						attrData = new IdStringAttrData()
						{
							Description = typeAttr.Description,
							Name = typeAttr.Name,
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
				parentString += parentTypeAttrData.ElementName;

				var parentTypeIdString = new IdString( parentString, sIdStringAttrList.Count + 1 );
				parentTypeAttrData.IdString = parentTypeIdString;
				sIdStringAttrList.Add( parentTypeAttrData );
				sIdAttrDataDic.Add( parentTypeIdString, parentTypeAttrData );
				sStringAttrDataDic.Add( parentString, parentTypeAttrData );

				parentPath += parentTypeAttrData.ElementName + ".";

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
							Name = attr.Name,
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
			UnityEngine.Debug.Log( $"IdString Initialized. {sw.Elapsed.TotalSeconds} sec." );
		}

		/// <summary>
		/// 全要素を DebugPrint
		/// </summary>
		public static void DebugPrintAllElements()
		{

			UnityEngine.Debug.Log( " --- IdString Debug Print AllElements start ---------------------" );
			foreach( var rootAttr in sAttrDataRoots )
			{
				foreach( var node in rootAttr.Hierarchy )
				{
					var attr = node as IdStringAttrData;
					var idS = attr.IdString;
					UnityEngine.Debug.Log( $"{idS.Id} : {idS.ToString()}" );
				}
			}
			UnityEngine.Debug.Log( " --- IdString Debug Print AllElements end -----------------------" );
		}

	}

}