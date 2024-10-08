using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Ptk.AbilitySystems;
using UnityEngine.Rendering;
using System.Linq;
using Unity.Logging;
using System;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ptk
{
	/// <summary>
	/// Trigger Collider の Container
	/// </summary>
	public class CharacterColliderContainer2D : MonoBehaviour
	{

		[SerializeField] private ColliderInfo _NormalAttackColliderInfo;

		public ColliderInfo NormalAttackColliderInfo => _NormalAttackColliderInfo;


		private GameObject mColliderInstance; // TODO
		public CharacterController2D OwnerCharacter { get; private set; }

		private void OnDestroy()
		{
		}

		private void OnEnable()
		{
			
			
		}

		public void Initialize( CharacterController2D character )
		{
			OwnerCharacter = character;
		}

		public ColliderTriggerReceiver2D InstantiateNormalAttackCollider()
		{
			if( _NormalAttackColliderInfo == null ) { return null; }

			// TODO instance 複数対応
			DestroyNormalAttackCollider();
			mColliderInstance = _NormalAttackColliderInfo.Instantiate();
			if( mColliderInstance == null ) { return null; }

			bool isFlip = OwnerCharacter != null ? OwnerCharacter.IsFlipX : false;
			if( isFlip )
			{
				mColliderInstance.transform.localRotation = Quaternion.Euler( 0, -180, 0 );
			}

			return mColliderInstance.GetComponent<ColliderTriggerReceiver2D>();
			
		}

		public void DestroyNormalAttackCollider()
		{
			// TODO instance 複数対応

			if( mColliderInstance == null ) { return; }
			Destroy( mColliderInstance );

		}
	}
	
	[Serializable]
	public sealed class ColliderInfo
	{
		[SerializeField] private GameObject _Prefab;

		[SerializeField] private Transform _AttachNode;

		public GameObject Instantiate()
		{
			if( _Prefab == null )
			{	
				Log.Error( "_Prefab is null." );
				return null; 
			}

			var ret = GameObject.Instantiate( _Prefab );

			if( _AttachNode != null )
			{
				ret.transform.SetParent( _AttachNode, false );
			}

			return ret;
		}
	}

}
