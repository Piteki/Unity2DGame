using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public class IdStringContainer : IEnumerable< IdString >, ICollection< IdString >, ISerializationCallbackReceiver, IDisposable
	{
		public const int ContainerDefaultCapacity = 32;

		[SerializeField]
		private List< IdString > mList;

		private Dictionary<IdString, int> mCounter;
		private Dictionary<IdString, int> mImplicitCounter;

		/// <summary>
		/// 要素変更通知
		/// </summary>
		/// <remarks>
		/// 要素の追加または削除があった際に呼び出される。
		/// bool は 追加時 true, 削除時 false が渡される
		/// </remarks>
		public event Action< IdString, bool > EventElementChanged;

		/// <summary>
		/// データ変更フラグ
		/// </summary>
		/// <remarks>
		/// フラグが true の場合、クラス内部のカウンタは未更新状態。
		/// Has 系のメソッドが呼ばれる UpdateParentCount を手動で呼ぶ事で最新状態に更新される。
		/// </remarks>
		public bool IsDirty{ get; private set; }

		/// <summary>
		/// Count 
		/// </summary>
		public int Count => mList.Count;

		/// <summary>
		/// インデクサ
		/// </summary>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public IdString this[ int idx ]
		{
			get { return mList[ idx ]; }
			set 
			{ 
				var oldValue = mList[ idx ];
				mList[ idx ] = value; 
				OnElementChanged( oldValue, false ); 
				OnElementChanged( value, true ); 
			}
		}

	#region Constructor ----------------------------------------------------------------------------------------------------

		public IdStringContainer()
			: this( ContainerDefaultCapacity )
		{
		}

		public IdStringContainer( int capacity )
		{
			if( capacity <= 0 )
			{
				capacity = ContainerDefaultCapacity;
			}
			mList = new( capacity );
			mCounter = new( capacity );
			mImplicitCounter = new( capacity );
		}

		public IdStringContainer( ICollection< IdString > collection )
			: this( collection == null ? 0 : collection.Count, collection )
		{
		}

		public IdStringContainer( int capacity, ICollection< IdString > collection )
			: this( capacity )
		{
			if( collection == null )
			{
				return;
			}
			mList.AddRange( collection );
		}

	#endregion

	#region Public methods ----------------------------------------------------------------------------------------------------

		/// <summary>
		/// 破棄
		/// </summary>
		/// <remarks>
		/// Clear() と異なり Tag を保持していても EventElementChanged を発行しない点に注意。
		/// </remarks>
		public void Dispose()
		{
			EventElementChanged = null;

			mList.Clear();
			mCounter.Clear();
			mImplicitCounter.Clear();
		}

		/// <summary>
		/// すべて削除
		/// </summary>
		public void Clear()
		{
			List< IdString > tmpList = 0 < mList.Count ? new( mList ) : null;

			mList.Clear();
			mCounter.Clear();
			mImplicitCounter.Clear();

			if( tmpList != null )
			{
				foreach( var idString in tmpList )
				{
					OnElementChanged( idString, false );
				}
			}
		}

		/// <summary>
		/// コレクションを末尾に登録
		/// </summary>
		/// <param name="collection"></param>
		public void AddRange( IEnumerable< IdString > collection )
		{
			mList.AddRange( collection );
			foreach( var idString in collection )
			{
				OnElementChanged( idString, true );
			}
		}

		/// <summary>
		/// 一意な値を末尾に登録
		/// </summary>
		/// <param name="idString"></param>
		/// <returns> 登録時 true。既に登録済の場合 false </returns>
		public bool AddUnique( in IdString idString )
		{
			if( mList.Contains( idString ) ) { return false; }
			Add( idString );
			return true;
		}

		/// <summary>
		/// 値を末尾に登録
		/// </summary>
		/// <remarks>
		/// 既に登録済の場合も登録される。
		/// </remarks>
		/// <param name="idString"></param>
		public void Add( in IdString idString )
		{
			mList.Add( idString );
			OnElementChanged( idString, true );
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
			OnElementChanged( idString, true );
		}

		/// <summary>
		/// 子要素を追加
		/// </summary>
		/// <remarks>
		/// 指定要素を末端の子要素として末尾に登録する。
		/// 対象の祖先にあたる要素が既に存在する場合はすべて削除される。
		/// 対象と同一あるいは対象の子孫にあたる要素が既に存在する場合は対象は登録されない。
		/// </remarks>
		/// <returns> 対象を新たに登録した場合は true </returns>
		public bool AddLeafElement( in IdString idString )
		{
			List< IdString > tmpList = null;
			bool bFind = false;
			for( int idx = mList.Count -1; 0 <= idx; --idx )
			{
				var elem = mList[ idx ];
				if( idString == elem ){ bFind = true; continue; }
				if( elem.IsDescendantOf( idString ) ){ bFind = true; continue; }
				if( idString.IsDescendantOf( elem ) )
				{
					if( tmpList == null ){ tmpList = new List< IdString >(); }
					tmpList.Add( elem );
					mList.RemoveAt( idx );
				}
			}
			if( !bFind )
			{
				mList.Add( idString );
				OnElementChanged( idString, true );
			}
			if( tmpList != null )
			{
				foreach( var removeElem in tmpList )
				{
					OnElementChanged( removeElem, false );
				}
			}
			return !bFind;
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
			OnElementChanged( idString, true );
		}

		/// <summary>
		/// 指定要素を先頭から1件削除
		/// </summary>
		/// <returns> 削除時 true </returns>
		public bool Remove( in IdString idString )
		{ 
			bool ret = mList.Remove( idString ); 
			if( ret )
			{
				OnElementChanged( idString, false );
			}
			return ret;
		}

		/// <summary>
		/// 指定要素を先頭から削除
		/// </summary>
		/// <returns> 削除要素数 </returns>
		public int Remove( in IdString idString, int count )
		{
			int ret = 0;
			for( int i = 0; i < count; ++i )
			{
				if( !mList.Remove( idString ) ){ break; }
				++ret;
			}
			if( 0 < ret )
			{
				OnElementChanged( idString, false );
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
				OnElementChanged( idString, false );
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
			if( !IsValidIndex( index ) ) { return false; }
			var elem = mList[ index ];
			mList.RemoveAt( index );
			OnElementChanged( elem, false );
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
		/// 指定の値を直接保持しているか
		/// </summary>
		/// <remarks>
		/// 指定の値を直接保持していれば true を返す。
		/// HasExact() と等価。
		/// </remarks>
		/// <returns> 値発見時 true </returns>
		public bool Contains( IdString idString )
		{
			return HasExact( idString );
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
		
		/// <summary>
		/// 先頭要素を取得
		/// </summary>
		public IdString FirstOrDefault()
		{
			return 0 < mList.Count ? mList[0] : IdString.None;
		}

		/// <summary>
		/// 末尾要素を取得
		/// </summary>
		public IdString LastOrDefault()
		{
			return 0 < mList.Count ? mList[mList.Count-1] : IdString.None;
		}

		/// <summary>
		/// 指定位置要素を取得
		/// </summary>
		public IdString ElementAtOrDefault( int idx )
		{
			return IsValidIndex(idx) ? mList[idx] : IdString.None;
		}

		/// <summary>
		/// インデックスが有効かチェック
		/// </summary>
		public bool IsValidIndex( int idx )
		{
			return 0 <= idx && idx < mList.Count;
		}

		

	#endregion

	#region Non Public methods----------------------------------------------------------------------------------------------------


		private void OnElementChanged( in IdString idString, bool isAdded )
		{
			IsDirty = true;

			EventElementChanged?.Invoke( idString, isAdded );
		}

		private void UpdateParentCount( bool force = false )
		{
			if( !IsDirty && !force ){ return; }
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

	#endregion

	#region IEnumerable ----------------------------------------------------------------------------------------------------
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator(){ return this.GetEnumerator(); }
		public IEnumerator< IdString > GetEnumerator() { return mList.GetEnumerator(); }
	#endregion

	#region ICollection 
		bool ICollection< IdString >.IsReadOnly => false;

		void ICollection< IdString >.Add( IdString idString )
		{
			Add( idString );
		}

		bool ICollection< IdString >.Remove( IdString idString )
		{
			return Remove( idString );
		}
		bool ICollection< IdString >.Contains( IdString idString )
		{
			return HasExact( idString );
		}

		public void CopyTo( IdString[] array, int arrayIndex )
		{
			foreach( var idString in mList )
			{
				array[arrayIndex] = idString;
				++arrayIndex;
			}
		}
	#endregion

	#region ISerializationCallbackReceiver ----------------------------------------------------------------------------------------------------
		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			UpdateParentCount(true);
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}
	#endregion


	}
}