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

		[SerializeField] private Collider2D _NormalAttackCollider;

		public Collider2D NormalAttackCollider => _NormalAttackCollider;


		private void OnDestroy()
		{
		}

		private void OnEnable()
		{
			
		}

	}
}
