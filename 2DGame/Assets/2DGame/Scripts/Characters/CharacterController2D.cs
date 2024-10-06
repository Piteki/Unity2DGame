using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Ptk.AbilitySystems;
using UnityEngine.Rendering;
using System.Linq;
using Unity.Logging;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ptk
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class CharacterController2D : MonoBehaviour, ICharacterAnimationEventListener
	{
		static public readonly int AnimatorHash_IsMoving = Animator.StringToHash("IsMoving");
		static public readonly int AnimatorHash_IsJumping = Animator.StringToHash("IsJumping");
		static public readonly int AnimatorHash_MoveSpeed = Animator.StringToHash("MoveSpeed");
		static public readonly int AnimatorHash_VelocityX = Animator.StringToHash("VelocityX");
		static public readonly int AnimatorHash_VelocityY = Animator.StringToHash("VelocityY");
		static public readonly int AnimatorHash_IsGrounded = Animator.StringToHash("IsGrounded");

		static public readonly int AnimatorHash_NormalAttack = Animator.StringToHash("NormalAttack");

		static public readonly float GroudedCheckIgnoreTimeAfterJump = 0.1f;

		[SerializeField] BoxCollider2D _BoxCollider;
		[SerializeField] SpriteRenderer _SpriteRenderer;
		[SerializeField] Animator _Animator;
		[SerializeField] PlayerInput _PlayerInput;
		[SerializeField] CharacterAnimationEventReceiver _CharacterAnimationEventReceiver;
		
		[SerializeField] AbilitySystem _AbilitySystem;

		[SerializeField] float _moveSpeed = 5;
		[SerializeField] float _moveAcceleration = 1.0f;
		[SerializeField] float _AirMoveAcceleration = 0.2f;
		[SerializeField] float _JumpSpeed = 5;
		[SerializeField] float _JumpBrakeTime = 0.5f;
		[SerializeField] int _JumpCountLimit = 1;
		[SerializeField] bool _IgnoreGroundedWhenFirstJump = false;
		[SerializeField] float _BrakeAcceleration = 0.5f;
		[SerializeField] float _AirBrakeAcceleration = 0.1f;
		[SerializeField] float _MoveStopSpeedThreshold = 0.01f;

		[SerializeField] float _GroundCastDistance = 0.1f;
		[SerializeField] float _GroundAdsorbAngle = 46.0f;

		[SerializeField] ContactFilter2D _CheckGroudedContactFilter = new()
		{
			useNormalAngle = true,
			minNormalAngle =  44.0f,
			maxNormalAngle = 136.0f,
		};

		private Rigidbody2D mRigidbody;

		private Vector2 mMoveInput;
		private bool mJumpInput;

		private Vector2 mMoveDirection;


		private RaycastHit2D[] mRaycastHit2Ds = new RaycastHit2D[10];
		private RaycastHit2D mLastHitGround;

		private float mJumpStartTime;

		public AbilitySystem  AbilitySystem => _AbilitySystem;
		public Animator  Animator => _Animator;
		public bool IsMoving { get; private set; }
		public bool CanJump => CheckCanJump();
		public bool IsJumping => 0 < JumpCount;
		public int JumpCount { get; private set; }
		public bool IsGrounded { get; private set; }

		protected virtual void OnEnable()
		{
			mRigidbody = GetComponent<Rigidbody2D>();
			if( _AbilitySystem == null )
			{
				_AbilitySystem = GetComponent<AbilitySystem>();
			}
			if( _PlayerInput == null )
			{
				_PlayerInput = GetComponent<PlayerInput>();
			}
			if( _BoxCollider == null )
			{
				_BoxCollider = GetComponentInChildren<BoxCollider2D>();
			}
			if( _SpriteRenderer == null )
			{
				_SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
			}
			if( _Animator == null )
			{
				_Animator = GetComponentInChildren<Animator>();
			}
			if( _CharacterAnimationEventReceiver == null )
			{
				_CharacterAnimationEventReceiver = GetComponentInChildren<CharacterAnimationEventReceiver>();
			}
			
			if( _CharacterAnimationEventReceiver != null )
			{
				_CharacterAnimationEventReceiver.AddListener( this );
			}
			
			//if( _PlayerInput != null )
			//{
			//	_PlayerInput.onActionTriggered -= OnInputActionTriggered;
			//	_PlayerInput.onActionTriggered += OnInputActionTriggered;
			//}

		}

		protected virtual void OnDisable()
		{
			//if( _PlayerInput != null )
			//{
			//	_PlayerInput.onActionTriggered -= OnInputActionTriggered;
			//}

			if( _CharacterAnimationEventReceiver != null )
			{
				_CharacterAnimationEventReceiver.RemoveListener( this );
			}

		}

		void FixedUpdate()
		{
			UpdateMove();
			UpdateJump();
		}
		protected void UpdateMove()
		{
			var gravityDir = Physics2D.gravity.normalized;

			var velocity = mRigidbody.linearVelocity;
			var force = Vector2.zero;

			mMoveDirection = Vector2.right;
			if( IsGrounded 
			 && mLastHitGround.collider != null 
			){
				var normal = mLastHitGround.normal;
				var groundAngle = Vector2.Angle( normal, -gravityDir );
				if( groundAngle <= _GroundAdsorbAngle )
				{
					mMoveDirection = -Vector2.Perpendicular( normal );
				}
			}

			float moveDirVelocity = Vector2.Dot( mMoveDirection, velocity );
			float moveSpeed = Mathf.Abs( moveDirVelocity );

			if( IsMoving )
			{
				float moveDirSign = 0 < mMoveInput.x ? 1 : -1;
				float moveMax = _moveSpeed * mMoveInput.x;
				float acc = IsGrounded ? _moveAcceleration : _AirMoveAcceleration;
				float spd = moveDirVelocity + acc * moveDirSign;

				if( 0 < moveDirSign )
				{
					if( moveMax < spd )
					{
						acc = 0;
					}
				}
				else
				{
					if( spd < moveMax )
					{
						acc = 0;
					}
					
				}
				force = mMoveDirection * acc * moveDirSign;

			}
			else
			{
				float brakeAcc = IsGrounded ? _BrakeAcceleration : _AirBrakeAcceleration;
				float brakeDir = moveDirVelocity < 0 ? 1: - 1;
				if(  _MoveStopSpeedThreshold < moveSpeed )
				{
					force = mMoveDirection * brakeDir * Mathf.Min( moveSpeed, brakeAcc );
				}
				else if( IsGrounded && !IsJumping)
				{
					mRigidbody.linearVelocity = Vector2.zero;
					
				}
			}

			mRigidbody.AddForce( force, ForceMode2D.Impulse );
		}

		protected void UpdateJump()
		{
			if( !IsJumping ){ return; }
			if( mRigidbody == null ){ return; }
			if( mRigidbody.linearVelocityY <= 0 ){ return; }
			if( !mJumpInput )
			{
				mRigidbody.linearVelocityY -= Mathf.Min( mRigidbody.linearVelocityY,  _JumpSpeed.SafeDivide(_JumpBrakeTime) * Time.fixedDeltaTime );
			}
		}

		/// <summary>
		/// Jump 可能かをチェック
		/// </summary>
		/// <returns></returns>
		public virtual bool CheckCanJump()
		{
			if( _JumpCountLimit <= JumpCount ) { return false; }
			if( !_IgnoreGroundedWhenFirstJump
			 && !IsJumping
			 && !IsGrounded
			){ return false; }

			return true;
		}

		/// <summary>
		/// Jump 実行
		/// </summary>
		/// <returns></returns>
		public virtual bool DoJump()
		{
			if( mRigidbody == null ){ return false; }

			if( !CanJump ) { return false; }

			++JumpCount;
			// Jump 開始時間
			mJumpStartTime = Time.time;

			float addSpeedY = Mathf.Max( 0, _JumpSpeed - mRigidbody.linearVelocityY );
			mRigidbody.linearVelocityY += addSpeedY;
			_Animator.SetTrigger( AnimatorHash_IsJumping );

			return true;
		}

		/// <summary>
		/// Jump 状態リセット
		/// </summary>
		protected void ResetJump()
		{
			JumpCount = 0;
			_Animator.ResetTrigger( AnimatorHash_IsJumping );
		}


		private void Update()
		{
			CheckGround();
			UpdateAnimatorParameter();
		}

		private void UpdateSprite()
		{
			if( _SpriteRenderer == null ){ return; }

			if( IsMoving )
			{
				_SpriteRenderer.flipX = mMoveInput.x < 0;
			}
		}

		private void UpdateAnimatorParameter()
		{
			if( _Animator == null ){ return; }

			var velocity = mRigidbody != null ? mRigidbody.linearVelocity : Vector2.zero;
		
			_Animator.SetBool( AnimatorHash_IsMoving, IsMoving );
		
			_Animator.SetFloat( AnimatorHash_MoveSpeed, Mathf.Abs(velocity.x) );
			_Animator.SetFloat( AnimatorHash_VelocityX, velocity.x );
			_Animator.SetFloat( AnimatorHash_VelocityY, velocity.y );
		

			_Animator.SetBool( AnimatorHash_IsGrounded, IsGrounded );
		

		}

		private void CheckGround()
		{
			bool beforeIsGrounded = IsGrounded;
			IsGrounded = false;

			if( mRigidbody == null ){ return; }


#if false
			IsGrounded = mRigidbody.IsTouching( _CheckGroudedContactFilter );
#else
			// IsTouching() だと小さな起伏でも空中判定されるので cast のほうが有利かも
			if( _BoxCollider == null ){ return; }
			var boxSize = _BoxCollider.size * _BoxCollider.transform.lossyScale;
			var pos = _BoxCollider.transform.TransformPoint( _BoxCollider.offset );
			var gravityDir = Physics2D.gravity.normalized;
			int hitCount = Physics2D.BoxCast( pos, boxSize, 0, gravityDir, _CheckGroudedContactFilter, mRaycastHit2Ds, _GroundCastDistance );
			IsGrounded = 0 < hitCount;

			if( 0 < hitCount )
			{
				mLastHitGround = mRaycastHit2Ds.FirstOrDefault();
			}
#endif

			if( IsGrounded )
			{
				if( IsJumping
				 && mJumpStartTime + GroudedCheckIgnoreTimeAfterJump < Time.time )
				{
					ResetJump();
				};
			}

		}


		/// 
		//private void OnInputActionTriggered(InputAction.CallbackContext context)
		//{
		//		// XXX ここで context.action.name == "Move" とかするのはダサい
		//		// XXX かといって FincAction( "Move" ); をあらかじめやっておくのもなんかしっくりこない
		//		// XXX 結局は UnityEvent でいいんじゃないかという事で onActionTriggered は保留。
		//}

		public void OnMoveInput(InputAction.CallbackContext context)
		{
			Vector2 movementInput = context.ReadValue<Vector2>();
			mMoveInput = movementInput;
		
			if (context.canceled)
			{
				IsMoving = false;
			}
			else
			{
				IsMoving = 0 < Mathf.Abs(mMoveInput.x);
			}

			UpdateSprite();
		}

		public void OnJumpInput(InputAction.CallbackContext context)
		{
			if (context.performed)
			{
				mJumpInput = true;
				//DoJump();
				if( AbilitySystem != null )
				{
					var abilityJump = AbilitySystem.GetAbility<AbilityJump>();
					if( abilityJump != null )
					{
						abilityJump.Execute();
					}
				}
			}
			else if( context.canceled )
			{
				mJumpInput = false;
			}
		}

		public void OnAttackInput(InputAction.CallbackContext context)
		{
			if (context.performed)
			{
				if( AbilitySystem != null )
				{
					var abilityAttack = AbilitySystem.GetAbility<AbilityAttack>();
					if( abilityAttack != null )
					{
						abilityAttack.Execute();
					}
				}
			}

		}

		public void OnBeginAttackHit( AnimationEvent animationEvent )
		{
			Log.Info( "Character.OnBeginAttackHit" );
		}

		public void OnEndAttackHit( AnimationEvent animationEvent )
		{
			Log.Info( "Character.OnEndAttackHit" );
		}

		public void OnStep( AnimationEvent animationEvent )
		{

		}

		public void OnDrawGizmos()
		{
			var before_color = Gizmos.color;

			Gizmos.DrawRay( transform.position, new Vector3( mMoveInput.x, mMoveInput.y, 0  ) );

			Gizmos.color = Color.green;
			Gizmos.DrawRay( transform.position, new Vector3( mMoveDirection.x, mMoveDirection.y, 0  ) );

			Gizmos.color = before_color;
		}

#if UNITY_EDITOR

		[CustomEditor(typeof(CharacterController2D), true)]
		public class CharacterController2DCustomEditor : Editor
		{

			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();

				var chara = target as CharacterController2D;
				if( chara == null ) { return; }

				EditorGUILayout.LabelField( $"IsGrounded : {chara.IsGrounded}");
				EditorGUILayout.LabelField( $"IsJumping : {chara.IsJumping} ({chara.JumpCount})");
				if( chara.mRigidbody != null )
				{
					EditorGUILayout.LabelField( $"Velocity : {chara.mRigidbody.linearVelocity}");
					EditorGUILayout.LabelField( $"Magnitude : {chara.mRigidbody.linearVelocity.magnitude}");
				}
				
			}
		}
#endif // UNITY_EDITOR
		
	}
}
