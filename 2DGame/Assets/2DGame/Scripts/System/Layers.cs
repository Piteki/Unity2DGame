
using UnityEngine;

namespace Ptk
{
	static public class Layers
	{
		static public readonly string LayerName_Ground = "Ground";

		static public readonly int LayerMask_Ground = LayerMask.GetMask( LayerName_Ground );
	}
}