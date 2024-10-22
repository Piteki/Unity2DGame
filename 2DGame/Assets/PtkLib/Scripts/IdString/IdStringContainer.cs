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

		private Dictionary<IdString, int> mImplicitCounter;

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
		/// 指定位置の要素を削除
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
		/// 指定要素を全て削除
		/// </summary>
		/// <param name="idString"></param>
		/// <returns></returns>
		public bool RemoveAll( in IdString idString )
		{
			return 0 < RemoveLast( idString, int.MaxValue );
		}

		/// <summary>
		/// 指定の値を保持しているか
		/// </summary>
		/// <remarks>
		/// 指定の値を保持していれば true を返す。
		/// 直接保持している値の他、保持している値の親に該当する場合も true を返す。
		/// 例 : IdStringContainer( { Parent.Child, } ).Has( Parent ) == true
		/// </remarks>
		/// <returns> 値発見時 true </returns>
		public bool Has( in IdString idString )
		{
			UpdateParentCount();

			return mCounter.ContainsKey( idString )
				|| mImplicitCounter.ContainsKey( idString );
		}

		/// <summary>
		/// 指定の値を直接保持しているか
		/// </summary>
		/// <remarks>
		/// 指定の値を直接保持していれば true を返す。
		/// 保持している値の親の値は考慮しない。
		/// 例 : IdStringContainer( { Parent.Child, } ).HasExact( Parent ) == false
		/// </remarks>
		/// <returns> 値発見時 true </returns>
		public bool HasExact( in IdString idString )
		{
			UpdateParentCount();

			return mCounter.ContainsKey( idString );
		}
		
		/// <summary>
		/// 指定の値のいずれかを保持しているか
		/// </summary>
		/// <remarks>
		/// 指定の値を一つでも保持していれば true を返す。
		/// 直接保持している値の他、保持している値の親に該当する場合も true を返す。
		/// 例 : IdStringContainer( { Parent.Child1, Parent.Child2, } ).HasAny( { Parent.Child2, Parent.Child3 } ) == true
		/// </remarks>
		/// <returns> 値発見時 true </returns>
		public bool HasAny( in IdStringContainer other )
		{
			UpdateParentCount();

			foreach( var idString in other.mList )
			{
				if( mCounter.ContainsKey( idString )
				 || mImplicitCounter.ContainsKey( idString )
				){ return true; }
			}
			return false;
		}

		/// <summary>
		/// 指定の値のいずれかを直接保持しているか
		/// </summary>
		/// <remarks>
		/// 指定の値を一つでも直接保持していれば true を返す。
		/// 保持している値の親の値は考慮しない。
		/// 例 : IdStringContainer( { Parent.Child1, Parent.Child2, } ).HasAnyExact( { Parent.Child4, Parent } ) == false
		/// </remarks>
		/// <returns> 値発見時 true </returns>
		public bool HasAnyExact( in IdStringContainer other )
		{
			UpdateParentCount();

			foreach( var idString in other.mList )
			{
				if( mCounter.ContainsKey( idString )){ return true; }
			}
			return false;
		}

		/// <summary>
		/// 指定のすべての値を保持しているか
		/// </summary>
		/// <remarks>
		/// 指定の値をすべて保持していれば true を返す。
		/// 直接保持している値の他、保持している値の親に該当する場合も true を返す。
		/// 例 : IdStringContainer( { Parent.Child1, Parent.Child2, } ).HasAll( { Parent.Child1, Parent } ) == true
		/// </remarks>
		/// <returns> 値発見時 true </returns>
		public bool HasAll( in IdStringContainer other )
		{
			UpdateParentCount();

			foreach( var idString in other.mList )
			{
				if( !mCounter.ContainsKey( idString )
				 && !mImplicitCounter.ContainsKey( idString )
				){ return false; }
			}
			return true;
			
		}
		/// <summary>
		/// 指定のすべての値を直接保持しているか
		/// </summary>
		/// <remarks>
		/// 指定の値をすべて直接保持していれば true を返す。
		/// 保持している値の親の値は考慮しない。
		/// 例 : IdStringContainer( { Parent.Child1, Parent.Child2, } ).HasAllExact( { Parent.Child1, Parent } ) == false
		/// </remarks>
		/// <returns> 値発見時 true </returns>
		public bool HasAllExact( in IdStringContainer other )
		{
			UpdateParentCount();

			foreach( var idString in other.mList )
			{
				if( !mCounter.ContainsKey( idString )){ return false; }
			}
			return true;
			
		}
		



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

			mImplicitCounter.Clear();
			foreach ( var idString in mList )
			{
				var attrData = idString.AttrData;
				if( attrData == null ){ continue; }
				foreach( var parent in attrData.ParentList )
				{
					mImplicitCounter.TryGetValue( parent, out var count );
					mImplicitCounter[parent] = count + 1;
				}
			}
		}

		#region IEnumerable 
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator(){ return this.GetEnumerator(); }
		public IEnumerator< IdString > GetEnumerator() { return mList.GetEnumerator(); }
		#endregion

	}
}