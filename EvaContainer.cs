using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MSD.EvaFollower
{
    class EvaContainer
    {
        private uint flightID;
        private KerbalEVA _eva;
        private Part _part;
        private EvaModule _module;
        private AnimationState _currentAnimationType = AnimationState.None;
        private Mode _evaMode = Mode.None;
        private EvaFormation _formation = new EvaFormation();
        private EvaPatrol _patrol = new EvaPatrol();
        private EvaOrder _order = new EvaOrder();
        private bool _selected = false;
        
        public bool _debug = false;

        /// <summary>
        /// The flight ID of the kerbal.
        /// </summary>
        public uint FlightID
        {
            get
            {
                return flightID;
            }
            set
            {
                flightID = value;
            }
        }

        /// <summary>
        /// The name of the EVA
        /// </summary>
        public string Name
        {
            get
            {
                return _eva.name;
            }
        }

        /// <summary>
        /// The EVA object itself
        /// </summary>
        public KerbalEVA EVA
        {
            get { return _eva; }
        }

        /// <summary>
        /// The current mode the EVA is in.
        /// </summary>
        public Mode Mode
        {
            get { return _evaMode; }
            set { _evaMode = value;  }
        }
        
        /// <summary>
        /// Formation Object is responsible of all formations.
        /// </summary>
        public EvaFormation Formation
        {
            get { return _formation; }
            set { _formation = value; }
        }        

        /// <summary>
        /// Patrol Object is responsible of all Patrol movements.
        /// </summary>
        public EvaPatrol Patrol
        {
            get { return _patrol; }
            set { _patrol = value; }
        }

        /// <summary>
        /// Order Object is responsible of all orders done by selections.
        /// </summary>
        public EvaOrder Order
        {
            get { return _order; }
            set { _order = value; }
        }

        /// <summary>
        /// Returns if the current kerbal is selected.
        /// </summary>
        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; }
        }

        public EvaContainer()
        {

        }

        public EvaContainer(Vessel vessel)
        {

            this.flightID = vessel.parts[0].flightID;
            this._part = vessel.parts[0];
            this._eva = ((KerbalEVA)_part.Modules["KerbalEVA"]);

            this._formation = new EvaFormation();

            //module on last.
            this._module = (EvaModule)_eva.GetComponent(typeof(EvaModule));
            this._module.Initialize(this);

        }

        /// <summary>
        /// Move the current kerbal to target.
        /// </summary>
        /// <param name="move"></param>
        /// <param name="speed"></param>
        private void Move(Vector3d move, float speed)
        {
            #region Move & Rotate Kerbal

            //speed values
            move *= speed;

            //rotate
            if (move != Vector3d.zero)
            {
                Quaternion from = _part.vessel.transform.rotation;
                Quaternion to = Quaternion.LookRotation(move, _eva.fUp);
                Quaternion result = Quaternion.RotateTowards(from, to, _eva.turnRate);
                _part.vessel.SetRotation(result);
            }

            //move
            _eva.rigidbody.MovePosition(_eva.rigidbody.position + move);

            #endregion
        }

        /// <summary>
        /// Animate the kerbal
        /// </summary>
        /// <param name="state">Set the animation to use and set</param>
        /// <param name="force">Force to set the animation. Should be used after ragedoll.</param>
        public void Animate(AnimationState state, bool force)
        {           
            string anim = GetAnimationName(state);

            if (!string.IsNullOrEmpty(anim))
            {
                //check the current animation.
                if(!_eva.animation[anim].enabled)
                    _eva.animation.CrossFade(anim);
            }

            _currentAnimationType = state;
        }

        private string GetAnimationName(AnimationState state)
        {
            string anim = "idle";

            switch (state)
            {
                case AnimationState.None: { } break;
                case AnimationState.Swim: { anim = "swim_forward"; } break;
                case AnimationState.Run: { anim = "wkC_run"; } break;
                case AnimationState.Walk: { anim = "wkC_forward"; } break;
                case AnimationState.BoundSpeed: { anim = "wkC_loG_forward"; } break;
                case AnimationState.Idle:
                    {

                        if (_part.WaterContact)
                            anim = "swim_idle";
                        else
                            anim = "idle";

                    } break;
            }
            return anim;
        }

        /// <summary>
        /// Check if the current animation is what it clams to be.
        /// </summary>
        /// <returns></returns>
        private bool IsStatedAnimationPlaying()
        {
            string anim = GetAnimationName(_currentAnimationType);
            return _eva.animation[anim].enabled && !_eva.isRagdoll;
        }

        public void Update(double geeForce)
        {
     
            #region Check kerbal state
            if (_eva.isRagdoll)
            {
                //Much Kudos to Razchek for finally slaying the Ragdoll Monster!
                if (_eva.canRecover && _eva.fsm.TimeAtCurrentState > 1.21f)
                { //&& !currentEVA.part.GroundContact
                    foreach (KFSMEvent stateEvent in _eva.fsm.CurrentState.StateEvents)
                    {
                        if (stateEvent.name == "Recover Start")
                        {
                            _eva.fsm.RunEvent(stateEvent);
                            break;
                        }
                    }
                }
            }


            #endregion

            Vector3d move = -_eva.vessel.GetWorldPos3D();

            #region Get next Action, Formation or Patrol

      
            if (_evaMode == Mode.Follow)
            {
                if (_formation.Leader != null) //reset if leader is gone
                    _formation.GetNextTarget(ref move);
                else
                    _evaMode = Mode.None;

            }
            else if (_evaMode == Mode.Patrol)
            {
                _patrol.GetNextTarget(ref move);
            }
            else if (_evaMode == EvaFollower.Mode.Order)
            {
                _order.GetNextTarget(ref move);
            }

            #endregion

            #region Path Finding

            double sqrDist = move.sqrMagnitude;
            float speed = TimeWarp.deltaTime;


            #endregion

            #region Break Free Code

            if( _evaMode == EvaFollower.Mode.Order &&
                (FlightGlobals.ActiveVessel == _eva.vessel))
            {
                if (Input.GetKey(KeyCode.W))
                    _evaMode = EvaFollower.Mode.None;
                if (Input.GetKey(KeyCode.S))
                    _evaMode = EvaFollower.Mode.None;
                if (Input.GetKey(KeyCode.A))
                    _evaMode = EvaFollower.Mode.None;
                if (Input.GetKey(KeyCode.D))
                    _evaMode = EvaFollower.Mode.None; 
                if (Input.GetKey(KeyCode.Q))
                    _evaMode = EvaFollower.Mode.None;
                if (Input.GetKey(KeyCode.E))
                    _evaMode = EvaFollower.Mode.None;

                if (_evaMode == EvaFollower.Mode.None)
                    return;
            }

            #endregion

            #region Animation Logic

            if (_part.WaterContact)
            {
                speed *= _eva.swimSpeed;
                Animate(AnimationState.Swim, false);
            }
            else if (!_part.GroundContact)
            {
                speed = 0;
            }
            else if (_evaMode == Mode.Patrol)
            {
                if (_patrol.AllowRunning)
                {
                    speed *= _eva.runSpeed;
                    Animate(AnimationState.Run, false);
                }
                else
                {
                    speed *= _eva.walkSpeed;
                    Animate(AnimationState.Walk, false);
                }
            }
           else if (_evaMode == EvaFollower.Mode.Order)
           {
               if (_order.AllowRunning)
               {
                   speed *= _eva.runSpeed;
                   Animate(AnimationState.Run, false);
               }
               else
               {
                   speed *= _eva.walkSpeed;
                   Animate(AnimationState.Walk, false);
               }
           }
           else if (sqrDist > 5f && geeForce >= _eva.minRunningGee)
           {
               speed *= _eva.runSpeed;
               Animate(AnimationState.Run, false);
           }

           else if (geeForce >= _eva.minWalkingGee)
           {
               speed *= _eva.walkSpeed;
               Animate(AnimationState.Walk, false);
           }
           else
           {
               speed *= _eva.boundSpeed;
               Animate(AnimationState.BoundSpeed, false);
           }

            #endregion

            move.Normalize();

            #region Distance Logic

            IEvaControlType controlType = null;

            if (_evaMode == Mode.Follow)
            {
                controlType = _formation;
            }
            else if (_evaMode == Mode.Patrol)
            {
                controlType = _patrol;
            }
            else if (_evaMode == EvaFollower.Mode.Order)
            {
                controlType = _order;
            }

            if (controlType.CheckDistance(sqrDist))
            {
                Animate(AnimationState.Idle, false);

                if (controlType is EvaOrder)
                {
                    _evaMode = EvaFollower.Mode.None;
                }
            }
            else
            {
                if (IsStatedAnimationPlaying())
                {
                    Move(move, speed);
                }
            }

            #endregion
            
        }
    }
}
