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
		[SerializeField] private float MaxHealth = 1;

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
			Health = Mathf.Max( 0, MaxHealth );
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

	}
}
