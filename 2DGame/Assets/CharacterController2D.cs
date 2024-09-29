using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 2;
    [SerializeField] float _moveAcceleration = 1.0f;

    private Vector2 mMoveInput;
    private bool mIsMoving;
    private Rigidbody2D mRigidbody;
    private Animator mAnimator;

    void Start()
    {
        mRigidbody = GetComponent<Rigidbody2D>();
        mAnimator = GetComponent<Animator>();
        
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

	public void OnDrawGizmos()
	{
		Gizmos.DrawRay( transform.position, new Vector3( mMoveInput.x, mMoveInput.y, 0  ) );
	}
}
