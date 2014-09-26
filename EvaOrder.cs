using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml;

namespace MSD.EvaFollower
{
    /// <summary>
    /// The object responsible for ordering the kerbal around. 
    /// </summary>
    class EvaOrder : IEvaControlType
    {

        private Vector3d Offset = -Vector3d.one;
        public bool AllowRunning { get; set; }
        private Vector3d position;

        public EvaOrder()
        {
            AllowRunning = true;
        }

        public bool CheckDistance(double sqrDistance)
        {
            bool complete = (sqrDistance < 0.8);
            
            return complete;
        }

        public void GetNextTarget(ref Vector3d move)
        {
            move += (Util.GetWorldPos3DLoad(Offset) + position);
        }

        public void Move(Vector3d pos, Vector3d off)
        {
            this.Offset = off;
            this.position = pos;
        }

        public void Save(XmlDocument doc, XmlNode node)
        {
            XmlElement el = doc.CreateElement("Order");

            XmlAttribute xa1 = doc.CreateAttribute("AllowRunning");
            xa1.Value = AllowRunning.ToString();
            el.Attributes.Append(xa1);

            XmlAttribute xa2 = doc.CreateAttribute("Position");
            xa2.Value = position.ToString();
            el.Attributes.Append(xa2);

            XmlAttribute xa3 = doc.CreateAttribute("Offset");
            xa3.Value = Offset.ToString();
            el.Attributes.Append(xa3);


            node.AppendChild(el);
        }

        public void Load(XmlNode node)
        {
            if (node.Attributes["AllowRunning"] != null)
            {
                this.AllowRunning = bool.Parse(node.Attributes["AllowRunning"].Value);
            }
            if (node.Attributes["Position"] != null)
            {
                this.position = Util.ParseVector3d(node.Attributes["Position"].Value);
            }
            if (node.Attributes["Offset"] != null)
            {
                this.Offset = Util.ParseVector3d(node.Attributes["Offset"].Value);
            }              
        }
    }
}
