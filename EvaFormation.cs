using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSD.EvaFollower
{
    class EvaFormation : IEvaControlType
    {
        private EvaContainer leader;
        private FormationType type = FormationType.None;

        public EvaContainer Leader
        {
            get { return leader; }
            set { leader = value; }
        }

        public FormationType FormationType
        {
            get { return type; }
            set { type = value; }
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
                //is follower but no leader?
                EvaDebug.DebugWarning("GetNextTarget(). leader == null");
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
