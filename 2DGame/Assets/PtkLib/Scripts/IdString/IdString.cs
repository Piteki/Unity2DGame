using System;
using UnityEngine;


namespace Ptk.IdString
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

		public string Name => mName;
		internal int Id => mId;

		[SerializeField] private string mName;
		private int mId;

		internal IdString( string name, int id )
		{
			mName = name;
			mId = id;
		}

		public override readonly string ToString()
		{
			return mName;
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
				return mName.Equals( str );
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
			this = IdStringManager.GetByName( mName );
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			// check only
			IdStringManager.GetByName( mName );
		}

		static public IdString Get( string stringValue ) => IdStringManager.GetByName( stringValue );
		static public IdString Get< T >() => IdStringManager.GetByType< T >();
	}

}