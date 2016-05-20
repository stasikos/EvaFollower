using System;
using System.Collections.Generic;

namespace MSD.EvaFollower
{
	interface IDetection
	{
		void UpdateMap (List<EvaContainer> collection);
		void Debug();
	}
}

