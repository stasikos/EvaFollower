using System;
using UnityEngine;
using System.Collections.Generic;

namespace MSD.EvaFollower
{
    class EvaContainer
    {
        public Guid flightID; 
        public Mode mode = Mode.None;
        public Status status = Status.None;

        private bool selected = false;
        private bool loaded = false;
        private bool showHelmet = true;
        
        private KerbalEVA eva;

        internal EvaFormation formation = new EvaFormation();
        internal EvaPatrol patrol = new EvaPatrol();
        internal EvaOrder order = new EvaOrder();
           
        public bool IsActive
        {
            get { return (eva.vessel == FlightGlobals.ActiveVessel); }
        }

        public bool IsRagDoll
        {
            get { return eva.isRagdoll; }
        }

        public bool AllowPatrol
        {
            get { return (patrol.actions.Count >= 1);  }
        }

        public bool AllowRunning
        {
            get {

                if (mode == Mode.Patrol)
                {
                    return patrol.AllowRunning;
                }
                else if (mode == Mode.Order)
                {
                    return order.AllowRunning;
                }

                return false;
            }
        }

        public bool CanTakeHelmetOff
        {
            get { return FlightGlobals.ActiveVessel.mainBody.bodyName == "Kerbin"; }
        }

        public KerbalEVA EVA
        {
            get { return eva; }
        }

        public bool Selected
        {
            get { return selected; }
            set { selected = value; }
        }
                
        /// <summary>
        /// Get the world position of the kerbal.
        /// </summary>
        public Vector3d Position
        {
            get { return eva.vessel.GetWorldPos3D(); }
        }

        public bool Loaded
        {
            get {
                bool isLoaded = loaded;

                if (loaded)
                    isLoaded |= eva.isEnabled;

                return isLoaded;
            }
        }

        public string Name { get; set; }

        public bool OnALadder { get { return eva.OnALadder; } }


        public EvaContainer(Guid flightID)
        {
            this.flightID = flightID;
            this.loaded = false;
        }

        public void Load(KerbalEVA eva)
        {
            //Load KerbalEVA.
            this.eva = eva;
            loaded = true;

            //Set Name
            this.Name = eva.name;

            //module on last.
            EvaModule module = (EvaModule)eva.GetComponent(typeof(EvaModule));
            module.Load(this);

            EvaDebug.DebugWarning("EvaContainer.Load("+eva.name+")");
        }

        public void Unload()
        {
            EvaDebug.DebugWarning("EvaContainer.Unload(" + eva.name + ")");
            loaded = false;
        }

        internal string ToSave()
        {
            return (flightID + "," + Name + "," + mode + "," + status + "," + selected + "," + showHelmet + ","
                + this.formation.ToSave() + ","
                + this.patrol.ToSave() + ","
                + this.order.ToSave());
        }

        internal void FromSave(string evaSettings)
        {
            EvaTokenReader reader = new EvaTokenReader(evaSettings);

            try
            {
                string sflightID = reader.NextTokenEnd(',');
                string sName = reader.NextTokenEnd(','); 
                string mode = reader.NextTokenEnd(',');
                string status = reader.NextTokenEnd(',');
                string selected = reader.NextTokenEnd(',');
                string showHelmet = reader.NextTokenEnd(',');

                string formation = reader.NextToken('(', ')'); reader.Consume();
                string patrol = reader.NextToken('(', ')'); reader.Consume();
                string order = reader.NextToken('(', ')');

                this.Name = sName;
                this.mode = (Mode)Enum.Parse(typeof(Mode), mode);
                this.status = (Status)Enum.Parse(typeof(Status), status);
                this.selected = bool.Parse(selected);
                this.showHelmet = bool.Parse(showHelmet);
                
                           
                this.formation.FromSave(formation);
                this.patrol.FromSave(patrol);
                this.order.FromSave(order);

                EvaDebug.DebugLog("Loaded: " + mode);
                EvaDebug.DebugLog("name: " + sName);
                EvaDebug.DebugLog("status: " + status);
                EvaDebug.DebugLog("selected: " + selected);

                if (this.showHelmet == false)
                {
                    eva.ShowHelmet(this.showHelmet);
                }
            }
            catch
            {
                throw new Exception("[EFX] FromSave Failed.");
            }        
        }


        internal void Follow()
        {
            Guid flightID = (FlightGlobals.fetch.activeVessel).id;
            EvaContainer leader = EvaController.fetch.GetEva(flightID);
                        
            selected = false;
            mode = Mode.Follow;
            formation.SetLeader(leader);
        }

        internal void Stay()
        {
            mode = Mode.None;
        }

        internal void SetWaypoint()
        {
            patrol.Move(eva.vessel);
        }

