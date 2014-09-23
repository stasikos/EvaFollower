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
        bool showHelmet = true;

        [KSPEvent(guiActive = true, guiName = "Follow Me", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void Follow()
        {
            uint flightID = (FlightGlobals.fetch.activeVessel).parts[0].flightID;
     
            EvaContainer leader = EvaController.fetch.GetEva(flightID);

            _currentKerbal.Selected = false;
            _currentKerbal.Formation.Leader = leader;
            _currentKerbal.Mode = Mode.Follow;

        }

        [KSPEvent(guiActive = true, guiName = "Stay Put", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void Stay()
        {
            _currentKerbal.Formation.Leader = null;
            _currentKerbal.Mode = Mode.None;
        }

        [KSPEvent(guiActive = true, guiName = "Add Waypoint", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void SetPoint()
        {
             Vector3d position = _currentKerbal.EVA.vessel.GetWorldPos3D();
            _currentKerbal.Patrol.Move(position);
        }

        [KSPEvent(guiActive = true, guiName = "Wait", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void Wait()
        {
            Vector3d position = _currentKerbal.EVA.vessel.GetWorldPos3D();
            _currentKerbal.Patrol.Wait(position);
            SetEvents();
        }


        [KSPEvent(guiActive = true, guiName = "Patrol", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void Patrol()
        {
            _currentKerbal.Mode = Mode.Patrol;
            SetEvents();
        }

        [KSPEvent(guiActive = true, guiName = "End Patrol", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void EndPatrol()
        {
            _currentKerbal.Mode = Mode.None;
            _currentKerbal.Patrol.Clear();
            _currentKerbal.Animate(AnimationState.Idle, true);
        }

        [KSPEvent(guiActive = true, guiName = "Run", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void PatrolRun()
        {
            _currentKerbal.Patrol.AllowRunning = true;
        }

        [KSPEvent(guiActive = true, guiName = "Walk", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void PatrolWalk()
        {
            _currentKerbal.Patrol.AllowRunning = false;
        }

        [KSPEvent(guiActive = true, guiName = "Toggle Helmet", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void ToggleHelmet()
        {
            showHelmet = !showHelmet;
            _currentKerbal.EVA.ShowHelmet(showHelmet);
        }

        [KSPEvent(guiActive = true, guiName = "Jump", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void Jump()
        {
            _currentKerbal.EVA.Jump();
        }

        [KSPEvent(guiActive = true, guiName = "Ladder Grab", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void LadderGrab()
        {
            _currentKerbal.EVA.GrapLadder();
        }

        [KSPEvent(guiActive = true, guiName = "Pack Toggle", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void PackToggle()
        {            
            _currentKerbal.EVA.PackToggle();
        }

        [KSPEvent(guiActive = true, guiName = "FearFactor", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void FearFactor()
        {
            _currentKerbal.EVA.FearFactor(-1000000f);
        }

        [KSPEvent(guiActive = true, guiName = "FindKerbal", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void FindKerbal()
        {
            var expS = GameObject.FindObjectsOfType<Kerbal>();

                foreach (var item in expS)
                {

                    EvaDebug.DebugLog("N: " + item.name);
                    EvaDebug.DebugLog("C: " + item.courage);
                    EvaDebug.DebugLog("S: " + item.stupidity);
                    EvaDebug.DebugLog("B: " + item.isBadass);


                    EvaDebug.DebugLog("C: " + item.gameObject.name);
                }
            
        }
      

        [KSPEvent(guiActive = true, guiName = "Debug", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void Debug()
        {
            _currentKerbal.Info();

            EvaDebug.DebugLog("Current:");
            foreach (var item in _currentKerbal.EVA.fsm.CurrentState.StateEvents)
            {
                EvaDebug.DebugLog(item.name);
            }
            EvaDebug.DebugLog("Last:");
            foreach (var item in _currentKerbal.EVA.fsm.LastState.StateEvents)
            {
                EvaDebug.DebugLog(item.name);
            }

            var expS = UnityEngine.Object.FindObjectsOfType<kerbalExpressionSystem>();

            foreach (var item in expS)
            {
                EvaDebug.DebugLog(item.name);
                EvaDebug.DebugLog("panicLevel:" + item.panicLevel);
                EvaDebug.DebugLog("fearFactor:" + item.fearFactor);
                EvaDebug.DebugLog("expression:" + item.expression);
                EvaDebug.DebugLog("expressionParameterName:" + item.expressionParameterName);
                EvaDebug.DebugLog("varianceParameterName:" + item.varianceParameterName);
                EvaDebug.DebugLog("secondaryVarianceParameterName:" + item.secondaryVarianceParameterName);

                item.fearFactor = 100;
            }

        }

                   
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

            if (!part.GroundContact)
                return;            
    
            bool isActive = (part.vessel == FlightGlobals.ActiveVessel);

            if (_currentKerbal.Mode == Mode.None)
            {
                Events["Follow"].active = true;
                Events["Stay"].active = false;
            }
            else if (_currentKerbal.Mode == Mode.Follow)
            {
                Events["Follow"].active = false;
                Events["Stay"].active = true;
            }
            else if (_currentKerbal.Mode == Mode.Patrol)
            {
                Events["Follow"].active = false;
                Events["Stay"].active = false;

                if (_currentKerbal.Patrol.AllowRunning)
                {
                    Events["PatrolWalk"].active = true;
                }
                else
                {
                    Events["PatrolRun"].active = true;
                }

                Events["Patrol"].active = false;
                Events["EndPatrol"].active = true;
            }
            else if (_currentKerbal.Mode == Mode.Order)
            {
                Events["Stay"].active = true;
                Events["Follow"].active = true;
            }
            

            if(isActive)
            {

                Events["Follow"].active = false;
                Events["Stay"].active = false;
                Events["SetPoint"].active = true;
                Events["Wait"].active = true;

                if (_currentKerbal.Mode != Mode.Patrol)
                {
                    if (_currentKerbal.Patrol.actions.Count >= 2)
                    {
                        Events["Patrol"].active = true;
                    }
                }
                else
                {
                    Events["SetPoint"].active = false;
                    Events["Wait"].active = false;
                }                
            }           
        }

        /// <summary>
        /// The default events based on the kerbal status.
        /// </summary>
        private void ResetEvents()
        {
            Events["Follow"].active = false;
            Events["Stay"].active = false;
            Events["SetPoint"].active = false;
            Events["Wait"].active = false;
            Events["Patrol"].active = false;
            Events["EndPatrol"].active = false;
            Events["PatrolRun"].active = false;
            Events["PatrolWalk"].active = false;
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
