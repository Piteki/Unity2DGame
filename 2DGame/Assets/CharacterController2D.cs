using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

	[SerializeField] float _moveSpeed = 2;
	[SerializeField] float _moveAcceleration = 1.0f;
	[SerializeField] float _AirMoveAcceleration = 0.2f;
	[SerializeField] float _JumpSpeed = 2;

	[SerializeField] BoxCollider2D _BoxCollider;
	[SerializeField] SpriteRenderer _SpriteRenderer;
	[SerializeField] Animator _Animator;


	[SerializeField] bool _isGrounded = false;

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

		var velocity = mRigidbody.velocity;
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
			//force.x = -velocity.x * 0.1f * fixedDeltaDiv;
		}

		if( mJumpInput )
		{
			if( IsGrounded )
			{
				force.y = _JumpSpeed * fixedDeltaDiv;
				IsJumping = true;
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

		var velocity = mRigidbody.velocity;
		
		_Animator.SetBool( AnimatorHash_IsMoving, IsMoving );
		_Animator.SetBool( AnimatorHash_IsJumping, IsJumping );
		if( IsJumping )
		{
			_Animator.SetTrigger( AnimatorHash_IsJumping );
			IsJumping = false;
		}
		else
		{
			_Animator.ResetTrigger( AnimatorHash_IsJumping );
		}
		
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

		var boxSize = _BoxCollider != null ? _BoxCollider.size : new Vector2( 1, 0.1f );
		boxSize.y = 0.05f;
		var downDir = Vector2.down;
		float distance = 0.1f;
		int hitCount = Physics2D.BoxCastNonAlloc( transform.position, boxSize, 0, downDir, mRaycastHit2Ds, distance, LayerIndex_Ground );
		
		//for( int i = 0; i < hitCount; ++i )
		//{
		//	mRaycastHit2Ds[ i ].
		//}
		IsGrounded = 0 < hitCount;
		if( IsGrounded )
		{
			IsJumping = false;
		}
		_isGrounded = IsGrounded;
	}

	public void OnMoveInput(InputAction.CallbackContext context)
	{
		Vector2 movementInput = context.ReadValue<Vector2>();
		mMoveInput = movementInput;
		

		if (context.started)
		{
			IsMoving = true;
		}
		else if (context.canceled)
		{
			IsMoving = false;
		}

		UpdateSprite();
	}

	public void OnJumpInput(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			mJumpInput = true;
		}
		else if (context.canceled)
		{
			mJumpInput = false;
		}

	}


	public void OnDrawGizmos()
	{
		Gizmos.DrawRay( transform.position, new Vector3( mMoveInput.x, mMoveInput.y, 0  ) );
	}
}
