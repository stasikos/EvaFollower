using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SD = System.Diagnostics;

namespace MSD.EvaFollower
{
    class EvaDebug 
    {
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
        
#if DEBUG

        public static float Elapsed = 0;
        private static SD.Stopwatch watch;
        /// <summary>
        /// Start the timer
        /// </summary>
        public static void StartTimer()
        {
            watch = SD.Stopwatch.StartNew();
        }

        /// <summary>
        /// End the timer, and get the elapsed time.
        /// </summary>
        public static void EndTimer()
        {
            watch.Stop();
            Elapsed = watch.ElapsedMilliseconds;
        }
#endif
    }
}
