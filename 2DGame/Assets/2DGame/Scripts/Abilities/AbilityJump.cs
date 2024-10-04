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



	[CreateAssetMenu(fileName = "New Jump Ability.asset", menuName = "2DGame/Ability/Jump")]
	public class AbilityJump : AbilityData 
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
			return CharaController == null ? false : CharaController.CheckCanJump();
		}


		protected override void OnExecuted()
		{
			base.OnExecuted();
			     
 			if ( CharaController == null ) { Finish(); return; }
			
			if( !CharaController.DoJump() ) { Finish(); return; }


		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if ( CharaController == null 
			 || !CharaController.IsJumping
			// || CharaController.IsGrounded
			){
				Finish();
				return;
			}
		}

	}
}
