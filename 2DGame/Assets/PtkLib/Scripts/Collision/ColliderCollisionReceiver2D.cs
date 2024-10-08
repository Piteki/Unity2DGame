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
	/// isTrigger ではない Collider 群の親に配置する事ですべての Collision メッセージを受領して Event を送出する
	/// </remarks>
	public class ColliderCollisionReceiver2D : MonoBehaviour
	{
		public event Action< Collision2D > EventCollisionEnter2D;
		public event Action< Collision2D > EventCollisionExit2D;
		public event Action< Collision2D > EventCollisionStay2D;

		private void OnDestroy()
		{
			EventCollisionEnter2D = null;
			EventCollisionExit2D = null;
			EventCollisionStay2D = null;
		}
		private void OnCollisionEnter2D( Collision2D collision )
		{
			if( collision == null ){ return; }
			EventCollisionEnter2D?.Invoke( collision );
		}
		private void OnCollisionExit2D( Collision2D collision )
		{
			if( collision == null ){ return; }
			EventCollisionExit2D?.Invoke( collision );
		}
		private void OnCollisionStay2D( Collision2D collision )
		{
			if( collision == null ){ return; }
			EventCollisionStay2D?.Invoke( collision );
		}

	}
	


}
