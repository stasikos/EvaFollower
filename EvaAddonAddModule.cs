using UnityEngine;

namespace MSD.EvaFollower
{
    /// <summary>
    /// Add the module to all kerbals available. 
    /// </summary>
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    class EvaAddonAddModule : MonoBehaviour
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