        internal void Wait()
        {
            patrol.Wait(eva.vessel);
        }

        internal void StartPatrol()
        {
            mode = Mode.Patrol;
        }

        internal void EndPatrol()
        {
            mode = Mode.None;
            patrol.Clear();
            eva.Animate(AnimationState.Idle);
        }

        internal void SetRunPatrolMode()
        {
            patrol.AllowRunning = true;
        }

        internal void SetWalkPatrolMode()
        {
            patrol.AllowRunning = false;
        }

        internal void ToggleHelmet()
        {
            this.showHelmet = !showHelmet;
            eva.ShowHelmet(this.showHelmet);
        }

        internal void UpdateLamps()
        {
            bool lampOn = Util.IsDark(eva.transform);

            if (showHelmet && !IsActive)
            {
                eva.TurnLamp(lampOn);
            }

            if (IsActive)
            {
                if (!showHelmet && eva.lampOn)
                {
                    eva.TurnLamp(false);
                }
            }
        }

        internal void RecoverFromRagdoll()
        {
            eva.RecoverFromRagdoll();
        }

        internal Vector3d GetNextTarget()
        {
            if (mode == Mode.Follow)
            {
                return formation.GetNextTarget();
            }
            else if (mode == Mode.Patrol)
            {
                return patrol.GetNextTarget();
            }
            else if (mode == Mode.Order)
            {
                return order.GetNextTarget();
            }

            //Error
            throw new Exception("[EFX] New Mode Introduced");
        }

        internal void ReleaseLadder()
        {
            eva.ReleaseLadder();
        }

        internal void UpdateAnimations(double sqrDist, ref float speed)
        {

            double geeForce = FlightGlobals.currentMainBody.GeeASL;

            if (eva.part.WaterContact)
            {
                speed *= eva.swimSpeed;
                eva.Animate(AnimationState.Swim);
            }
            else if (eva.JetpackDeployed)
            {
                speed *= 1f;
                eva.Animate(AnimationState.Idle);
            }
            else if (geeForce >= eva.minRunningGee)//sqrDist > 5f &&
            {
                if (AllowRunning)
                {
                    speed *= eva.runSpeed;
                    eva.Animate(AnimationState.Run);
                }
                else if (sqrDist > 5f && mode == Mode.Follow)
                {
                    speed *= eva.runSpeed;
                    eva.Animate(AnimationState.Run);
                }
                else
                {
                    speed *= eva.walkSpeed;
                    eva.Animate(AnimationState.Walk);
                }
            }
            else if (geeForce >= eva.minWalkingGee)
            {
                speed *= eva.walkSpeed;
                eva.Animate(AnimationState.Walk);
            }
            else
            {
                speed *= eva.boundSpeed * 1.25f; //speedup
                eva.Animate(AnimationState.BoundSpeed);
            }

        }

        internal void CheckDistance(Vector3d move, float speed, double sqrDist)
        {
            IEvaControlType controlType = null;

            if (mode == Mode.Follow){       controlType = formation;    }
            else if (mode == Mode.Patrol){  controlType = patrol;       }
            else if (mode == Mode.Order){   controlType = order;        }

            if (controlType.CheckDistance(sqrDist))
            {
                eva.Animate(AnimationState.Idle);

                if (controlType is EvaOrder)
                {
                    mode = Mode.None;
                }
            }
            else
            {
                if (AbleToMove())
                {
                    Move(move, speed);
                }
            }
        }

        private bool AbleToMove()
        {
            return (!eva.isEnabled) | (!eva.isRagdoll) | (!eva.rigidbody.isKinematic);
        }
        /// <summary>
        /// Move the current kerbal to target.
        /// </summary>
        /// <param name="move"></param>
        /// <param name="speed"></param>
        internal void Move(Vector3d move, float speed)
        {
            #region Move & Rotate Kerbal

            //speed values
            move *= speed;

            //rotate            
            if (move != Vector3d.zero)
            {
                if (eva.JetpackDeployed)
                {
                    eva.PackToggle();
                }
                else
                {
                    //rotation
                    Quaternion from = eva.part.vessel.transform.rotation;
                    Quaternion to = Quaternion.LookRotation(move, eva.fUp);
                    Quaternion result = Quaternion.RotateTowards(from, to, eva.turnRate);

                    eva.part.vessel.SetRotation(result);

                    //move   
                    eva.rigidbody.MovePosition(eva.rigidbody.position + move);
                }
            }

            #endregion
        }

        internal void CheckModeIsNone()
        {
            if(mode == Mode.None)
                eva.Animate(AnimationState.Idle);
        }
        
        internal void Order(Vector3d position, Vector3d vector3d)
        {
            order.Move(position, vector3d);
        }



  
    }
}
