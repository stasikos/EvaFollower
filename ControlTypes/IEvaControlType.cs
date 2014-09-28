using System;
using System.Collections.Generic;
using System.Text;

namespace MSD.EvaFollower
{
    interface IEvaControlType
    {
        /// <summary>
        /// Check if the criteria is met, 
        /// </summary>
        /// <param name="sqrDistance">The distance between position and target</param>
        /// <returns>Returns true if the criteria is met.</returns>
        bool CheckDistance(double sqrDistance);

        /// <summary>
        /// Get the next target to move on. 
        /// </summary>
        /// <param name="move"></param>
        Vector3d GetNextTarget();
    }
}
