using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SD = System.Diagnostics;

namespace MSD.EvaFollower
{

#if DEBUG
     [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class EvaDebug : MonoBehaviour
    {
         private Rect pos;
         private string content = "None";
         private GUIStyle style = null;
 
         public void Start()
         {
             DontDestroyOnLoad(this);
         }

         public void OnGUI()
         {
             if (HighLogic.LoadedScene == GameScenes.FLIGHT)
             {
                 if (style == null)
                 {	
					var w = 600;
					var h = 250;

					pos = new Rect(Screen.width - (20+w), 60, w, h);

	                style = new GUIStyle(GUI.skin.label);
	                style.alignment = TextAnchor.UpperRight;
	                style.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 0.6f);
                 }

                 GUI.Label(pos, content, style);
             }
         }

         public void Update()
         {
             if (HighLogic.LoadedScene == GameScenes.FLIGHT)
             {
				content = "Active Kerbals: " + EvaController.instance.collection.Count;
				content += Environment.NewLine + EvaController.instance.debug;
             }
             else
             {
                 content = "None";
             }
         }
#else 
          public class EvaDebug : MonoBehaviour
        {
#endif
        //Debug log yes/no
        private static bool debugLogActive = true;
        
        public static void DebugLog(string text)
        {
            if (debugLogActive)
            {
                Debug.Log("[EFX] " + text);
            }
        }

        public static void DebugLog(string text, UnityEngine.Object context)
        {
            if (debugLogActive)
            {
                Debug.Log("[EFX] " + text, context);
            }
        }

        public static void DebugWarning(string text)
        {
            if (debugLogActive)
            {
                Debug.LogWarning("[EFX] " + text);
            }
        }

        public static void DebugError(string text)
        {
            if (debugLogActive)
            {
                Debug.LogError("[EFX] " + text);
            }
        }


        public static void ProfileStart()
        {
            StartTimer();
        }

        public static void ProfileEnd(string name)
        {
            EndTimer();
            EvaDebug.DebugWarning(string.Format("Profile: {0}: {1}ms", name, Elapsed));
        }

        public static float Elapsed = 0;
        private static SD.Stopwatch watch;
        /// <summary>
        /// Start the timer
        /// </summary>
        private static void StartTimer()
        {
            watch = SD.Stopwatch.StartNew();
        }

        /// <summary>
        /// End the timer, and get the elapsed time.
        /// </summary>
        private static void EndTimer()
        {
            watch.Stop();
            Elapsed = watch.ElapsedMilliseconds;
        }
    }
}
