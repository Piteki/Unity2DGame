using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ptk
{

	/// <summary>
	/// Attack 関係の Animation Clip の Event を受信する Receiver
	/// </summary>
	public class AttackAnimationEventReceiver : MonoBehaviour
	{
		public event Action<AnimationEvent> EventBeginAttackHit;
		public event Action<AnimationEvent> EventEndAttackHit;

		public readonly HashSet<IAttackAnimationEventListener> mListeners = new();
		public readonly List<IAttackAnimationEventListener> mTmpListeners = new();

		private void OnDestroy()
		{
			EventBeginAttackHit = null;
			EventEndAttackHit = null;
			mListeners.Clear();
		}

		/// <summary>
		/// Listener 登録
		/// </summary>
		public bool AddListener( IAttackAnimationEventListener listener )
		{
			if( listener == null ){ return false; }
			return mListeners.Add( listener );
		}

		/// <summary>
		/// Listener 解除
		/// </summary>
		public bool RemoveListener( IAttackAnimationEventListener listener )
		{
			return mListeners.Remove( listener );
		}

	

		private void InvokeListenerMethod( Action< IAttackAnimationEventListener > action )
		{
			var listeners = mListeners;
			mTmpListeners.Clear();
			if( mTmpListeners.Capacity < listeners.Count ){ mTmpListeners.Capacity = listeners.Count; };
			foreach( var listener in mListeners )
			{
				mTmpListeners.Add( listener );
			}
			foreach( var listener in mTmpListeners )
			{
				if( listener == null ){ continue; }
				action?.Invoke( listener );
			}
			mTmpListeners.Clear();
		}

		private void OnBeginAttackHit( AnimationEvent animationEvent )
		{
			InvokeListenerMethod( listener => listener.OnBeginAttackHit( animationEvent ) );
			EventBeginAttackHit?.Invoke( animationEvent );
		}

		private void OnEndAttackHit( AnimationEvent animationEvent )
		{
			InvokeListenerMethod( listener => listener.OnEndAttackHit( animationEvent ) );
			EventEndAttackHit?.Invoke( animationEvent );
		}

	}

	/// <summary>
	/// Attack 関係の Animation Clip の Event を傍受する Listener インターフェイス
	/// </summary>
	public interface IAttackAnimationEventListener
	{
		public void OnBeginAttackHit( AnimationEvent animationEvent );

		public void OnEndAttackHit( AnimationEvent animationEvent );

	}
}