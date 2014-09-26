using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using UnityEngine;

namespace MSD.EvaFollower
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class EvaSettings : MonoBehaviour
    {
        public static EvaSettings fetch;

        private bool savedNode = false; 
        private bool nodesLoaded = false;
        private List<XmlNode> buffer = new List<XmlNode>();

        public void Start()
        {
            fetch = this;
        }

        public void Load()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                EvaDebug.ProfileStart();

                EvaDebug.DebugWarning("EvaSettings.Load()");
                LoadConfig();
                nodesLoaded = true;

                EvaDebug.ProfileEnd("LoadConfig()");
            }
        }

        public void Save()
        {
            if (nodesLoaded && !savedNode)
            {
                EvaDebug.ProfileStart();

                EvaDebug.DebugWarning("EvaSettings.Save()");
                SaveConfig();

                EvaDebug.ProfileEnd("SaveConfig()");

                nodesLoaded = false;
                savedNode = true;
            }
        }


        private void LoadConfig()
        {
            try
            {
                KSP.IO.TextReader tr = KSP.IO.TextReader.CreateForType<EvaSettings>(String.Format("Evas-{0}.txt", HighLogic.CurrentGame.Title));
                String strFile = tr.ReadToEnd();
                tr.Close();

                if (string.IsNullOrEmpty(strFile))
                    return;
                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(strFile);
                
                XmlNodeList evas = doc["root"].GetElementsByTagName("EVA");

                foreach (XmlNode evaNode in evas)
                {
                    if (evaNode.Attributes["Id"] != null)
                    {
                        Guid id = new Guid(evaNode.Attributes["Id"].Value);

                        EvaContainer eva = EvaController.fetch.GetEva(id);

                        ReadEva(evaNode, eva);
                    }
                }
                
            }
            catch { }
        }

        public void LoadEvaVesselConfig(Guid id)
        {
            try
            {
                KSP.IO.TextReader tr = KSP.IO.TextReader.CreateForType<EvaSettings>(String.Format("Evas-{0}.txt", HighLogic.CurrentGame.Title));
                String strFile = tr.ReadToEnd();
                tr.Close();

                if (string.IsNullOrEmpty(strFile))
                    return;

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(strFile);

                XmlNodeList evas = doc["root"].GetElementsByTagName("EVA");

                foreach (XmlNode evaNode in evas)
                {
                    if (evaNode.Attributes["Id"] != null)
                    {
                        Guid cid = new Guid(evaNode.Attributes["Id"].Value);

                        if (cid == id)
                        {
                            EvaContainer eva = EvaController.fetch.GetEva(id);

                            if (eva != null)
                                return;

                            ReadEva(evaNode, eva);
                            return;
                        }
                    }
                }

            }
            catch { }
        }

        private void ReadEva(XmlNode evaNode, EvaContainer eva)
        {
            if (!eva.Loaded)
            {
                //Can't load.. Do not remove old data.
                buffer.Add(evaNode);

            }

            if (evaNode.Attributes["Selected"] != null)
            {
                eva.Selected = bool.Parse(evaNode.Attributes["Selected"].Value);
            }
            if (evaNode.Attributes["Mode"] != null)
            {
                eva.Mode = (Mode)Enum.Parse(typeof(Mode), evaNode.Attributes["Mode"].Value);
            }
            if (evaNode.Attributes["HelmetOn"] != null)
            {
                eva.HelmetOn = bool.Parse(evaNode.Attributes["HelmetOn"].Value);
                eva.EVA.ShowHelmet(eva.HelmetOn);
            }


            foreach (XmlNode child in evaNode.ChildNodes)
            {
                switch (child.Name)
                {
                    case "Formations": { eva.Formation.Load(child); } break;
                    case "Patrol": { eva.Patrol.Load(child); } break;
                    case "Order": { eva.Order.Load(child); } break;
                }
            }
        }

        private void SaveConfig()
        {
            KSP.IO.TextWriter tw = KSP.IO.TextWriter.CreateForType<EvaSettings>(String.Format("Evas-{0}.txt", HighLogic.CurrentGame.Title));


            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("root");

            foreach (var eva in EvaController.fetch.Collection)
            {
                if (eva.Loaded)
                {
                    SaveEva(doc, root, eva);
                }
                else
                {
                    foreach (var b in buffer)
                    {                       
                        root.AppendChild(doc.ImportNode(b,true));
                    }
                }
            }
            doc.AppendChild(root);

            string document = "";
            using (var stringWriter = new StringWriter())
            {
                using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                {
                    doc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
                    document = stringWriter.GetStringBuilder().ToString();
                }
            }
            tw.Write(document);
            tw.Close();

            buffer.Clear();
        }

        private void SaveEva(XmlDocument doc, XmlNode root, EvaContainer eva)
        {
            root.AppendChild(doc.CreateWhitespace("\r\n"));
            XmlElement element = doc.CreateElement("EVA");

            XmlAttribute xa0 = doc.CreateAttribute("Id");
            xa0.Value = eva.FlightID.ToString();
            element.Attributes.Append(xa0);

            XmlAttribute xa1 = doc.CreateAttribute("Selected");
            xa1.Value = eva.Selected.ToString();
            element.Attributes.Append(xa1);

            XmlAttribute xa2 = doc.CreateAttribute("Mode");
            xa2.Value = eva.Mode.ToString();
            element.Attributes.Append(xa2);

            XmlAttribute xa3 = doc.CreateAttribute("HelmetOn");
            xa3.Value = eva.HelmetOn.ToString();
            element.Attributes.Append(xa3);

            //Formations
            eva.Formation.Save(doc, element);

            //Patrols     
            eva.Patrol.Save(doc, element);

            //Order
            eva.Order.Save(doc, element);

            root.AppendChild(element);
            root.AppendChild(doc.CreateWhitespace("\r\n"));
        }
    }
}
