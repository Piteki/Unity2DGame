using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ptk
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class CharacterController2D : MonoBehaviour
	{
		static public string LayerName_Ground = "Ground";

		static public int AnimatorHash_IsMoving = Animator.StringToHash("IsMoving");
		static public int AnimatorHash_IsJumping = Animator.StringToHash("IsJumping");
		static public int AnimatorHash_MoveSpeed = Animator.StringToHash("MoveSpeed");
		static public int AnimatorHash_VelocityX = Animator.StringToHash("VelocityX");
		static public int AnimatorHash_VelocityY = Animator.StringToHash("VelocityY");
		static public int AnimatorHash_IsGrounded = Animator.StringToHash("IsGrounded");

		[SerializeField] float _moveSpeed = 5;
		[SerializeField] float _moveAcceleration = 1.0f;
		[SerializeField] float _AirMoveAcceleration = 0.2f;
		[SerializeField] float _JumpSpeed = 2;
		[SerializeField] float _GroundCastDistance = 0.05f;
		[SerializeField] float _BrakeAcceleration = 0.5f;
		[SerializeField] float _AirBrakeAcceleration = 0.1f;
		[SerializeField] float _MoveStopSpeedThreshold = 0.01f;

		[SerializeField] BoxCollider2D _BoxCollider;
		[SerializeField] SpriteRenderer _SpriteRenderer;
		[SerializeField] Animator _Animator;


		private Vector2 mMoveInput;
		private bool mJumpInput;
		private Rigidbody2D mRigidbody;
		private RaycastHit2D[] mRaycastHit2Ds = new RaycastHit2D[10];

		public bool IsMoving { get; private set; }
		public bool IsJumping { get; private set; }
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
		
		}

		void FixedUpdate()
		{
			float fixedDeltaDiv = 1 / Time.fixedDeltaTime;

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
				force.x = addSpd * fixedDeltaDiv;
			}
			else
			{
				float brakeAcc = IsGrounded ? _BrakeAcceleration : _AirBrakeAcceleration;
				float brakeDir = velocity.x < 0 ? 1: - 1;
				float speed = Mathf.Abs(velocity.x);
				if(  _MoveStopSpeedThreshold < speed )
				{
					force.x = brakeDir * Mathf.Min( speed, brakeAcc ) * fixedDeltaDiv;
				}
				else
				{
					mRigidbody.linearVelocityX = 0;
				}
			}

			if( mJumpInput )
			{
				if( IsGrounded 
				 && !IsJumping
				){
					mJumpInput = false;

					force.y = _JumpSpeed * fixedDeltaDiv;
					IsJumping = true;
					_Animator.SetTrigger( AnimatorHash_IsJumping );
				}
			}

			mRigidbody.AddForce( force );

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

			var velocity = mRigidbody.linearVelocity;
		
			_Animator.SetBool( AnimatorHash_IsMoving, IsMoving );
		
			_Animator.SetFloat( AnimatorHash_MoveSpeed, Mathf.Abs(velocity.x) );
			_Animator.SetFloat( AnimatorHash_VelocityX, velocity.x );
			_Animator.SetFloat( AnimatorHash_VelocityY, velocity.y );
		

			_Animator.SetBool( AnimatorHash_IsGrounded, IsGrounded );
		

		}

		private void CheckGround()
		{
			IsGrounded = false;

			// TODO
			int LayerIndex_Ground = LayerMask.GetMask( LayerName_Ground );

			var downDir = Vector2.down;
			var rect = GetCheckGroundBox();
			int hitCount = Physics2D.BoxCastNonAlloc( rect.position, rect.size, 0, downDir, mRaycastHit2Ds, _GroundCastDistance, LayerIndex_Ground );
		
			//for( int i = 0; i < hitCount; ++i )
			//{
			//	mRaycastHit2Ds[ i ].
			//}
			IsGrounded = 0 < hitCount;
			if( IsGrounded )
			{
				IsJumping = false;
				_Animator.ResetTrigger( AnimatorHash_IsJumping );
			}
		}

		public Rect GetCheckGroundBox()
		{
			if( _BoxCollider == null )
			{
				var pos = transform.position;
				return new Rect( new Vector2( pos.x, pos.y ) + _BoxCollider.offset, Vector2.one );
			}
			else
			{
				var boxSize = _BoxCollider.size * _BoxCollider.transform.lossyScale;
				var pos = _BoxCollider.transform.TransformPoint( _BoxCollider.offset );
				return new Rect( new Vector2( pos.x, pos.y ), boxSize );
			}
		}

		public void OnMoveInput(InputAction.CallbackContext context)
		{
			Vector2 movementInput = context.ReadValue<Vector2>();
			mMoveInput = movementInput;
		
			IsMoving = 0 < Mathf.Abs(mMoveInput.x);

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
				if( !IsJumping )
				{
					mJumpInput = true;
				}
			}
			else if (context.canceled)
			{
				mJumpInput = false;
			}

		}


		public void OnDrawGizmos()
		{
			Gizmos.DrawRay( transform.position, new Vector3( mMoveInput.x, mMoveInput.y, 0  ) );

			var rect = GetCheckGroundBox();
			Gizmos.DrawWireCube( new Vector3( rect.position.x, rect.position.y, transform.position.z ), new Vector3( rect.size.x, rect.size.y, 0.01f ) );
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
				
			}
		}
#endif // UNITY_EDITOR
		
	}
}
