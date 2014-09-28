using System;
using UnityEngine;

namespace MSD.EvaFollower
{
  
    class Util
    { 
        
        private static bool isDark = false;

        /// <summary>
        /// Returns true if there is no direct line with the sun.
        /// The forceupdate should be used so it doesn't update every second.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="forceUpdate"></param>
        /// <returns></returns>
        public static bool IsDark(Transform from, bool forceUpdate = true)
        {
            //raycast to the sun ?
            if (forceUpdate)
            {
                var target = FlightGlobals.Bodies[0].transform;
                RaycastHit hit;
                if (Physics.Raycast(from.position, target.position, out hit))
                {

                    if (hit.transform.name == target.name)
                    {
                        isDark = false;
                        return false;
                    }
                    else
                    {
                        //shadow.
                        isDark = true;
                        return true;
                    }
                }

                return false;
            }
            else
            {
                return isDark;
            }
        }
        
        private Vector3d MoveMax(Vector3d move)
        {
            double x = move.x;
            double y = move.y;
            double z = move.z;

            double ax = Math.Abs(x);
            double ay = Math.Abs(y);
            double az = Math.Abs(z);

            x = (ax > ay) ? ((ax > az) ? x : 0) : 0;
            y = (ay > ax) ? ((ay > az) ? y : 0) : 0;
            z = (az > ax) ? ((az > ay) ? z : 0) : 0;


            return new Vector3d(x, y, z);
        }

        /// <summary>
        /// Get the position on the planet.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3d GetWorldPos3DSave(Vessel v)
        {
            return new Vector3d(v.latitude,v.longitude,v.altitude);
        }

        public static Vector3d GetWorldPos3DLoad(Vector3d v)
        {
            return FlightGlobals.getMainBody().GetWorldSurfacePosition(v.x, v.y, v.z);
        }


        public static Vector3d ParseVector3d(string value, bool removeTokens = true)
        {
            if (removeTokens)
            {
                value = value.Remove(0, 1);
                value = value.Remove(value.Length - 1, 1);
            }

            string[] vals = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            Vector3d v = new Vector3d();

            v.x = double.Parse(vals[0]);
            v.y = double.Parse(vals[1]);
            v.z = double.Parse(vals[2]);
            
            return v;
        }

    }     

    static class Extentions
    {
        public static Vector3d Trim(this Vector3d v)
        {
            v.x = double.Parse(v.x.ToString("0.0000"));
            v.y = double.Parse(v.y.ToString("0.0000"));
            v.z = double.Parse(v.z.ToString("0.0000"));

            return v;
        }
    }
}
