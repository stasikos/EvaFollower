using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSD.EvaFollower
{
    /// <summary>
    /// The object responsible for Formations.
    /// </summary>
    class EvaFormation : IEvaControlType
    {
        private EvaContainer leader;

        public EvaContainer Leader
        {
            get { return leader; }
            set { leader = value; }
        }

        /// <summary>
        /// Get the next position to walk to. 
        /// Formation should handle differents positions.
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        public void GetNextTarget(ref Vector3d move)
        {
           
            if (leader == null)
            {
                //EvaDebug.DebugWarning("GetNextTarget(). leader == null");
                return;
            }
            
            //get the leader. 
            var target = leader.EVA.rigidbody.position;            

            //I should figure out how to give every follower an id, so I can give a 
            //position to them. 

            //update move vector.
            move += target;
        }

    

        /// <summary>
        /// Check if the distance to the target is reached.
        /// </summary>
        /// <param name="sqrDistance"></param>
        /// <returns></returns>
        public bool CheckDistance(double sqrDistance)
        {

            if (sqrDistance < 3.0)
            {
                return true;
            }

            return false;
        }

    }
}
