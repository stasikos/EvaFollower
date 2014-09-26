using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml;

namespace MSD.EvaFollower
{
    /// <summary>
    /// The object responsible for Patroling the kerbal.
    /// </summary>
    class EvaPatrol : IEvaControlType
    {

        public bool AllowRunning { get; set; }
        public List<PatrolAction> actions = new List<PatrolAction>();
        public int currentPatrolPoint = 0;
        public string referenceBody = "None";

        private float delta = 0;

        public bool CheckDistance(double sqrDistance)
        {
            bool complete = (sqrDistance < 0.3);

            if (complete)
            {
                PatrolAction currentPoint = actions[currentPatrolPoint];

                if (currentPoint.type == PatrolActionType.Wait)
                {
                    delta += Time.deltaTime;

                    if (delta > currentPoint.delay)
                    {
                        SetNextPoint();
                        delta = 0;
                    }
                }
                else //move
                {
                    SetNextPoint();
                }
            }

            return complete;
        }

        private void SetNextPoint()
        {
            ++currentPatrolPoint;

            if (currentPatrolPoint >= actions.Count)
                currentPatrolPoint = 0;
        }

        public void GetNextTarget(ref Vector3d move)
        {
            PatrolAction currentPoint = actions[currentPatrolPoint];
           
            move += Util.GetWorldPos3DLoad(currentPoint.position);
        }

        public void Move(Vector3d position)
        {
            actions.Add(new PatrolAction(PatrolActionType.Move, 0, position));
#if DEBUG
            setLine(position);
#endif
        }

        public void Wait(Vector3d position)
        {
            actions.Add(new PatrolAction(PatrolActionType.Wait, 1, position));
          
#if DEBUG
            setLine(position);
#endif
        }
#if DEBUG
        private void setLine(Vector3d position)
        {

            lineRenderer.SetVertexCount(actions.Count);
            lineRenderer.SetPosition(actions.Count - 1, Util.GetWorldPos3DLoad(position));

        }
#endif
        public void Clear()
        {
            referenceBody = "None";

            currentPatrolPoint = 0;
            actions.Clear();

#if DEBUG
            lineRenderer.SetVertexCount(0);
#endif

        }

        public void Save(XmlDocument doc, XmlNode node)
        {
            XmlElement el = doc.CreateElement("Patrol");

            XmlAttribute xa1 = doc.CreateAttribute("AllowRunning");
            xa1.Value = AllowRunning.ToString();
            el.Attributes.Append(xa1);

            XmlAttribute xa2 = doc.CreateAttribute("CurrentPatrolPoint");
            xa2.Value = currentPatrolPoint.ToString();
            el.Attributes.Append(xa2);

            XmlAttribute xa3 = doc.CreateAttribute("ReferenceBody");
            xa3.Value = referenceBody.ToString();
            el.Attributes.Append(xa3);
                             

            foreach (var action in actions)
            {
                el.AppendChild(action.SaveNode(doc));
            }

            node.AppendChild(el);
        }

        public void Load(XmlNode node)
        {
                      
            if (node.Attributes["AllowRunning"] != null)
            {
                AllowRunning = bool.Parse(node.Attributes["AllowRunning"].Value);
            }
            if (node.Attributes["CurrentPatrolPoint"] != null)
            {
                currentPatrolPoint = int.Parse(node.Attributes["CurrentPatrolPoint"].Value);
            }
            if (node.Attributes["ReferenceBody"] != null)
            {
                referenceBody = node.Attributes["ReferenceBody"].Value;               
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                PatrolAction p = new PatrolAction();
                p.LoadNode(child);

                actions.Add(p);
            }
            
#if DEBUG
            ///debug
            int index = 0;
            lineRenderer.SetVertexCount(actions.Count-1);
            foreach(var action in actions){
                lineRenderer.SetPosition(index, Util.GetWorldPos3DLoad(action.position));
                ++index;
            }
#endif
        }

#if DEBUG
        LineRenderer lineRenderer;

        public EvaPatrol()
        {
            lineRenderer = new GameObject().AddComponent<LineRenderer>();

            lineRenderer.useWorldSpace = false;
            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.SetWidth(0.05f, 0.05f);
            lineRenderer.SetColors(Color.green, Color.red);

            lineRenderer.renderer.castShadows = false;
            lineRenderer.renderer.receiveShadows = false;
            lineRenderer.renderer.enabled = true;

            lineRenderer.SetVertexCount(0);
        }
#endif

    }

    internal class PatrolAction
    {
        public Vector3d position;
        public PatrolActionType type;
        public int delay = 0;

        public PatrolAction()
        {
            this.type = PatrolActionType.Move;
            this.delay = 10;
            this.position = new Vector3d();
        }

        public PatrolAction(PatrolActionType type, int delay, Vector3d position)
        {
            this.type = type;
            this.delay = delay;
            this.position = position;
        }

        public void LoadNode(XmlNode element)
        {
            if (element.Attributes["Type"] != null)
            {
                this.type = (PatrolActionType)Enum.Parse(typeof(PatrolActionType), element.Attributes["Type"].Value);
            }
            if (element.Attributes["Delay"] != null)
            {
                this.delay = int.Parse(element.Attributes["Delay"].Value);
            }
            if (element.Attributes["Position"] != null)
            {
                this.position = Util.ParseVector3d(element.Attributes["Position"].Value);
            }
        }

        public XmlElement SaveNode(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement("PatrolAction");

            XmlAttribute xa1 = doc.CreateAttribute("Type");
            xa1.Value = type.ToString();
            node.Attributes.Append(xa1);

            XmlAttribute xa2 = doc.CreateAttribute("Delay");
            xa2.Value = ((int)delay).ToString();
            node.Attributes.Append(xa2);

            XmlAttribute xa3 = doc.CreateAttribute("Position");
            xa3.Value = position.ToString();
            node.Attributes.Append(xa3);

            return node;
        }

        public override string ToString()
        {
            return "position = " + position.ToString() + ", delay = " + delay + ", type = " + type.ToString();
        }
    }

    [Flags]
    internal enum PatrolActionType
    {
        Move,
        Wait,
    }
    
}
