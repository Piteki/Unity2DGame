
using Ptk.IdStrings;

namespace Ptk.AppTag
{
	[IdString(Description ="Ability �֘A�^�O")]
	public static class AbilityTag
	{
		[IdString(Description ="���S���")]
		public static IdString Dead { get; private set; }

		[IdString( Description = "�U���^�O" )]	
		public static class Attack
		{
			[IdString] public static IdString Normal { get; private set; }
			[IdString] public static IdString DashAttack { get; private set; }
		}


	}

	[IdString(Description ="Status �֘A�^�O")]
	public static class StatusTag 
	{
		[IdString] public static IdString Poison { get; private set; }
		[IdString] public static IdString Stun { get; private set; }
	}

}