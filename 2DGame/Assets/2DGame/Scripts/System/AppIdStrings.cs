
using Ptk.IdStrings;
using UnityEngine;

namespace Ptk.AppTag
{
	[IdString(Description ="Ability 関連タグ")]
	public static class AbilityTag
	{
		[IdString(Description ="死亡状態")]
		public static IdString Dead { get; private set; }

		[IdString( Description = "攻撃タグ" ) ]	
		public static class Attack
		{
			[IdString] public static IdString Normal { get; private set; }
			[IdString] public static IdString DashAttack { get; private set; }
		}


	}

	[IdString(Description ="Status 関連タグ")]
	public static class StatusTag 
	{
		[IdString] public static IdString Poison { get; private set; }

		[IdString] 
		public static IdString SecretStatus { get; private set; }

		[IdString] public static IdString Stun { get; private set; }
	}

	[IdString(Description ="Battle タグ")]
	public static class BattleTag 
	{
		[IdString] public static IdString Battle { get; private set; }
		[IdString] public static IdString Field { get; private set; }
	}


}