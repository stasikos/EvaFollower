using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MSD.EvaFollower
{
    /// <summary>
    /// Keep track of the Context Menu. 
    /// </summary>
    class EvaModule : PartModule
    {
        
        EvaContainer _currentKerbal;

        [KSPEvent(guiActive = true, guiName = "Follow Me", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void eFollow()
        {
            Guid flightID = (FlightGlobals.fetch.activeVessel).id;
            EvaContainer leader = EvaController.fetch.GetEva(flightID);

            
            _currentKerbal.Selected = false;
            _currentKerbal.Formation.Leader = leader;
            _currentKerbal.Mode = Mode.Follow;

        }

        [KSPEvent(guiActive = true, guiName = "Stay Put", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void eStay()
        {
            _currentKerbal.Formation.Leader = null;
            _currentKerbal.Mode = Mode.None;
        }

        [KSPEvent(guiActive = true, guiName = "Add Waypoint", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void eSetPoint()
        {
            SetReferenceBody();

            Vector3d position = Util.GetWorldPos3DSave(_currentKerbal.EVA.vessel);   
            _currentKerbal.Patrol.Move(position);
            
            SetEvents();
        }


        [KSPEvent(guiActive = true, guiName = "Wait", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void eWait()
        {
            SetReferenceBody();

            Vector3d position = _currentKerbal.EVA.vessel.GetWorldPos3D();        
            _currentKerbal.Patrol.Wait(position);

            SetEvents();
        } 
        
        private void SetReferenceBody()
        {
            if (_currentKerbal.Patrol.referenceBody == "None")
            {
                _currentKerbal.Patrol.referenceBody = FlightGlobals.ActiveVessel.mainBody.bodyName;
            }
        }

        [KSPEvent(guiActive = true, guiName = "Patrol", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void ePatrol()
        {
            _currentKerbal.Mode = Mode.Patrol;
            SetEvents();
        }

        [KSPEvent(guiActive = true, guiName = "End Patrol", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void eEndPatrol()
        {
            _currentKerbal.Mode = Mode.None;
            _currentKerbal.Patrol.Clear();
            _currentKerbal.Animate(AnimationState.Idle, true);
        }

        [KSPEvent(guiActive = true, guiName = "Run", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void ePatrolRun()
        {
            _currentKerbal.Patrol.AllowRunning = true;
        }

        [KSPEvent(guiActive = true, guiName = "Walk", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void ePatrolWalk()
        {
            _currentKerbal.Patrol.AllowRunning = false;
        }

        [KSPEvent(guiActive = true, guiName = "Toggle Helmet", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void eToggleHelmet()
        {
            bool showHelmet = !_currentKerbal.HelmetOn;
            _currentKerbal.EVA.ShowHelmet(showHelmet);
            _currentKerbal.HelmetOn = showHelmet;
        }

        /*
        [KSPEvent(guiActive = true, guiName = "RCS", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void RCS()
        {

            double geeForce = FlightGlobals.currentMainBody.GeeASL;

            EvaDebug.DebugLog("Force: " + geeForce);
            EvaDebug.DebugLog("walkSpeed: " + _currentKerbal.EVA.walkSpeed);
            EvaDebug.DebugLog("runSpeed: " + _currentKerbal.EVA.runSpeed);
            EvaDebug.DebugLog("minRunningGee: " + _currentKerbal.EVA.minRunningGee);
            EvaDebug.DebugLog("minWalkingGee: " + _currentKerbal.EVA.minWalkingGee);

            _currentKerbal.EVA.PackToggle();
        }
        */

        /*
        [KSPEvent(guiActive = true, guiName = "Debug", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void Debug()
        {
            _currentKerbal.EVA.FearFactor(1000);

            EvaDebug.DebugWarning("name:" + FlightGlobals.getMainBody().name);
            EvaDebug.DebugWarning("bodyName: " + FlightGlobals.getMainBody().bodyName);


            EvaDebug.DebugWarning("cname:" + FlightGlobals.currentMainBody.name);
            EvaDebug.DebugWarning("cbodyName: " + FlightGlobals.currentMainBody.bodyName);
        }*/
        
                   
        public void Start()
        {
            EvaDebug.DebugLog("Loaded EvaModule.");

            SetEvents();
            GameEvents.onCrewOnEva.Add(new EventData<GameEvents.FromToAction<Part, Part>>.OnEvent(OnCrewOnEva));
            GameEvents.onVesselChange.Add(new EventData<Vessel>.OnEvent(onVesselChange));
            GameEvents.onFlightReady.Add(new EventVoid.OnEvent(onFlightReadyCallback));
        }

                /// <summary>
        /// Set the events based on the kerbal status.
        /// </summary>
        private void SetEvents()
        {
            if (_currentKerbal == null)
                return;

            ResetEvents();

            if (!part.vessel.Landed)
                return;

            if (_currentKerbal.Mode == Mode.None)
            {
                Events["eFollow"].active = true;
                Events["eStay"].active = false;
            }
            else if (_currentKerbal.Mode == Mode.Follow)
            {
                Events["eFollow"].active = false;
                Events["eStay"].active = true;
            }
            else if (_currentKerbal.Mode == Mode.Patrol)
            {
                Events["eFollow"].active = false;
                Events["eStay"].active = false;

                if (_currentKerbal.Patrol.AllowRunning)
                {
                    Events["ePatrolWalk"].active = true;
                }
                else
                {
                    Events["ePatrolRun"].active = true;
                }

                Events["ePatrol"].active = false;
                Events["eEndPatrol"].active = true;
            }
            else if (_currentKerbal.Mode == Mode.Order)
            {
                Events["eStay"].active = true;
                Events["eFollow"].active = true;
            }

            if (FlightGlobals.currentMainBody != null)
            {
                if (FlightGlobals.currentMainBody.bodyName == "Kerbin")
                {
                    Events["eToggleHelmet"].active = true;
                }
            }

            bool isActive = (part.vessel == FlightGlobals.ActiveVessel);
            if(isActive)
            {
            

                Events["eFollow"].active = false;
                Events["eStay"].active = false;
                Events["eSetPoint"].active = true;
                Events["eWait"].active = true;

                if (_currentKerbal.Mode != Mode.Patrol)
                {
                    if (_currentKerbal.Patrol.actions.Count >= 2)
                    {
                        Events["ePatrol"].active = true;
                    }
                }
                else
                {
                    Events["eSetPoint"].active = false;
                    Events["eWait"].active = false;
                }                
            }           
        }

        /// <summary>
        /// The default events based on the kerbal status.
        /// </summary>
        private void ResetEvents()
        {
            Events["eFollow"].active = false;
            Events["eStay"].active = false;
            Events["eSetPoint"].active = false;
            Events["eWait"].active = false;
            Events["ePatrol"].active = false;
            Events["eEndPatrol"].active = false;
            Events["ePatrolRun"].active = false;
            Events["ePatrolWalk"].active = false;
            Events["eToggleHelmet"].active = false;
        }

        /// <summary>
        /// Runs when the kerbal goes on EVA.
        /// </summary>
        /// <param name="e"></param>
        public void OnCrewOnEva(GameEvents.FromToAction<Part, Part> e)
        {
            SetEvents();
        }

        public void onFlightReadyCallback()
        {  
            //initialize extentions.
            SetEvents();        
        }

        public void onVesselChange(Vessel vessel)
        {
            SetEvents();
        }

        public void Initialize(EvaContainer kerbal)
        {
            _currentKerbal = kerbal;

            SetEvents();
        }

        public void Update()
        {
            SetEvents();
        }
    }
}
