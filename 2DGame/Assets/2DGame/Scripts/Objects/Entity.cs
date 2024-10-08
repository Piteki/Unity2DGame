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
	/// Entity
	/// </summary>
	public class Entity : MonoBehaviour
	{
		[SerializeField] private List<Collider2D> _BodyColliders;


		[SerializeField] private float _MaxHealth = 1;

		public List<Collider2D> BodyColliders => _BodyColliders;

		public float Health { get; protected set; }

		protected virtual void Awake()
		{
			ResetStatus();
		}

		protected virtual void OnDestroy()
		{
		}

		protected virtual void OnEnable()
		{
			
		}

		public virtual void ResetStatus()
		{
			Health = Mathf.Max( 0, _MaxHealth );
		}


		public virtual void Damage( float damage )
		{
			Health -= damage;
			if( Health <= 0 )
			{
				BreakUp();
			}
		}

		public virtual void BreakUp()
		{
			if( gameObject == null ){ return; }
			Destroy( gameObject, 0.5f );
		}

		protected virtual void OnTriggerEnter2D( Collider2D collision )
		{
			if( collision == null ){ return; }
			Log.Verbose( $"OnTriggerEnter2D collider = {collision.name}" );

		}
		protected virtual void OnTriggerExit2D( Collider2D collision )
		{
			if( collision == null ){ return; }
			Log.Verbose( $"OnTriggerExit2D collider = {collision.name}" );

		}


	}
}
