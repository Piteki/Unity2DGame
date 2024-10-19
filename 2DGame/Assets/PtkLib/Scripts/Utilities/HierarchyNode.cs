using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace Ptk
{

	/// <summary>
	/// 階層 Node の interface
	/// </summary>
	/// <remarks>
	/// 実際に階層表現や操作を行うための HierarchyContainer を提供する interface。
	/// 実装時は自身を包含する HierarchyContainer クラスをプロパティ Hierarchy で提供すること。
	/// また Serializable クラスとして使用する場合はこの HierarchyContainer のフィールドを SerializeReference とすること。
	/// 可能な限り Hierarchy プロパティは非 null を保障する事が望ましい。
	/// HierarchyContainer 内部からの初回操作時に各 Node の Hierarchy が null である場合は自動生成し set する実装となっているが、
	/// 実装クラス内で Hierarchy プロパティの非 null が保障されている場合は外部からの setter による更新を無視してよい。
	/// 
	/// また Undo や JsonUtility 等で Deserialize を行うクラスの場合、interface の Serialize がサポートされていないために Hierarchy.RefObject の参照が途切れる。
	/// ISerializationCallbackReceiver を使用し Hierarchy.RestoreRefObject() で適切に再設定を行うことで解決できる。
	/// </remarks>
	public interface IHierarchyNode
	{
		/// <summary>
		/// 階層クラス
		/// </summary>
		/*[field:SerializeReference]*/ 
		HierarchyContainer Hierarchy { get; set; }
	}

	/// <summary>
	/// 階層 Node の基底となる HierarchyNodeBase
	/// </summary>
	/// <remarks>
	/// このクラスを継承しなくても IHierarchyNode の実装でも HierarchyContainer の利用は可能。
	/// </remarks>
	[Serializable] public class HierarchyNodeBase : IHierarchyNode, ISerializationCallbackReceiver
	{
#pragma warning disable 0649
		[field:SerializeReference] private HierarchyContainer _Hierarchy;
#pragma warning restore 0649 
		
		public HierarchyContainer Hierarchy { 
			get 
			{
				if( _Hierarchy == null )
				{
					_Hierarchy = new HierarchyContainer( this );
				}
				return _Hierarchy;
			}
			set
			{
				// mHierarchy = value;
				// no set 
			}
		}

		#region ISerializationCallbackReceiver 
		public virtual void OnBeforeSerialize(){} 
		public virtual void OnAfterDeserialize()
		{
			// RefObject を再設定
			Hierarchy.RestoreRefObject( this );
		}

		#endregion 	
	}

	/// <summary>
	/// 階層の操作を行う Container class。
	/// </summary>
	/// <remarks>
	/// IHierarchyNode のクラスメンバとしてこの container を保持する事で Tree 構造の Hierarchy を表現する事ができる。
	/// Node を Serializable クラスとして使用する場合はこの HierarchyContainer のフィールドを SerializeReference とすること。
	/// </remarks>
	[Serializable] public class HierarchyContainer : ISerializationCallbackReceiver, IEnumerable< IHierarchyNode >
	{
#pragma warning disable 0649
		[SerializeReference] [HideInInspector] private IHierarchyNode _RefObject = null;
		[SerializeReference] private List< IHierarchyNode > _Children = new List< IHierarchyNode >();
#pragma warning restore 0649
			
		[NonSerialized] private IHierarchyNode mParent = null; 
		[NonSerialized] private int mDepth = 0;
		[NonSerialized] private bool mDirty = false;

		public IHierarchyNode RefObject => _RefObject;

		public int ChildrenCount => _Children.Count;
		public IHierarchyNode Parent => mParent;
		public int Depth { get { UpdateDepth(); return mDepth; } }


		public HierarchyContainer( IHierarchyNode refObject )
		{
			RestoreRefObject( refObject );
		}

		/// <summary>
		/// RefObject を再設定
		/// </summary>
		/// <remarks>
		/// Deserialize 後などで RefObject の参照が喪失した際に再設定する。
		/// Undo や JsonUtility 等で Deserialize した場合、interface の Serialize がサポートされていないために参照が途切れる。
		/// ISerializationCallbackReceiver で適切に再設定を行うことで解決できる。
		/// </remarks>
		public void RestoreRefObject( IHierarchyNode refObject )
		{
			_RefObject = refObject;
			if( _RefObject == null )
			{
				Debug.LogError( "refObject is null." );
			}
		}

		#region ISerializationCallbackReceiver 
		public void OnBeforeSerialize(){} 
		public void OnAfterDeserialize()
		{
			mDirty = true; 
			if( _Children != null )
			{
				foreach( var child in _Children )
				{
					if( child == null ){ continue; }
					if( child.Hierarchy == null )
					{
						Debug.LogWarning( "child.Hierarchy is null." );
						child.Hierarchy = new HierarchyContainer( child );
					}
					child.Hierarchy._SetParent( _RefObject );
				}
			}
		}

		#endregion 

		#region IEnumerable 
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator(){ return this.GetEnumerator(); }
		public IEnumerator< IHierarchyNode > GetEnumerator() { return GetAllDescendants(); }
		#endregion

	
		private void UpdateDepth()
		{
			if( !mDirty ){ return; }

			// mDirty の場合 == OnAfterDeserialize 直後なので root から全要素更新
			var root = FindRootNode();
			if( root == null || root.Hierarchy == null )
			{
				Debug.LogError( "root.Hierarchy is null." );
				return;
			}
			root.Hierarchy.UpdateDepthRecursively();
		}

		private void UpdateDepthRecursively()
		{
			if( mParent != null )
			{
				if( mParent.Hierarchy == null )
				{
					Debug.LogWarning( "mParent.Hierarchy is null." );
					mParent.Hierarchy = new HierarchyContainer( mParent );
					mParent.Hierarchy._AddChild( _RefObject, 0 );
				}
				mDepth = mParent.Hierarchy.mDepth + 1;
			}else
			{
				mDepth = 0;
			}
			mDirty = false;
			foreach( var child in _Children )
			{
				if( child == null ){ continue; }
				if( child.Hierarchy == null )
				{
					Debug.LogWarning( "child.Hierarchy is null." );
					child.Hierarchy = new HierarchyContainer( child );
					child.Hierarchy._SetParent( _RefObject );
				}
				child.Hierarchy.UpdateDepthRecursively();
			}
		}


		private void _SetParent( IHierarchyNode parent )
		{
			mParent = parent;
		}

		private bool _AddChild( IHierarchyNode child, int index )
		{
			if( child == null ){ return false; }
			if( ContainsChild( child ) ){ return false; }
			index = Math.Max( 0, Math.Min( index, _Children.Count ) );
			_Children.Insert( index, child );
			return true;
		}
		private bool _RemoveChild( IHierarchyNode child )
		{
			if( child == null ){ return false; }
			if( !_Children.Remove( child ) ){ return false; }
			return true;
		}

		public IHierarchyNode GetChild( int index )
		{
			//return Util.GetElement( _Children, index );
			return _Children.ElementAtOrDefault( index );
		}

		public TNode GetChild< TNode >( int index )
			where TNode : class, IHierarchyNode
		{
			return GetChild( index ) as TNode;
		}

		/// <summary>
		/// 子を取得
		/// </summary>
		/// <remarks>
		/// GC を避ける目的で List をそのまま返す。中を触らないこと。
		/// </remarks>
		public List< IHierarchyNode > GetChildren()
		{
			return _Children;
		}

		public IEnumerator< IHierarchyNode > GetAllDescendants() 
		{ 
			yield return _RefObject;
			foreach( var child in _Children )
			{	
				if( child == null || child.Hierarchy == null ){ continue; }
				var enumerator = child.Hierarchy.GetEnumerator();
				while( enumerator.MoveNext() )
				{
					yield return enumerator.Current;
				}
			}
		}

		/// <summary>
		/// 子孫に含まれるかを検索
		/// </summary>
		/// <param name="node"> 検査対象 </param>
		/// <returns> node が子孫に含まれる場合 true </returns>
		public bool IsDecendant( IHierarchyNode node )
		{
			// 無限回避
			int ct = 0;
			const int ctMax = int.MaxValue - 1;
			var current = node;
			while( current != null )
			{
				if( current == _RefObject ){ return true; }
				current = current.Hierarchy.Parent;
				++ct;
				if( ctMax < ct )
				{
					Debug.LogError( "GetRootNode count limit over." );
					break;
				}
			}
			return false;
		}
		

		public IHierarchyNode FindRootNode()
		{
			// 無限回避
			int ct = 0;
			const int ctMax = int.MaxValue - 1;

			var rootContainer = this;
			while( rootContainer.mParent != null && rootContainer.mParent.Hierarchy != null )
			{
				rootContainer = rootContainer.mParent.Hierarchy;
				++ct;
				if( ctMax < ct )
				{
					Debug.LogError( "GetRootNode count limit over." );
					break;
				}
			}
			return rootContainer._RefObject;
		}

		public bool SetParent( IHierarchyNode parent )
		{
			if( mParent == parent ){ return true; }
			return SetParent( parent, parent == null || parent.Hierarchy == null ? 0 : parent.Hierarchy.ChildrenCount );
		}
		public bool SetParent( IHierarchyNode parent, int childIndex )
		{
			if( mParent != null )
			{
				if( mParent.Hierarchy == null )
				{
					Debug.LogWarning( "SetParent failed. mParent HierarchyContainer is null." );
				}
				else
				{
					if( mParent == parent )
					{
						return mParent.Hierarchy.SetChildIndex( _RefObject, childIndex );
					}
					mParent.Hierarchy._RemoveChild( _RefObject );
				}
			}
			_SetParent( null );
			if( parent != null )
			{ 
				if( parent.Hierarchy == null )
				{
					parent.Hierarchy = new HierarchyContainer( parent );
				}
				if( !parent.Hierarchy._AddChild( _RefObject, childIndex ) )
				{
					Debug.LogWarning( "_AddChild failed." );
					UpdateDepthRecursively();
					return false;
				}
			}
			_SetParent( parent );
			UpdateDepthRecursively();
			return true;
		}

		public IHierarchyNode GetParent()
		{
			return Parent;
		}

		public TNode GetParent< TNode >()
			where TNode : class, IHierarchyNode
		{
			return Parent as TNode;
		}
		

		public bool AddChild( IHierarchyNode child )
		{
			return AddChild( child, ChildrenCount );
		}
		public bool AddChild( IHierarchyNode child, int childrenIndex )
		{
			if( child == null )
			{ 
				Debug.LogError( "AddChild failed. child is null." );
				return false; 
			}
			if( child.Hierarchy == null )
			{
				child.Hierarchy = new HierarchyContainer( child );
			}
			return child.Hierarchy.SetParent( _RefObject, childrenIndex );
		}

		public bool RemoveChild( IHierarchyNode child )
		{
			if( child == null )
			{ 
				Debug.LogError( "AddChild failed. child is null." );
				return false; 
			}
			if( !ContainsChild( child ) )
			{ 
				Debug.LogError( "AddChild failed. child not found." );
				return false; 
			}
			if( child.Hierarchy == null )
			{ 
				Debug.LogWarning( "AddChild HierarchyContainer is null." );
				return _RemoveChild( child ); 
			}
			return child.Hierarchy.SetParent( null );
		}
		public bool RemoveChildAt( int index )
		{
			if( index < 0 || _Children.Count <= index )
			{
				Debug.LogWarning( "RemoveChildAt out of range index." );
				return false; 
			}
			var child = _Children[ index ];
			return RemoveChild( child );
		}

		public void RemoveAllChild()
		{
			for( int i = _Children.Count - 1; 0 <= i; --i )
			{
				RemoveChildAt( i );
			}
		}

		public bool SetChildIndex( IHierarchyNode child, int index )
		{
			if( !_RemoveChild( child ) )
			{
				Debug.LogError( "SetChildIndex failed. Child not found." );
				return false;
			}
			return _AddChild( child, index );
		}

		public int GetChildIndex( IHierarchyNode child )
		{
			
			for( int i = 0; i < _Children.Count; ++i )
			{
				var element = _Children[ i ];
				if( element == child ){ return i; }
			}
			return -1;
		}
		public bool ContainsChild( IHierarchyNode child )
		{
			foreach( var element in _Children )
			{
				if( element == child ){ return true; }
			}
			return false;
		}

		public int GetIndexInParent()
		{
			if( mParent == null ){ return 0; }
			if( mParent.Hierarchy == null )
			{ 
				Debug.LogError( "HierarchyContainer is null." );
				return 0; 
			}
			return mParent.Hierarchy.GetChildIndex( _RefObject );
		}
		
		public void SortChildren< TNode >( Comparison< TNode > comparison )
			where TNode : class, IHierarchyNode
		{
			if( comparison == null ){ return; }

			_Children.Sort( ( a, b ) =>
			{
				var nodeA = a as TNode;
				var nodeB = b as TNode;
				return comparison.Invoke( nodeA, nodeB );
			} );
		}

		public void SortChildren( Comparison< IHierarchyNode > comparison )
		{
			_Children.Sort( comparison );
		}

		public void SortChildren()
		{
			_Children.Sort();
		}
	}

#if UNITY_EDITOR

	/// <summary>
	/// CustomPropertyDrawer HierarchyContainer
	/// </summary>
	//[CustomPropertyDrawer( typeof( HierarchyContainer ) )]
	public class HierarchyContainerPropertyDrawer : PropertyDrawer {

		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) 
		{
			EditorGUILayout.PropertyField( property, property.isExpanded );
		}
		public void OnGUINN( Rect position, SerializedProperty property, GUIContent label ) 
		{
			// root の Foldout描画
			//property.isExpanded = EditorGUI.Foldout( position, property.isExpanded, label, true );
			//if( !property.isExpanded ){ return; }

			using( new EditorGUI.PropertyScope( position, null, property ) ) 
			{
				//property.NextVisible( true );
				do
				{
					if( property.name == "_RefObject" ){ continue; }
					
					// 各要素既定の描画
					//position.y += position.height;
					//position.height = EditorGUI.GetPropertyHeight( property, property.isExpanded );
					using( new EditorGUI.PropertyScope( position, new GUIContent( property.displayName ), property ) )
					{ 
						EditorGUILayout.PropertyField( property, property.isExpanded );
					}

				}while( property.NextVisible( false ) );

			}
			// プロパティの変更を反映します。
			//property.serializedObject.ApplyModifiedProperties();
		}
//		public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
		public float GetPropertyHeightNN( SerializedProperty property, GUIContent label )
		{
			float ret = 0;
			do
			{
				if( property.name == "_RefObject" ){ continue; }

				ret += EditorGUI.GetPropertyHeight( property, property.isExpanded );
			
			}while( property.NextVisible( false ) );

			return ret;
		}

	}

#endif // UNITY_EDITOR
}
