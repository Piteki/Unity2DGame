using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController2D : MonoBehaviour
{
	static public string LayerName_Ground = "Ground";

	[SerializeField] float _moveSpeed = 2;
	[SerializeField] float _moveAcceleration = 1.0f;

	[SerializeField] bool _isGrounded = false;

	private Vector2 mMoveInput;
	private bool mIsMoving;
	private Rigidbody2D mRigidbody;
	private BoxCollider2D mBoxCollider;
	private Animator mAnimator;
	private RaycastHit2D[] mRaycastHit2Ds = new RaycastHit2D[10];

	public bool IsGrounded { get; private set; }

	void Start()
	{
		mRigidbody = GetComponent<Rigidbody2D>();
		mBoxCollider = GetComponentInChildren<BoxCollider2D>();
		mAnimator = GetComponentInChildren<Animator>();
		
	}

	void FixedUpdate()
	{
		float fixedDeltaDiv = 1 / Time.fixedDeltaTime;

		var velocity = mRigidbody.velocity;
		float forceX = 0;
		if( mIsMoving )
		{
			//float moveFactor = (_moveSpeed * Mathf.Abs( mMoveInput.x ) - Mathf.Abs( velocity.x ) ) * fixedDeltaDiv;
			//forceX = mMoveInput.x * moveFactor;
			float moveDir = mMoveInput.x < 0 ? - 1: 1;
			float addSpd = _moveAcceleration * moveDir;
			float maxSpeed = Mathf.Abs( _moveSpeed * mMoveInput.x );
			if( maxSpeed < Mathf.Abs( addSpd + velocity.x ) )
			{
				addSpd = 0;
			}
			forceX = addSpd * fixedDeltaDiv;
		}
		else
		{
			//forceX = -velocity.x * 0.1f * fixedDeltaDiv;
		}
		mRigidbody.AddForce( new Vector2( forceX, 0 ) );

	}

	private void Update()
	{
		CheckGround();
	}


	public void OnMove(InputAction.CallbackContext context)
	{
		Vector2 movementInput = context.ReadValue<Vector2>();
		mMoveInput = movementInput;
		

		if (context.started)
		{
			mIsMoving = true;
		}
		else if (context.canceled)
		{
			mIsMoving = false;
		}
	}

	private void CheckGround()
	{
		IsGrounded = false;

		// TODO
		int LayerIndex_Ground = LayerMask.GetMask( LayerName_Ground );

		var boxSize = mBoxCollider != null ? mBoxCollider.size : new Vector2( 1, 0.1f );
		boxSize.y = 0.05f;
		var downDir = Vector2.down;
		float distance = 0.1f;
		int hitCount = Physics2D.BoxCastNonAlloc( transform.position, boxSize, 0, downDir, mRaycastHit2Ds, distance, LayerIndex_Ground );
		
		//for( int i = 0; i < hitCount; ++i )
		//{
		//	mRaycastHit2Ds[ i ].
		//}
		IsGrounded = 0 < hitCount;
		_isGrounded = IsGrounded;
	}

	public void OnDrawGizmos()
	{
		Gizmos.DrawRay( transform.position, new Vector3( mMoveInput.x, mMoveInput.y, 0  ) );
	}
}
