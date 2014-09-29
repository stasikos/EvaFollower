using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MSD.EvaFollower
{
    class EvaSettings
    {
        internal static bool displayDebugLines = false;

        private static Dictionary<Guid, string> collection = new Dictionary<Guid, string>();

        private static bool isLoaded = false;

        public static void LoadConfiguration()
        {
            //try{
                if (FileExcist("Config.cfg"))
                {
                    KSP.IO.TextReader tr = KSP.IO.TextReader.CreateForType<EvaSettings>("Config.cfg");
                    string[] lines = tr.ReadToEnd().Split('\n');

                    foreach (var line in lines)
                    {
                        string[] parts = line.Split('=');

                        if (parts.Length > 1)
                        {
                            string name = parts[0].Trim();
                            string value = parts[1].Trim();
                            
                            switch (name)
                            {
                                case "ShowDebugLines": { displayDebugLines = bool.Parse(value); } break;
                            }
                        }
                    }
                }
            //}
            //catch
            //{
            //    throw new Exception("[EFX] Config loading failed. ");
            //}
        }

        public static void SaveConfiguration()
        {
            KSP.IO.TextWriter tr = KSP.IO.TextWriter.CreateForType<EvaSettings>("Config.cfg");
            tr.Write("ShowDebugLines = true");
            tr.Close();
        }

        public static bool FileExcist(string name)
        {
           return KSP.IO.File.Exists<EvaSettings>(name);
        }

        public static void Load()
        {
            //EvaDebug.DebugWarning("OnLoad()");
            //ScreenMessages.PostScreenMessage("Loading Kerbals...", 3, ScreenMessageStyle.LOWER_CENTER);
            LoadFunction();
        }

        public static void LoadFunction()
        {
            EvaDebug.ProfileStart();
            LoadFile();
            EvaDebug.ProfileEnd("EvaSettings.Load()");
            isLoaded = true;
        }
        
        public static void Save()
        {
            if (isLoaded)
            {
                EvaDebug.DebugWarning("OnSave()");
                //ScreenMessages.PostScreenMessage("Saving Kerbals...", 3, ScreenMessageStyle.LOWER_CENTER);
                SaveFunction();

                isLoaded = false;
            }
        }

        public static void SaveFunction()
        {
            EvaDebug.ProfileStart();
            SaveFile();
            EvaDebug.ProfileEnd("EvaSettings.Save()");
        }

        public static void LoadEva(EvaContainer container)
        {

            //EvaDebug.DebugWarning("EvaSettings.LoadEva(" + container.Name + ")");

            //The eva was already has a old save.
            //Load it. 
            if (collection.ContainsKey(container.flightID))
            {
                //string evaString = collection[container.flightID];
                //EvaDebug.DebugWarning(evaString);

                container.FromSave(collection[container.flightID]);
            }
            else
            {
                //No save yet.                
            }
        }
        public static void SaveEva(EvaContainer container){

            //EvaDebug.DebugWarning("EvaSettings.SaveEva(" + container.Name + ")");

            if (container.status == Status.Removed)
            {
                if (collection.ContainsKey(container.flightID))
                {
                    collection.Remove(container.flightID);
                }
            }
            else
            {
                //The eva was already has a old save.
                if (collection.ContainsKey(container.flightID))
                {
                    //Replace the old save.
                    collection[container.flightID] = container.ToSave();
                }
                else
                {
                    //No save yet. Add it now.
                    collection.Add(container.flightID, container.ToSave());
                }
            }
        }

        private static void LoadFile()
        {
            string fileName  = String.Format("Evas-{0}.txt", HighLogic.CurrentGame.Title);
            if (FileExcist(fileName))
            {
                KSP.IO.TextReader tr = KSP.IO.TextReader.CreateForType<EvaSettings>(fileName);

                string file = tr.ReadToEnd();
                tr.Close();

                EvaTokenReader reader = new EvaTokenReader(file);

                //read every eva.
                while (!reader.EOF)
                {
                    //Load all the eva's in the list.
                    LoadEva(reader.NextToken('[', ']'));
                }
            }
        }

        private static void LoadEva(string eva)
        {
            Guid flightID = GetFlightIDFromEvaString(eva);
            collection.Add(flightID, eva);
        }
    

        private static Guid GetFlightIDFromEvaString(string evaString)
        {
            EvaTokenReader reader = new EvaTokenReader(evaString);

            string sflightID = reader.NextTokenEnd(',');

            //Load the eva
            Guid flightID = new Guid(sflightID);
            return flightID;
        }
   

        private static void SaveFile()
        {  
            KSP.IO.TextWriter tw = KSP.IO.TextWriter.CreateForType<EvaSettings>(String.Format("Evas-{0}.txt", HighLogic.CurrentGame.Title));
            
            foreach (var item in collection)
            {
                tw.Write("[" + item.Value + "]");
            }
            
            tw.Close();

            collection.Clear();
        }              
    }
}
