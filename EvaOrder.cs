using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MSD.EvaFollower
{
    /// <summary>
    /// The object responsible for ordering the kerbal around. 
    /// </summary>
    class EvaOrder : IEvaControlType
    {
        public bool AllowRunning { get; set; }
        private Vector3d position;

        public EvaOrder()
        {
            AllowRunning = true;
        }

        public bool CheckDistance(double sqrDistance)
        {
            bool complete = (sqrDistance < 0.5);
            
            return complete;
        }

        public void GetNextTarget(ref Vector3d move)
        {
            move += position;
        }

        public void Move(Vector3d position)
        {
            this.position = position;
        }

    }
}
