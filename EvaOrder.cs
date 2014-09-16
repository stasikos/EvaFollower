using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MSD.EvaFollower
{
    class EvaOrder : IEvaControlType
    {
        public bool AllowRunning { get; set; }
        private Vector3 position;

        public EvaOrder()
        {
            AllowRunning = true;
        }

        public bool CheckDistance(double sqrDistance)
        {
            bool complete = (sqrDistance < 1.6);
            
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
