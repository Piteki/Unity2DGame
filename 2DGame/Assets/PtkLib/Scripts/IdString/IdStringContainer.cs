using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Ptk.IdStrings
{
	/// <summary>
	/// IdString Container
	/// </summary>
	/// <remarks>
	/// 複数の IdString を格納するクラス
	/// </remarks>
	[Serializable]
	public class IdStringContainer : IEnumerable< IdString >
	{
		public const int ContainerDefaultCapacity = 64;

		[SerializeField]
		private List< IdString > mList = new( ContainerDefaultCapacity );

		private Dictionary<IdString, int> mCounter;

		//private List< IdString > mTmp = new( ContainerDefaultCapacity );

		public bool IsDirty{ get; private set; }

		public int Count => mList.Count;


		/// <summary>
		/// 一意な値を末尾に登録
		/// </summary>
		/// <param name="idString"></param>
		/// <returns> 登録時 true。既に登録済の場合 false </returns>
		public bool AddLastUnique( in IdString idString )
		{
			if( mList.Contains( idString ) ) { return false; }
			AddLast( idString );
			return true;
		}

		/// <summary>
		/// 値を末尾に登録
		/// </summary>
		/// <remarks>
		/// 既に登録済の場合も登録される。
		/// </remarks>
		/// <param name="idString"></param>
		public void AddLast( in IdString idString )
		{
			mList.Add( idString );
			IsDirty = true;
			OnTagChanged( idString );
		}

		/// <summary>
		/// 一意な値を先頭に登録
		/// </summary>
		/// <param name="idString"></param>
		/// <returns> 登録時 true。既に登録済の場合 false </returns>
		public bool AddFirstUnique( in IdString idString )
		{
			if( mList.Contains( idString ) ) { return false; }
			AddFirst( idString );
			return true;
		}

		/// <summary>
		/// 値を先頭に登録
		/// </summary>
		/// <remarks>
		/// 既に登録済の場合も登録される。
		/// </remarks>
		/// <param name="idString"></param>
		public void AddFirst( in IdString idString )
		{
			mList.Insert( 0, idString );
			OnTagChanged( idString );
		}

		/// <summary>
		/// 一意な値を指定位置に登録
		/// </summary>
		/// <param name="idString"></param>
		/// <returns> 登録時 true。既に登録済の場合 false </returns>
		public bool InsertUnique( int index, in IdString idString )
		{
			if( mList.Contains( idString ) ) { return false; }
			Insert( index, idString );
			return true;
		}

		/// <summary>
		/// 値を指定位置に登録
		/// </summary>
		/// <remarks>
		/// 既に登録済の場合も登録される。
		/// </remarks>
		/// <param name="idString"></param>
		public void Insert( int index, in IdString idString )
		{
			index = Mathf.Clamp( index, 0, mList.Count );
			mList.Insert( index, idString );
			OnTagChanged( idString );
		}

		/// <summary>
		/// 指定要素を先頭から1件削除
		/// </summary>
		/// <returns> 削除時 true </returns>
		public bool RemoveFirst( in IdString idString )
		{ 
			bool ret = mList.Remove( idString ); 
			if( ret )
			{
				OnTagChanged( idString );
			}
			return ret;
		}

		/// <summary>
		/// 指定要素を先頭から削除
		/// </summary>
		/// <returns> 削除要素数 </returns>
		public int RemoveFirst( in IdString idString, int count )
		{
			int ret = 0;
			for( int i = 0; i < count; ++i )
			{
				if( !mList.Remove( idString ) ){ break; }
				++ret;
			}
			if( 0 < ret )
			{
				OnTagChanged( idString );
			}

			return ret; 
		}

		/// <summary>
		/// 指定要素を末尾から 1 件削除
		/// </summary>
		/// <returns> 削除時 true </returns>
		public bool RemoveLast( in IdString idString )
		{ 
			return 0 < RemoveLast( idString, 1 );
		}

		/// <summary>
		/// 指定要素を末尾から削除
		/// </summary>
		/// <returns> 削除要素数 </returns>
		public int RemoveLast( in IdString idString, int count )
		{ 
			int ret = 0;
			for( int i = mList.Count -1 ; 0 <= i; --i )
			{
				if( mList[ i ] != idString ){ continue; }
				mList.RemoveAt( i );
				++ret;
			}
			if( 0 < ret )
			{
				OnTagChanged( idString );
			}
			return ret; 
		}


		/// <summary>
		/// 指定位置要素を削除
		/// </summary>
		/// <param name="idString"></param>
		/// <returns></returns>
		public bool RemoveAt( int index )
		{
			if( index < 0 || mList.Count <= index ) { return false; }
			mList.RemoveAt( index );
			return true;
		}

		/// <summary>
		/// 指定要素を全件削除
		/// </summary>
		/// <param name="idString"></param>
		/// <returns></returns>
		public bool RemoveAll( in IdString idString )
		{
			return 0 < RemoveLast( idString, int.MaxValue );
		}


		//public bool Has( in IdString idString )
		//{
		//	//return 
		//}


		private void OnTagChanged( in IdString idString )
		{
			IsDirty = true;
		}

		private void UpdateParentCount()
		{
			if( !IsDirty ){ return; }
			IsDirty = false;

			mCounter.Clear();
			foreach( var idString in mList )
			{
				mCounter.TryGetValue( idString, out var count );
				mCounter[idString] = count + 1;
			}
		}

		#region IEnumerable 
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator(){ return this.GetEnumerator(); }
		public IEnumerator< IdString > GetEnumerator() { return mList.GetEnumerator(); }
		#endregion

	}
}