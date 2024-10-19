
using Ptk.IdStrings;

[assembly: IdStringDefine( "assemblyType.asmParent.asmChild" ) ]
[assembly: IdStringDefine( "assemblyType.asmParent.asmChild.asmGrandchild", "asmGrandchild の Description." ) ]

namespace Ptk.AppTag
{

	[IdStringDefineMember(Description ="Ability 関連タグ", Order = -2)]
	public static class AbilityTag
	{
		[IdStringDefineMember(Description ="死亡状態")]
		public static IdString Dead { get; private set; }

		[IdStringDefineMember( Description = "攻撃タグ", Order = -1, HideInViewer = false ) ]	
		public static class Attack
		{
			[IdStringDefineMember] public static IdString Normal { get; private set; }
			[IdStringDefineMember] public static IdString DashAttack { get; private set; }
		}

		[IdStringDefineMember(Description ="生存状態")]
		public static IdString Arrive { get; private set; }

	}

	[IdStringDefineMember(Description ="Status 関連タグ")]
	public static class StatusTag 
	{
		[IdStringDefineMember] public static IdString Poison { get; private set; }

		[IdStringDefineMember( HideInViewer = true )] 
		public static IdString SecretStatus { get; private set; }

		[IdStringDefineMember] public static IdString Stun { get; private set; }
	}

	[IdStringDefineMember(Description ="Battle タグ")]
	public static class BattleTag 
	{
		[IdStringDefineMember] public static IdString Battle { get; private set; }
		[IdStringDefineMember] public static IdString Field { get; private set; }
	}

	[IdStringDefineMember(Description ="HideInViewer タグ", HideInViewer = true )] 
	public static class Hiding 
	{
		[IdStringDefineMember] public static IdString HidingA { get; private set; }
		[IdStringDefineMember] public static IdString HidingB { get; private set; }
	}


}