using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Ptk
{
	/// <summary>
	/// Collider の Collision イベント送出クラス
	/// </summary>
	/// <remarks>
	/// isTrigger の Collider 群の親に配置する事ですべての Trigger メッセージを受領して Event を送出する
	/// </remarks>
	public class ColliderTriggerReceiver2D : MonoBehaviour
	{
		public event Action< Collider2D > EventTriggerEnter2D;
		public event Action< Collider2D > EventTriggerExit2D;
		public event Action< Collider2D > EventTriggerStay2D;

		private void OnDestroy()
		{
			EventTriggerEnter2D = null;
			EventTriggerExit2D = null;
			EventTriggerStay2D = null;
		}

		private void OnTriggerEnter2D( Collider2D collider )
		{
			if( collider == null ){ return; }

			EventTriggerEnter2D?.Invoke( collider );
		}

		private void OnTriggerExit2D( Collider2D collider )
		{
			if( collider == null ){ return; }
			EventTriggerExit2D?.Invoke( collider );
		}

		private void OnTriggerStay2D( Collider2D collider )
		{
			if( collider == null ){ return; }
			EventTriggerStay2D?.Invoke( collider );
		}

	}
	


}
