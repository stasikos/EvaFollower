using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSD.EvaFollower
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class EvaController : MonoBehaviour
    {
        public static EvaController fetch;

        public List<EvaContainer> collection = new List<EvaContainer>();

        public void Start()
        {

            EvaDebug.DebugWarning("EvaController.Start()");
            //initialize the singleton.
            fetch = this;

            GameEvents.onFlightReady.Add(onFlightReadyCallback);

            GameEvents.onVesselCreate.Add(OnVesselCreate);
            GameEvents.onVesselLoaded.Add(OnVesselLoaded);
            GameEvents.onVesselChange.Add(OnVesselChange);
            GameEvents.onVesselTerminated.Add(OnVesselTerminated);
            
            GameEvents.onCrewOnEva.Add(OnCrewOnEva);
            GameEvents.onCrewBoardVessel.Add(OnCrewBoardVessel);
            GameEvents.onCrewKilled.Add(OnCrewKilled);
            
            GameEvents.onGameStateSave.Add(OnSave);
        }

        public void OnDestroy()
        {
            //EvaDebug.DebugWarning("EvaController.OnDestroy()");
        
            GameEvents.onFlightReady.Remove(onFlightReadyCallback);

            GameEvents.onVesselCreate.Remove(OnVesselCreate);
            GameEvents.onVesselLoaded.Remove(OnVesselLoaded);
            GameEvents.onVesselChange.Remove(OnVesselChange);
            GameEvents.onVesselTerminated.Remove(OnVesselTerminated);

            GameEvents.onCrewOnEva.Remove(OnCrewOnEva);
            GameEvents.onCrewBoardVessel.Remove(OnCrewBoardVessel);
            GameEvents.onCrewKilled.Remove(OnCrewKilled);

            GameEvents.onGameStateSave.Remove(OnSave);
        }
           
        /// <summary>
        /// Load the list 
        /// </summary>
        private void onFlightReadyCallback()
        {
            //Load the eva list.
            //EvaDebug.DebugLog("onFlightReadyCallback()");
            FetchEVAS();
        }

        /// <summary>
        /// Fetch all evas in the universe.
        /// </summary>
        private void FetchEVAS()
        {
            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                AddEva(vessel);
            }

            EvaSettings.Load();
        }


        public void OnSave(ConfigNode node)
        {
            //Save the eva list.
            //EvaDebug.DebugLog("OnSave()");

            EvaSettings.Save();
        }

        private void OnVesselCreate(Vessel vessel)
        {
            //add new kerbal, or load it.
            //EvaDebug.DebugLog("OnVesselCreate()");
            AddEva(vessel);
        }

        public void OnVesselLoaded(Vessel vessel)
        {
            //add new kerbal, or load it.
            //EvaDebug.DebugLog("OnVesselCreate()");
            AddEva(vessel);
        }


        public void OnVesselChange(Vessel vessel)
        {
            //EvaDebug.DebugLog("OnVesselChange()");
            //AddEva(vessel);
        }

        public void OnVesselTerminated(ProtoVessel protoVessel)
        {
            //Remove protoVessel.
            //EvaDebug.DebugLog("OnVesselTerminated()");
            //RemoveEva(protoVessel.vesselID);
        }

        /// <summary>
        /// Runs when the kerbal goes on EVA.
        /// </summary>
        /// <param name="e"></param>
        public void OnCrewOnEva(GameEvents.FromToAction<Part, Part> e)
        {
            //add new kerbal
            //EvaDebug.DebugLog("OnCrewOnEva()");
            AddEva(e.to.vessel);
        }

        /// <summary>
        /// Runs when the EVA goes onboard a vessel.
        /// </summary>
        /// <param name="e"></param>
        public void OnCrewBoardVessel(GameEvents.FromToAction<Part, Part> e)
        {
            //remove kerbal
            //EvaDebug.DebugLog("OnCrewBoardVessel()");
            RemoveEva(e.from.vessel);
        }

        /// <summary>
        /// Runs when the EVA is killed.
        /// </summary>
        /// <param name="report"></param>
        public void OnCrewKilled(EventReport report)
        {
            //EvaDebug.DebugLog("OnCrewKilled()");
            RemoveEva(report.origin.vessel);
        }

        public void AddEva(Vessel vessel)
        {
            if (!vessel.isEVA)
                return;

            if (!Contains(vessel.id))
            {
                EvaContainer container = new EvaContainer(vessel.id);

                KerbalEVA eva = vessel.GetComponent<KerbalEVA>();

                if (eva == null)
                    return; //skip for now.

                if (vessel.loaded)
                    container.Load(eva);

                collection.Add(container);

            }
            else
            {
                EvaContainer container = GetEva(vessel.id);

                if (container.Loaded)
                    return;

                KerbalEVA eva = vessel.GetComponent<KerbalEVA>();

                if (eva == null)
                    return; //skip for now.

                if (vessel.loaded)
                    container.Load(eva);
            }
        }

        public void RemoveEva(Vessel vessel)
        {
            lock (collection)
            {
                //EvaDebug.DebugLog("RemoveEva()");

                for (int i = 0; i < collection.Count; i++)
                {
                    if (collection[i].flightID == vessel.id)
                    {
                        collection[i].Unload();
                    }
                }
            }
        }

        internal bool Contains(Guid id)
        {
            //EvaDebug.DebugLog("Contains()");

            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i].flightID == id)
                    return true;
            }

            return false;
        }


        internal EvaContainer GetEva(Guid flightID)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i].flightID == flightID)
                    return collection[i];
            }

            return null;
        }
    }
}
