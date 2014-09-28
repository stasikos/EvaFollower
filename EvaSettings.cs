using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MSD.EvaFollower
{
    class EvaSettings
    {
        private static bool savedNode = false;
        private static bool nodesLoaded = false;

        internal static bool displayDebugLines = false;

        public static void LoadConfiguration()
        {
            try
            {
                KSP.IO.TextReader tr = KSP.IO.TextReader.CreateForType<EvaSettings>("Config.cfg");
                string[] lines = tr.ReadToEnd().Split('\n');

                foreach (var line in lines)
                {
                    string[] parts = line.Split(':');

                    if (parts.Length > 1)
                    {
                        string name = parts[0].Trim();
                        string value = parts[1].Trim();

                        EvaDebug.DebugLog("EvaSettings: " + name);

                        switch (name)
                        {
                            case "DisplayDebugLines": { displayDebugLines = bool.Parse(value); } break;
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("[EFX] Config loading failed. ");
            }
        }

        public static void SaveConfiguration()
        {
            KSP.IO.TextWriter tr = KSP.IO.TextWriter.CreateForType<EvaSettings>("Config.cfg");
            tr.Write("DisplayDebugLines: true");
            tr.Close();
        }

        public static void Load()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                LoadFunction();
                nodesLoaded = true;
            }

        }

        public static void LoadFunction()
        {
            EvaDebug.ProfileStart();

            LoadFile();

            EvaDebug.ProfileEnd("EvaSettings.Load()");
        }


        public static void Save()
        {
            if (nodesLoaded && !savedNode)
            {
                SaveFunction();
                nodesLoaded = false;
                savedNode = true;
            }
        }

        public static void SaveFunction()
        {
            EvaDebug.ProfileStart();

            SaveFile();

            EvaDebug.ProfileEnd("EvaSettings.Save()");
        }
    
        private static void LoadFile()
        {
            KSP.IO.TextReader tr = KSP.IO.TextReader.CreateForType<EvaSettings>(String.Format("Evas-{0}.txt", HighLogic.CurrentGame.Title));
            string file = tr.ReadToEnd();
            tr.Close();

            EvaTokenReader reader = new EvaTokenReader(file);

            //read every eva.
            while (!reader.EOF)
            {
                LoadEva(reader.NextToken('[', ']'));
            }
        }

        private static void LoadEva(string eva)
        {
            Guid flightID = GetFlightIDFromEvaString(eva);
            EvaContainer container = EvaController.fetch.GetEva(flightID);

            if (container != null)
            {
                container.FromSave(eva);
            }
        }

        private static Guid GetFlightIDFromEvaString(string evaString)
        {
            EvaTokenReader reader = new EvaTokenReader(evaString);

            string sflightID = reader.NextTokenEnd(',');

            //Load the eva
            Guid flightID = new Guid(sflightID);
            return flightID;
        }
        private static Status GetStatusFromEvaString(string evaString)
        {
            EvaTokenReader reader = new EvaTokenReader(evaString);

            reader.NextTokenEnd(',');
            reader.NextTokenEnd(',');
            string status = reader.NextTokenEnd(',');

            return (Status)Enum.Parse(typeof(Status), status);
        }


        private static void SaveFile()
        {
            //Load the old one from the list.
            #region Load 
            KSP.IO.TextReader tr = KSP.IO.TextReader.CreateForType<EvaSettings>(String.Format("Evas-{0}.txt", HighLogic.CurrentGame.Title));
            string file = tr.ReadToEnd();
            tr.Close();

            EvaTokenReader reader = new EvaTokenReader(file);

            Dictionary<Guid, string> oldKerbals = new Dictionary<Guid, string>();

            //read every eva.
            while (!reader.EOF)
            {
                string evaString = reader.NextToken('[', ']');
                Guid flightID = GetFlightIDFromEvaString(evaString);
                oldKerbals.Add(flightID, evaString);
            }
            #endregion

            ///now save it.
            KSP.IO.TextWriter tw = KSP.IO.TextWriter.CreateForType<EvaSettings>(String.Format("Evas-{0}.txt", HighLogic.CurrentGame.Title));

            List<Guid> inMemory = new List<Guid>();

            foreach (var eva in EvaController.fetch.collection)
            {
                inMemory.Add(eva.flightID);

                SaveEvaNode(tw, eva);                
            }

             
            foreach (var g in oldKerbals)
            {
                if (inMemory.Contains(g.Key))
                {
                    //don't have to save.
                }
                else
                {
                    Status status = GetStatusFromEvaString(g.Value);

                    if (status == Status.None)
                    {
                        //add it.
                        EvaContainer eva = new EvaContainer(g.Key);
                        eva.FromSave(g.Value);
                        SaveEvaNode(tw, eva);
                    }
                }
            }

            tw.Close();
        }



        private static void SaveEvaNode(KSP.IO.TextWriter tw, EvaContainer eva)
        {
            tw.Write("[" + eva.ToSave() + "]" + Environment.NewLine);
        }        
    }
}
