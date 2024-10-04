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
	[RequireComponent(typeof(Rigidbody2D))]
	public class CharacterController2D : MonoBehaviour
	{
		static public readonly int AnimatorHash_IsMoving = Animator.StringToHash("IsMoving");
		static public readonly int AnimatorHash_IsJumping = Animator.StringToHash("IsJumping");
		static public readonly int AnimatorHash_MoveSpeed = Animator.StringToHash("MoveSpeed");
		static public readonly int AnimatorHash_VelocityX = Animator.StringToHash("VelocityX");
		static public readonly int AnimatorHash_VelocityY = Animator.StringToHash("VelocityY");
		static public readonly int AnimatorHash_IsGrounded = Animator.StringToHash("IsGrounded");

		static public readonly float GroudedCheckIgnoreTimeAfterJump = 0.1f;

		[SerializeField] BoxCollider2D _BoxCollider;
		[SerializeField] SpriteRenderer _SpriteRenderer;
		[SerializeField] Animator _Animator;
		[SerializeField] AbilitySystem _AbilitySystem;

		[SerializeField] float _moveSpeed = 5;
		[SerializeField] float _moveAcceleration = 1.0f;
		[SerializeField] float _AirMoveAcceleration = 0.2f;
		[SerializeField] float _JumpSpeed = 5;
		[SerializeField] int _JumpCountLimit = 1;
		[SerializeField] bool _IgnoreGroundedWhenFirstJump = false;
		[SerializeField] float _BrakeAcceleration = 0.5f;
		[SerializeField] float _AirBrakeAcceleration = 0.1f;
		[SerializeField] float _MoveStopSpeedThreshold = 0.01f;

		[SerializeField] ContactFilter2D _CheckGroudedContactFilter = new()
		{
			useNormalAngle = true,
			minNormalAngle =  44.0f,
			maxNormalAngle = 136.0f,
		};


		private Vector2 mMoveInput;

		private Rigidbody2D mRigidbody;
		private RaycastHit2D[] mRaycastHit2Ds = new RaycastHit2D[10];

		private float mJumpStartTime;

		public AbilitySystem  AbilitySystem => _AbilitySystem;
		public bool IsMoving { get; private set; }
		public bool CanJump => CheckCanJump();
		public bool IsJumping => 0 < JumpCount;
		public int JumpCount { get; private set; }
		public bool IsGrounded { get; private set; }

		void Start()
		{
			mRigidbody = GetComponent<Rigidbody2D>();
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
			if( _AbilitySystem == null )
			{
				_AbilitySystem = GetComponent<AbilitySystem>();
			}
			
		
		}

		void FixedUpdate()
		{
			UpdateMove();
		}

		protected void UpdateMove()
		{
			//float fixedDeltaDiv = 1 / Time.fixedDeltaTime;

			var velocity = mRigidbody.linearVelocity;
			var force = Vector2.zero;
			if( IsMoving )
			{
				//float moveFactor = (_moveSpeed * Mathf.Abs( mMoveInput.x ) - Mathf.Abs( velocity.x ) ) * fixedDeltaDiv;
				//forceX = mMoveInput.x * moveFactor;
				float moveDir = mMoveInput.x < 0 ? - 1: 1;

				float acc = IsGrounded ? _moveAcceleration : _AirMoveAcceleration;
				float addSpd = acc * moveDir;

			

				float maxSpeed = Mathf.Abs( _moveSpeed * mMoveInput.x );
				if( maxSpeed < Mathf.Abs( addSpd + velocity.x ) )
				{
					addSpd = 0;
				}
				force.x = addSpd;// * fixedDeltaDiv;
			}
			else
			{
				float brakeAcc = IsGrounded ? _BrakeAcceleration : _AirBrakeAcceleration;
				float brakeDir = velocity.x < 0 ? 1: - 1;
				float speed = Mathf.Abs(velocity.x);
				if(  _MoveStopSpeedThreshold < speed )
				{
					force.x = brakeDir * Mathf.Min( speed, brakeAcc );// * fixedDeltaDiv;
				}
				else
				{
					mRigidbody.linearVelocityX = 0;
				}
			}

			mRigidbody.AddForce( force, ForceMode2D.Impulse );
		}


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

			IsGrounded = mRigidbody.IsTouching( _CheckGroudedContactFilter );

			if( IsGrounded )
			{
				if( IsJumping
				 && mJumpStartTime + GroudedCheckIgnoreTimeAfterJump < Time.time )
				{
					ResetJump();
				};
			}
		}

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
		}


		public void OnDrawGizmos()
		{
			Gizmos.DrawRay( transform.position, new Vector3( mMoveInput.x, mMoveInput.y, 0  ) );
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
					EditorGUILayout.LabelField( $"VelocityX : {chara.mRigidbody.linearVelocityX}");
					EditorGUILayout.LabelField( $"VelocityY : {chara.mRigidbody.linearVelocityY}");
				}
				
			}
		}
#endif // UNITY_EDITOR
		
	}
}
