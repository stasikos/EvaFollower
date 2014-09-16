using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MSD.EvaFollower
{
    /// <summary>
    /// Add the module to all kerbals avaible. 
    /// </summary>
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class EvaAddonAddModule : MonoBehaviour
    {
        public void Awake()
        {
            EvaDebug.DebugLog("Loaded AddonAddModule.");

            ConfigNode EVA = new ConfigNode("MODULE");
            EVA.AddValue("name", "EvaModule");

            try
            {
              PartLoader.getPartInfoByName("kerbalEVA").partPrefab.AddModule(EVA);
            }
            catch { }
        }

    }



}
