using System.Collections.Generic;
using UnityEngine;
using Unity.Logging;
using Ptk.AbilitySystems;
using Ptk.IdStrings;

namespace Ptk
{

	[CreateAssetMenu(fileName = "New Attack Ability.asset", menuName = "2DGame/Ability/Attack")]
	public class AbilityDataAttack : AbilityData< AbilityAttack, AbilityDataAttack >
	{
		[SerializeField] private float _BaseDamage = 1;

		[SerializeField] IdString _PlaneTag;

		[IdStringView]
		[SerializeField] IdString _OwnTag;

		[IdStringView( IgnoreHideInViewer = true )]
		[SerializeField] IdString _IgnoreTag;

		[IdStringView( Filter = "AbilityTag" )]
		[SerializeField] IdString _FilterTag;
		[IdStringView( Filter = "AbilityTag." )]
		[SerializeField] IdString _FilterGroupTag;
		[IdStringView( Filter = "AbilityTag.Attack" )]
		[SerializeField] IdString _FilterAttackTag;
		[IdStringView( Filter = "AbilityTag.Attack." )]
		[SerializeField] IdString _FilterAttackGroupTag;

		[IdStringView( Filter = "Hiding", IgnoreHideInViewer = true )]
		[SerializeField] IdString _FilterIgnoreYag;

		[SerializeField] private float _NextProperty = 1;

		public float BaseDamage => _BaseDamage;
	}

	public class AbilityAttack : AbilityBase< AbilityAttack, AbilityDataAttack >, IAttackAnimationEventListener
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
			if( !base.CheckCanExecute() ){ return false; }
			return CharaController == null ? false : true;
		}


		protected override void OnExecuted()
		{
			base.OnExecuted();
				 
			if ( CharaController == null ) { Finish(); return; }
			if ( CharaController.Animator == null ) { Finish(); return; }
			
			var eventReceiver = CharaController.AttackAnimationEventReceiver;
			if ( eventReceiver != null )
			{
				eventReceiver.AddListener( this );
			}
			
			
			CharaController.Animator.Play( CharacterController2D.AnimatorHash_NormalAttack );
		}

		protected override void OnFinished()
		{
			base.OnFinished();

			if ( CharaController == null ) { return; }
			var eventReceiver = CharaController.AttackAnimationEventReceiver;
			if ( eventReceiver != null )
			{
				eventReceiver.RemoveListener( this );
			}
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if ( CharaController == null )
			{
				Finish();
				return;
			}
		}

		public void OnBeginAttackHit( AnimationEvent animationEvent )
		{
			Log.Verbose( "AbilityAttack.OnBeginAttackHit" );

			var colliderContainer = CharaController == null ? null : CharaController.CharacterColliderContainer;
			if( colliderContainer == null ){ return; }


			if( Data == null ) { Log.Fatal( "Data is null." ); return; }

			var triggerReceiver = colliderContainer.InstantiateNormalAttackCollider();
			if( triggerReceiver != null )
			{
				triggerReceiver.EventTriggerEnter2D += hitCollider => 
				{
					// Entity にダメージ
					var entity = EntityColliders.GetEntity( hitCollider );
					if( entity != null )
					{
						entity.Damage( Data.BaseDamage );
					}
				};
			}
		}

		public void OnEndAttackHit( AnimationEvent animationEvent )
		{
			Log.Verbose( "AbilityAttack.OnEndAttackHit" );

			var colliderContainer = CharaController == null ? null : CharaController.CharacterColliderContainer;
			if( colliderContainer == null ){ return; }

			colliderContainer.DestroyNormalAttackCollider();

			Finish();
		}

	}
}
