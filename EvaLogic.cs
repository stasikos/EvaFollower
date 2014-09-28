using UnityEngine;
using System.Diagnostics;
using System;

namespace MSD.EvaFollower
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class EvaLogic : MonoBehaviour
    {
        public void Start()
        {
            EvaDebug.DebugWarning("EvaLogic.Start()");
        }
        public void OnDestroy()
        {
            //EvaDebug.DebugWarning("EvaLogic.OnDestroy()");
        }

        public void Update()
        {
            if (!FlightGlobals.ready || PauseMenu.isOpen)
                return;

            try
            {
                foreach (EvaContainer eva in EvaController.fetch.collection.ToArray())
                {
                    if (eva == null)
                    {
                        //is this possible ?
                        EvaDebug.DebugWarning("eva == null");
                        continue;
                    }

                    //skip unloaded vessels
                    if (!eva.Loaded)
                    {
                        continue;
                    }

                    //Turn the lights on when dark.     
                    //Skip for now, too buggy..
                    //eva.UpdateLamps();
                   
                    if (eva.mode == Mode.None)
                    {
                        //Nothing to do here.
                        continue;
                    }

                    //Recover from ragdoll, if possible.    
                    if (eva.IsRagDoll)
                    {
                        eva.RecoverFromRagdoll();
                        continue;
                    }

                    Vector3d move = -eva.Position;
                    
                    //Get next Action, Formation or Patrol
                    Vector3d target = eva.GetNextTarget();

                    // Path Finding
                    //todo: check if the target is occopied.
                    move += target;

                    double sqrDist = move.sqrMagnitude;
                    float speed = TimeWarp.deltaTime;

                    eva.AILogic(sqrDist);

                    //Break Free Code
                    if (eva.OnALadder)
                    {
                        eva.ReleaseLadder();
                    }

                    if (eva.IsActive && eva.mode == Mode.Order)
                    {
                        Mode mode = eva.mode;

                        if (Input.GetKeyUp(KeyCode.W))
                            mode = EvaFollower.Mode.None;
                        if (Input.GetKeyUp(KeyCode.S))
                            mode = EvaFollower.Mode.None;
                        if (Input.GetKeyUp(KeyCode.A))
                            mode = EvaFollower.Mode.None;
                        if (Input.GetKeyUp(KeyCode.D))
                            mode = EvaFollower.Mode.None;
                        if (Input.GetKeyUp(KeyCode.Q))
                            mode = EvaFollower.Mode.None;
                        if (Input.GetKeyUp(KeyCode.E))
                            mode = EvaFollower.Mode.None;

                        if (eva.mode == Mode.None)
                            continue;
                    }

                    //Animation Logic
                    eva.UpdateAnimations(sqrDist, ref speed);

                    move.Normalize();

                    //Distance Logic
                    eva.CheckDistance(move, speed, sqrDist);

                    //Reset Animation Mode Events 
                    eva.CheckModeIsNone();

                }
            }
            catch (Exception exp)
            {
                EvaDebug.DebugWarning("[EFX] EvaLogic: " + exp.Message + ":" + exp.ToString());
            }
        }
    }
}
