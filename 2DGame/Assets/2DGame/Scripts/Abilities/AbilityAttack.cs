using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Ptk.AbilitySystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ptk
{



	[CreateAssetMenu(fileName = "New Attack Ability.asset", menuName = "2DGame/Ability/Attack")]
	public class AbilityAttack : AbilityData 
	{
		public CharacterController2D CharaController { get; private set; }

		public override void OnEnterSystem( AbilitySystem abilitySystem )
		{
			base.OnEnterSystem( abilitySystem );

			CharaController = abilitySystem == null ? null : abilitySystem.GetComponent<CharacterController2D>();
		}
		public override void OnExitSystem()
		{
			base.OnExitSystem();
			CharaController = null;
		}

		protected override bool CheckCanExecute()
		{
			return CharaController == null ? false : true;
		}


		protected override void OnExecuted()
		{
			base.OnExecuted();
				 
			if ( CharaController == null ) { Finish(); return; }
			if ( CharaController.Animator == null ) { Finish(); return; }
			
			CharaController.Animator.Play( CharacterController2D.AnimatorHash_NormalAttack );
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if ( CharaController == null 
			 || true
			){
				Finish();
				return;
			}
		}

	}
}
