using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ptk
{

	/// <summary>
	/// Animation Clip の Event を受信する Receiver
	/// </summary>
	public class CharacterAnimationEventReceiver : MonoBehaviour
	{
		public event Action<AnimationEvent> EventStep;

		public readonly HashSet<ICharacterAnimationEventListener> mListeners = new();

		private void OnDestroy()
		{
			EventStep = null;
			mListeners.Clear();
		}

		/// <summary>
		/// Listener 登録
		/// </summary>
		public bool AddListener( ICharacterAnimationEventListener listener )
		{
			if( listener == null ){ return false; }
			return mListeners.Add( listener );
		}

		/// <summary>
		/// Listener 解除
		/// </summary>
		public bool RemoveListener( ICharacterAnimationEventListener listener )
		{
			return mListeners.Remove( listener );
		}

	

		private void InvokeListenerMethod( Action< ICharacterAnimationEventListener > action )
		{
			foreach( var listener in mListeners )
			{
				if( listener == null ){ continue; }
				action?.Invoke( listener );
			}
		}

		private void OnStep( AnimationEvent animationEvent )
		{
			InvokeListenerMethod( listener => listener.OnStep( animationEvent ) );
			EventStep?.Invoke( animationEvent );
		}
	}

	/// <summary>
	/// Animation Clip の Event を傍受する Listener インターフェイス
	/// </summary>
	public interface ICharacterAnimationEventListener
	{
		public void OnStep( AnimationEvent animationEvent );

	}
}