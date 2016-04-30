using System;

namespace MSD.EvaFollower
{
	public interface IDetection
	{
		bool Evaluate (Vector3d position);
		void Debug();
	}
}

