using System;
using UnityEngine;


namespace Ptk.IdStrings
{
	/// <summary>
	/// IdString
	/// </summary>
	/// <remarks>
	/// string 定数を int 型定数のコストで扱う struct。
	/// IdString Attribute で文字列を定義する事で Domain Load 後にはその文字列を int 型の ID として扱う事が可能になる。
	/// Serialize 時は string で外部保存を行い、Deserialize 時は文字列検索で ID に変換される。
	/// またネストした static field として属性を定義する事で階層表現も可能。
	/// 文字列定数や列挙型の代替など幅広い用途で使用可能。
	/// </remarks>
	[Serializable]
	public struct IdString : IEquatable< IdString >, ISerializationCallbackReceiver
	{
		public static readonly IdString None = new();

		[SerializeField] private string mFullName;
		private int mId;

		/// <summary>
		/// Full Name 
		/// </summary>
		/// <remarks>
		/// Serialize 対象の文字列。
		/// </remarks>
		public string FullName => mFullName;

		/// <summary>
		/// Id
		/// </summary>
		/// <remarks>
		/// IdStringManager 内部の登録 Id。
		/// 等価比較などに使用される。
		/// 値は Domain Reload のたびに変更されるためこれを保存して使用しないこと。
		/// </remarks>
		internal int Id => mId;

		/// <summary>
		/// attribute data
		/// </summary>
		internal IdStringAttrData AttrData => IdStringManager.GetAttrData( this );

		/// <summary>
		/// 要素名
		/// </summary>
		/// <remarks>
		/// FullName から ParentPath を除いた文字列。
		/// </remarks>
		public string ElementName
		{
			get{
				var attrData = AttrData;
				if( attrData == null ){ return null; }
				return attrData.ElementName;
			}
		}

		/// <summary>
		/// 階層レベル
		/// </summary>
		public int HierarchyLevel
		{
			get{
				var attrData = AttrData;
				if( attrData == null ){ return 0; }
				return attrData.Hierarchy.Depth;
			}
		}

		/// <summary>
		/// 説明
		/// </summary>
		public string Description
		{
			get{
				var attrData = AttrData;
				if( attrData == null ){ return null; }
				return attrData.Attribute?.Description;
			}
		}

		/// <summary>
		/// 文字列から IdString を取得
		/// </summary>
		/// <param name="stringValue"> FullName 文字列 </param>
		/// <returns> 登録済 IdString。ない場合は IdString.None を返し Warning Log を出力する。 </returns>
		static public IdString Get( string stringValue ) => IdStringManager.GetByName( stringValue );

		/// <summary>
		/// 文字列から IdString を取得
		/// </summary>
		/// <param name="stringValue"> FullName 文字列 </param>
		/// <param name="result"> 登録済 IdString。ない場合は IdString.None。 </param>
		/// <returns> 登録済 IdString が見つかった場合 true。 </returns>
		static public bool TryGetByName( string stringValue, out IdString result ) => IdStringManager.TryGetByName( stringValue, out result );
		
		/// <summary>
		/// Type から IdString を取得
		/// </summary>
		/// <remarks>
		/// IdStringDefineMember Attribute を付与した Type で定義した IdString を取得する。
		/// </remarks>
		/// <typeparam name="T"> IdStringDefineMember を付与したType </typeparam>
		/// <returns> 登録済 IdString。ない場合は IdString.None を返し Warning Log を出力する。 </returns>
		static public IdString Get< T >() => IdStringManager.GetByType< T >();
		
		/// <summary>
		/// Type から IdString を取得
		/// </summary>
		/// <remarks>
		/// IdStringDefineMember Attribute を付与した Type で定義した IdString を取得する。
		/// </remarks>
		/// <param name="type"> IdStringDefineMember を付与したType </param>
		/// <returns> 登録済 IdString。ない場合は IdString.None を返し Warning Log を出力する。 </returns>
		static public IdString Get( Type type ) => IdStringManager.GetByType( type );

		/// <summary>
		/// Type から IdString を取得
		/// </summary>
		/// <remarks>
		/// IdStringDefineMember Attribute を付与した Type で定義した IdString を取得する。
		/// </remarks>
		/// <typeparam name="T"> IdStringDefineMember を付与したType </typeparam>
		/// <param name="result"> 登録済 IdString。ない場合は IdString.None。 </param>
		/// <returns> 登録済 IdString が見つかった場合 true。 </returns>
		static public bool TryGetByType< T >( out IdString result ) => IdStringManager.TryGetByType< T >( out result );

		/// <summary>
		/// Type から IdString を取得
		/// </summary>
		/// <remarks>
		/// IdStringDefineMember Attribute を付与した Type で定義した IdString を取得する。
		/// </remarks>
		/// <param name="type"> IdStringDefineMember を付与したType </param>
		/// <param name="result"> 登録済 IdString。ない場合は IdString.None。 </param>
		/// <returns> 登録済 IdString が見つかった場合 true。 </returns>
		static public bool TryGetByType( Type type, out IdString result ) => IdStringManager.TryGetByType( type, out result );


		internal IdString( string fullName, int id )
		{
			mFullName = fullName;
			mId = id;
		}

		public override readonly string ToString()
		{
			return mFullName;
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
				return mFullName.Equals( str );
			}

			return false;
		}

		static public implicit operator IdString( string str )
		{
			return IdStringManager.GetByName( str );
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
			var idString = IdStringManager.GetByName( mFullName );
			if( idString != IdString.None )
			{
				this = idString;
			}
			else
			{
				// missing..
				mId = 0;
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

	}

}