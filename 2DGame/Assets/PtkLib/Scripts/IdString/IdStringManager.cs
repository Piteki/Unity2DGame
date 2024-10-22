using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Ptk.IdStrings
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
		public const int NameMaxLength = 512;
		public static readonly char[] PathSeparators = new[]{ '.', };
		public static readonly string PathSeparator = ".";

		static private bool sIsInitialized;

		static private List< IdStringAttrData > sIdStringAttrList = new();
		static private Dictionary< string, IdStringAttrData > sStringAttrDataDic = new();
		static private Dictionary< Type, IdStringAttrData > sTypeAttrDataDic = new();
		static private List< IdStringAttrData > sAttrDataRoots = new();

		/// <summary>
		/// name で IdString を取得
		/// </summary>
		/// <param name="name"> name ( full path ) </param>
		/// <returns> IdString. 見つからない場合は IdString.None </returns>
		static public IdString GetByName( string name )
		{
			if( !TryGetByName( name, out var idString ) )
			{
				if( !string.IsNullOrEmpty(name) )
				{
					IdStringManager.LogWarning( $"IdString not registered. ({name})" );
				}
			}
			return idString;
		}

		/// <summary>
		/// name で IdString を取得 or 生成。
		/// </summary>
		/// <remarks>
		/// 存在しない場合は Missing Reference を生成。
		/// </remarks>
		/// <param name="name"> name ( full path ) </param>
		/// <returns> IdString. 見つからない場合は IdString.None </returns>
		static internal IdString GetByNameOrCreateMissingReference( string name )
		{
			if( !TryGetByName( name, out var idString ) )
			{
				if( !string.IsNullOrEmpty(name) )
				{
					idString = new IdString( name, 0 );
				}
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
			if( string.IsNullOrEmpty(name)
			 || !sStringAttrDataDic.TryGetValue( name, out var attrData ) )
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

		/// <summary>
		/// 指定要素の子かを判定
		/// </summary>
		/// <returns> self の親が parent の場合 true </returns>
		static public bool IsChildOf( in IdString self, in IdString parent )
		{
			if( !parent.IsValid ){ return false; }
			var attrData = self.AttrData;
			if( attrData == null ){ return false; }
			//var firstElem = attrData.ParentList.FirstOrDefault();
			var parentList = attrData.ParentList;
			var firstElem = 0 < parentList.Count ? parentList[ 0 ] : IdString.None;
			return attrData.ParentList.FirstOrDefault() == firstElem;
		}

		/// <summary>
		/// 指定要素の子孫かを判定
		/// </summary>
		/// <returns> self の祖先に ancestor が含まれる場合 true </returns>
		static public bool IsDescendantOf( in IdString self, in IdString ancestor )
		{
			if( !ancestor.IsValid ){ return false; }
			var attrData = self.AttrData;
			if( attrData == null ){ return false; }
			return attrData.ParentList.Contains( ancestor );
		}



		static internal IdStringAttrData GetAttrData( in IdString idString )
		{
			int idx = Mathf.Max( 0, idString.Id );
			if( sIdStringAttrList.Count <= idx )
			{ 
				LogError( $"Invalid idString id:{idString.Id} name:{idString.FullName}" );
				return null; 
			}
			return sIdStringAttrList[idx];
		}

		static internal List<IdStringAttrData> GetAllElements()
		{
			Initialize();
			return sIdStringAttrList;
		}

		static internal List<IdStringAttrData> GetRootElements()
		{
			Initialize();
			return sAttrDataRoots;
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

			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();

			sIdStringAttrList.Clear();
			sStringAttrDataDic.Clear();
			sTypeAttrDataDic.Clear();
			sAttrDataRoots.Clear();

			int defineOrderCount = 0;
			
			// add none 
			{
				string elementName = "None";
				var attrData = new IdStringAttrData()
				{
					Attribute = null,

					ElementName = elementName,
					ParentPath = null,
					IsHideInViewer = false,
					DefineOrder = defineOrderCount++,

					IdString = IdString.None,
				};
				sIdStringAttrList.Add( attrData );
			}

			// name と Attribute を一時領域に登録
			IdStringAttrData AddNameAndAttribute( string name, IdStringDefineAttribute attribute = null, int overwriteDefineOrder = 0 )
			{
				if( string.IsNullOrEmpty( name ) ){ LogWarning( "IdStringDefine name is null." ); return null; }

				bool nonHierarchical = false;
				bool hideInViewer = false;
				if( attribute != null )
				{
					nonHierarchical = attribute.NonHierarchical;
					hideInViewer = attribute.HideInViewer;
				}

				var validatedName = !nonHierarchical ? name : GetValidatedPath( name );
				// 登録済みで attribute があるなら終了 ( 先着優先 )
				sStringAttrDataDic.TryGetValue( validatedName, out var attrData );
				if( attrData != null
				 && ( attrData.Attribute != null || attribute == null )
				){ 
					// TODO 設定が異なったら Error 出す
					// LogWarning( "IdStringDefine already exists. name: {validatedName}" );
					return attrData; 
				}

				string elementName = validatedName;
				string parentPath = null;
				if( !nonHierarchical )
				{
					GetParentPath( validatedName, out parentPath, out elementName, true );
				}
				
				if( attrData == null )
				{
					attrData = new IdStringAttrData();
					attrData.DefineOrder =  0 < overwriteDefineOrder ? overwriteDefineOrder : defineOrderCount++;
				}
				attrData.Attribute = attribute;
				attrData.ElementName = elementName;
				attrData.ParentPath = parentPath;
				attrData.IsHideInViewer = hideInViewer; // 後で親階層の値を反映
				attrData.IdString = new IdString( validatedName, -1 ); // 後で Id を付与

				sStringAttrDataDic[ validatedName ] = attrData;
				return attrData;
			}

			
			// memberInfo リスト
			var attrMemberInfoDic = new Dictionary<IdStringAttrData, List<MemberInfo>>();
			// memberInfo リスト に登録
			void AddMemberInfoDic( IdStringAttrData attrData, MemberInfo memberInfo )
			{
				if( attrData == null ){ return; }
				if( memberInfo == null ){ return; }

				// type は typeDic に格納
				if( memberInfo is TypeInfo type )
				{
					if( !sTypeAttrDataDic.TryAdd( type, attrData ) )
					{
						LogWarning( $"AddMemberInfoDic already exists. type : {type.FullName}" );
					}
					return;
				}

				attrMemberInfoDic.TryGetValue( attrData, out var list );
				if( list == null )
				{
					list = new List<MemberInfo>();
					attrMemberInfoDic[ attrData ] = list;
				}
				list.Add( memberInfo );
			}


			// MemberInfo が対象かを判定し含まれる Type を返す
			Type GetTypeFromTargetMember( MemberInfo memberInfo )
			{
				// type, static property, static fieldのみ登録

				if( memberInfo is TypeInfo typeInfo )
				{
					return typeInfo;
				}

				if( memberInfo is PropertyInfo propertyInfo )
				{
					var setter = propertyInfo.GetSetMethod( true );
					if( setter != null 
					 && setter.IsStatic
					){
						return propertyInfo.ReflectedType;
					}

					LogWarning( $"property {propertyInfo.Name} is not has static setter." );
					return null;
				}

				if( memberInfo is FieldInfo fieldInfo )
				{
					if( fieldInfo.IsStatic )
					{
						return fieldInfo.ReflectedType;
					}

					LogWarning( $"field {fieldInfo.Name} is not static." );
					return null;
				}

				return null;
			}

			// 対象メンバーとその子孫の AttrData を登録
			void AddMemberInfoAttribute( 
				IdStringAttrData parent,
				MemberInfo memberInfo,
				string parentPath = null,
				string parentNamespace = null
			){
				if( memberInfo == null ){ return; }
				var attribute = memberInfo.GetCustomAttribute< IdStringDefineMemberAttribute >( true );
				
				TypeInfo selfTypeInfo = memberInfo as TypeInfo;
				Type targetMemberType = null;

				// attribute が存在する場合は targetMember かを判定
				if( attribute != null )
				{
					// type, static property, static fieldのみ登録
					targetMemberType = GetTypeFromTargetMember( memberInfo );

				}

				// attribute が存在する type, static property, static fieldのみ登録
				if( attribute != null
				 && targetMemberType != null 
				){
					var name = attribute.Name;
					if( string.IsNullOrEmpty( name ) )
					{
						name = AppendPath( parentPath, memberInfo.Name );
					}

					string @namespace = attribute.NamespaceType switch
					{
						EIdStringNamespaceType.None => null,
						EIdStringNamespaceType.UseParentNamespace => parentNamespace,
						EIdStringNamespaceType.FullName => targetMemberType.Namespace,
						_ => throw new NotImplementedException()
					};
					string nameWithNamespace = AppendPath( @namespace, name );

					// attribute 登録
					var attrData = AddNameAndAttribute( nameWithNamespace, attribute );
					// MemberInfo 登録
					AddMemberInfoDic( attrData, memberInfo );

					parentPath = name;
					parentNamespace = @namespace;
				}

				// type の場合は子を検索
				if( selfTypeInfo != null )
				{
					// こう bindFlag で絞ったほうが早そうなんだけど Type と Field の順序を保つ方法がなさそう
					// と思ったけど Type と Field の定義順は取れなさそう。しかも bindFlags を使っても速くはならない
					//var memberBindFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
					//memberBindFlags |= BindingFlags.SetField;
					//memberBindFlags |= BindingFlags.SetProperty;
					//memberBindFlags |= BindingFlags.DeclaredOnly;
					//foreach( var child in selfTypeInfo.GetMembers( memberBindFlags ) )
					foreach( var child in selfTypeInfo.DeclaredMembers )
					{
						AddMemberInfoAttribute( parent, child, parentPath, parentNamespace );
					}
				}
			}



			// 全 assenbly から Attribute を検索して登録
			foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var attributes = assembly.GetCustomAttributes< IdStringDefineAttribute >();
				foreach( var attribute in attributes )
				{
					if( attribute == null ) { continue; }
					AddNameAndAttribute( attribute.Name, attribute );
				}
			}

			// 全 assenbly から Attribute を持つ root type を検索
			foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach( var type in assembly.GetTypes() )
				{
					if( sTypeAttrDataDic.ContainsKey( type ) ){ continue; }
					
					// 先頭の type に attribute がある場合は attribute の祖先を検索
					IdStringDefineMemberAttribute findAttr = null;
					Type findType = null;
					var current = type;
					do
					{
						var typeAttr = current.GetCustomAttribute<IdStringDefineMemberAttribute>();
						if( typeAttr != null )
						{
							findAttr = typeAttr;
							findType = current;
						}
						current = current.ReflectedType;
					}
					while( current != null
					    && findAttr != null );

					if( findType == null ){ continue; }

					// 子を順次処理
					AddMemberInfoAttribute( null, findType );
				}
			}

			var attrDataList = sStringAttrDataDic.Values.ToList();
			var checkedList = new HashSet<IdStringAttrData>();
			// 親登録
			foreach( var attrData in attrDataList )
			{
				if( attrData == null ){ continue; }

				AddAttrDataParentReclusively( attrData );
			}

			// AttrData の ParentPath を順次登録
			void AddAttrDataParentReclusively( IdStringAttrData attrData )
			{
				if( attrData == null ){ return; }

				// 処理済判定
				if( checkedList.Contains( attrData ) ){ return; }

				IdStringAttrData parentAttrData = null;
				if( !string.IsNullOrEmpty( attrData.ParentPath ) )
				{
					sStringAttrDataDic.TryGetValue( attrData.ParentPath, out parentAttrData );
					if( parentAttrData == null )
					{
						var attribute = attrData.Attribute;
						// NonHierarchical ノードより上のものは登録しない
						bool nonHierarchical = attribute != null ? attribute.NonHierarchical : false;
						if( !nonHierarchical )
						{
							// 自動登録の親は 元の子の defineOrder を使用。
							parentAttrData = AddNameAndAttribute( attrData.ParentPath, null, attrData.DefineOrder );
						}
					}
				}

				attrData.Hierarchy.SetParent( parentAttrData );

				checkedList.Add( attrData );

				if( parentAttrData == null )
				{
					sAttrDataRoots.Add( attrData );
					return;
				}

				AddAttrDataParentReclusively( parentAttrData );
			}

			// Order順 DefineOrder 順ソート
			Comparison< IdStringAttrData > comparison = ( a, b ) =>
			{
				if( a == null || b == null ){ return 0; }

				var orderA = a.Attribute != null ? a.Attribute.Order : 0;
				var orderB = b.Attribute != null ? b.Attribute.Order : 0;
				if( orderA != orderB ){ return orderA - orderB; }
				return a.DefineOrder - b.DefineOrder;
			};

			sAttrDataRoots.Sort( comparison );
			foreach( var root in sAttrDataRoots )
			{
				if( root == null ){ continue; }
				foreach( var attrData in root.Hierarchy )
				{
					if( attrData == null ){ continue; }
					attrData.Hierarchy.SortChildren( comparison );
				}
			}

			// IdString Id 登録
			foreach( var root in sAttrDataRoots )
			{
				if( root == null ){ continue; }
				foreach( var node in root.Hierarchy )
				{
					AddIdString( node as IdStringAttrData );
				}
			}
			void AddIdString( IdStringAttrData attrData )
			{
				if( attrData == null ){ return; }

				var parent = attrData.Hierarchy.GetParent< IdStringAttrData >();

				var attribute = attrData.Attribute;
				bool nonHierarchical = attribute != null ? attribute.NonHierarchical : false;

				attrData.IsHideInViewer = attribute != null ? attribute.HideInViewer : false;

				List<IdString> parentList;

				if( parent != null 
				 && !nonHierarchical
				){
					attrData.IsHideInViewer |= parent.IsHideInViewer;

					parentList = new List<IdString>( parent.ParentList );
					parentList.Insert( 0, parent.IdString );
				}
				else
				{
					parentList = new List<IdString>( 0 );
				}
				attrData.ParentList = parentList;

				var fullName = attrData.IdString.FullName;
				attrData.IdString = new IdString( fullName, sIdStringAttrList.Count );
				sIdStringAttrList.Add( attrData );
			}

			// MemberInfo 対象にセット
			foreach ( var pair in attrMemberInfoDic )
			{
				var attrData = pair.Key;
				var memberInfoList = pair.Value;
				if( attrData == null ){ continue; }
				if( memberInfoList == null ){ continue; }

				foreach( var memberInfo in memberInfoList )
				{
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
						else
						{
							throw new NotImplementedException();
						}
					}
					catch( Exception exception )
					{
						IdStringManager.LogError( $"IdString SetValue Failed. Name: {memberInfo.Name}, Assembly: {memberInfo.GetType().Assembly.FullName}\nException: {exception.GetType().Name} Message: {exception.Message}" );
					}

				}
			}

			sw.Stop();
			UnityEngine.Debug.Log( $"IdString Initialized. {sw.Elapsed.TotalSeconds} sec." );
		}

		private static void AppendPath( ref string src, in string addString )
		{
			if( string.IsNullOrEmpty( addString ) )
			{ return; }

			if( !string.IsNullOrEmpty( src ) )
			{
				src += PathSeparator;
			}
			src += addString;
		}
		private static string AppendPath( string src, in string addString )
		{
			var ret = src;
			AppendPath( ref ret, addString );
			return ret;
		}

		

		private static string[] SplitPath( in string path )
		{
			
			var ret = path?.Split( PathSeparators, StringSplitOptions.RemoveEmptyEntries /*| StringSplitOptions.TrimEntries*/ );
			if( ret != null )
			{
				foreach( var item in ret )
				{
					item.TrimStart();
					item.TrimEnd();
				}
			}
			return ret;
		}

		/// <summary>
		/// 余分な文字を除去
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		internal static string GetSanitizedString( in string path )
		{
			if( string.IsNullOrEmpty( path ) ) { return path; }
			var str = path;
			if( NameMaxLength < str.Length )
			{
				str = path.Substring( 0, NameMaxLength );
			}
			str = str.Trim();
			str = str.Replace( "\r", string.Empty ).Replace( "\n", string.Empty );
			return str;
		}

		/// <summary>
		/// 検証済パスを返す
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		internal static string GetValidatedPath( in string path )
		{
			var str = GetSanitizedString( path );
			var splitPaths = SplitPath(str);
			return string.Join( PathSeparator, splitPaths );
		}

		/// <summary>
		/// 1 階層上のパスを返す
		/// </summary>
		/// <param name="path"></param>
		/// <param name="parentPath"></param>
		/// <param name="elementName"></param>
		/// <param name="isValidated"></param>
		/// <returns></returns>
		internal static bool GetParentPath( in string path, out string parentPath, out string elementName, bool isValidated = false )
		{
			elementName = isValidated ? path : GetValidatedPath( path );
			parentPath = null;
			int idx = elementName.LastIndexOf( PathSeparator );
			if( idx <= 0 ){ return false; }
			parentPath = elementName.Substring( 0, idx );
			if( string.IsNullOrEmpty( parentPath ) )
			{
				parentPath = null;
			}
			elementName = elementName.Substring( idx + 1 );
			return parentPath != null;
		}

		/// <summary>
		/// 全要素を DebugPrint
		/// </summary>
		public static void DebugPrintAllElements()
		{

			UnityEngine.Debug.Log( " --- IdString Debug Print AllElements start ---------------------" );
			foreach( var rootAttr in sAttrDataRoots )
			{
				if( rootAttr == null ){ continue; }
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

	/// <summary>
	/// IdString Attribute Data 
	/// </summary>
	internal class IdStringAttrData : HierarchyNodeBase
	{
		public const EIdStringNamespaceType DefaultNamespaceType = EIdStringNamespaceType.None;

		/// <summary> Arrtibute </summary>
		public IdStringDefineAttribute Attribute { get; set; }

		/// <summary> 親階層パスを除いた要素名 </summary>
		/// <remarks> Attribute.Name or MemberInfo.Name </remarks>
		public string ElementName { get; set; }

		/// <summary> 要素名を除いた親階層パス </summary>
		public string ParentPath { get; set; }

		/// <summary> Viewer 非表示設定 </summary>
		public bool IsHideInViewer { get; set; }

		public int DefineOrder { get; set; }
		
		/// <summary> IdString </summary>
		public IdString IdString { get; set; }

		public List<IdString> ParentList { get; internal set; }

		public override string ToString()
		{
			return IdString.FullName != null 
				? IdString.FullName
				: IdString == IdString.None ? IdString.ElementName : "(Error : FullName is null )";
		}

	}
}