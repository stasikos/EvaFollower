using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MSD.EvaFollower
{
    public static class KerbalEvaExtentions
    {
        /*
         * [LOG 22:06:53.574] [EFX] Move (Arcade)
[LOG 22:06:53.575] [EFX] Move (FPS)
[LOG 22:06:53.575] [EFX] Move Low G (Arcade)
[LOG 22:06:53.576] [EFX] Move Low G (FPS)
[LOG 22:06:53.577] [EFX] Jump Start
[LOG 22:06:53.577] [EFX] Fall
[LOG 22:06:53.578] [EFX] Stumble
[LOG 22:06:53.578] [EFX] Pack Toggle
[LOG 22:06:53.579] [EFX] Feet Wet
[LOG 22:06:53.580] [EFX] Ladder Grab Start
[LOG 22:06:53.580] [EFX] Boarding Part
[LOG 22:06:53.581] [EFX] Flag Plant Started
[LOG 22:06:53.582] [EFX] Seat Board
[LOG 22:06:53.582] [EFX] Grapple
         */

        public static Mesh helmetMesh = null;
        public static Mesh visorMesh = null;
        
        public static void ShowHelmet(this KerbalEVA eva,bool showHelmet)
        {             
            foreach (Renderer renderer in eva.GetComponentsInChildren<Renderer>())
            {
                var smr = renderer as SkinnedMeshRenderer;

                if (smr != null)
                {
                    switch (smr.name)
                    {
                        case "helmet":
                            {
                                if (helmetMesh == null)
                                    helmetMesh = smr.sharedMesh;

                                smr.sharedMesh = showHelmet ? helmetMesh : null; 
                            }break;
                        case "visor":
                            {
                                if (visorMesh == null)
                                    visorMesh = smr.sharedMesh;

                                smr.sharedMesh = showHelmet ? visorMesh : null;
                            } break;
                    }
                }
            }
        }

        public static void Jump(this KerbalEVA eva)
        {
            foreach (var item in eva.fsm.CurrentState.StateEvents)
            {
                if (item.name == "Jump Start")
                {
                    eva.fsm.RunEvent(item);
                }
            }
        }

        public static void GrapLadder(this KerbalEVA eva)
        {
            foreach (var item in eva.fsm.CurrentState.StateEvents)
            {
                if (item.name == "Ladder Grab Start")
                {
                    eva.fsm.RunEvent(item);
                }
            }
        }


        public static void PackToggle(this KerbalEVA eva)
        {
            foreach (var item in eva.fsm.CurrentState.StateEvents)
            {
                if (item.name ==  "Pack Toggle")
                {
                    eva.fsm.RunEvent(item);
                }
            }         
        }

        public static void FearFactor(this KerbalEVA eva, float fear)
        {
            var expS = UnityEngine.Object.FindObjectsOfType<kerbalExpressionSystem>();

            foreach (var item in expS)
            {
                if (item.kerbalEVA == eva)
                {
                    item.flight_gee = 10000;
                }
            }
        }

    }
}
