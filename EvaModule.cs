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
        [KSPField(isPersistant = true)]
        bool showHelmet = false;

        EvaContainer _currentKerbal;
        
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
            
#if DEBUG
            EvaDebug.DebugWarning("Added Waypoint: " + position.ToString());
#endif
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
            UpdateHelmet();
        }

#if DEBUG
        [KSPEvent(guiActive = true, guiName = "Debug", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
        public void Debug()
        {
     
            
        }
#endif
           
        public void Start()
        {
            EvaDebug.DebugLog("Loaded EvaModule.");

            SetEvents();
            GameEvents.onCrewOnEva.Add(new EventData<GameEvents.FromToAction<Part, Part>>.OnEvent(OnCrewOnEva));
            GameEvents.onVesselChange.Add(new EventData<Vessel>.OnEvent(onVesselChange));
            GameEvents.onFlightReady.Add(new EventVoid.OnEvent(onFlightReadyCallback));
            GameEvents.onVesselLoaded.Add(new EventData<Vessel>.OnEvent(onVesselLoaded));
        }

        private void UpdateHelmet()
        {
            if (_currentKerbal == null)
                return;
            
            if (_currentKerbal.EVA == null)
                return;


            foreach (Renderer renderer in _currentKerbal.EVA.GetComponentsInChildren<Renderer>())
            {
                var smr = renderer as SkinnedMeshRenderer;

                if (smr != null)
                {
                    switch (smr.name)
                    {
                        case "helmet":smr.sharedMesh = showHelmet ? EvaController.helmetMesh : null; break;
                        case "visor":smr.sharedMesh = showHelmet ?  EvaController.visorMesh : null;  break;
                    }
                }
            }

        }

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
                }                
            }           
        }

        private void ResetEvents()
        {
            Events["Follow"].active = false;
            Events["Stay"].active = false;
            Events["SetPoint"].active = false;
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
#if DEBUG
            EvaDebug.DebugLog("EvaModule.onFlightReadyCallback()");
#endif
            SetEvents();
            UpdateHelmet();
        }

        public void onVesselLoaded(Vessel vessel)
        {
#if DEBUG
            EvaDebug.DebugLog("EvaModule.onVesselLoaded()");
#endif
            if (part.flightID == vessel.parts[0].flightID)
            {
                UpdateHelmet();
            }
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
