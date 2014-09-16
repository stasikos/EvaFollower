using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MSD.EvaFollower
{
    class EvaPatrol : IEvaControlType
    {

        public bool AllowRunning { get; set; }
        public List<PatrolAction> actions = new List<PatrolAction>();
        public int currentPatrolPoint = 0;

        public bool CheckDistance(double sqrDistance)
        {
            bool complete = (sqrDistance < 0.3);

            if (complete)
            {
                ++currentPatrolPoint;

                if (currentPatrolPoint >= actions.Count)
                    currentPatrolPoint = 0;
            }

            return complete;
        }

        public void GetNextTarget(ref Vector3d move)
        {
            PatrolAction currentPoint = actions[currentPatrolPoint];

            if (currentPoint.type == PatrolActionType.Move)
                move += currentPoint.position;
        }

        public void Move(Vector3d position)
        {
            actions.Add(new PatrolAction(PatrolActionType.Move, 0, position));
        }

        public void Clear()
        {
            currentPatrolPoint = 0;
            actions.Clear();
        }
    }

    public class PatrolAction
    {
        public Vector3d position;
        public PatrolActionType type;
        public int delay = 0;

        public PatrolAction(PatrolActionType type, int delay, Vector3d position)
        {
            this.type = type;
            this.delay = delay;
            this.position = position;
        }

        public override string ToString()
        {
            return "position = " + position.ToString() + ", delay = " + delay + ", type = " + type.ToString();
        }
    }

    public enum PatrolActionType
    {
        Move,
        Wait,
    }
    
}
