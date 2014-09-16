using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MSD.EvaFollower
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class EvaController : MonoBehaviour
    {
        public static EvaController fetch;

        //
        public static Mesh helmetMesh = null;
        public static Mesh visorMesh = null;
               
        private List<EvaContainer> _evaCollection = new List<EvaContainer>();       
        private Dictionary<string, LineRenderer> selectionLines = new Dictionary<string, LineRenderer>();
        private LineRenderer _cursor = new LineRenderer();

        //Selection variables
        private Texture2D _selectionHighlight = new Texture2D(200,200);
        private Rect _selection = new Rect(0, 0, 0, 0);
        private Vector3 _startClick = -Vector3.one;

        //Cursor variables
        private bool showCursor = false;
        private RaycastHit _cursorHit = new RaycastHit();
        private Vector3 _cursorPosition;
        private Quaternion _cursorRotation;
        private bool _animatedCursor = false;
        private float _animatedCursorValue = 0;
        
        //Should be bindable. 
        private int _selectMouseButton = 0;
        private int _dispatchMouseButton = 2;
        
        //animation variable
        double angle = 0;

        /// <summary>
        /// Runs when the object starts.
        /// </summary>
        public void Start()
        {
            try
            {
                EvaDebug.DebugWarning("Start() Initialized.");

                fetch = this;
                GameEvents.onFlightReady.Add(new EventVoid.OnEvent(onFlightReadyCallback));
                GameEvents.onCrewOnEva.Add(new EventData<GameEvents.FromToAction<Part, Part>>.OnEvent(OnCrewOnEva));
                GameEvents.onCrewBoardVessel.Add(new EventData<GameEvents.FromToAction<Part, Part>>.OnEvent(OnCrewBoardVessel));
                GameEvents.onCrewKilled.Add(new EventData<EventReport>.OnEvent(OnCrewKilled));

                InitializeMeshes();
                InitializeCursor();
            }
            catch
            {
                EvaDebug.DebugWarning("Start() failed");
            }

        }

        private void InitializeMeshes()
        {
            // Save pointer to helmet & visor meshes so helmet removal can restore them.
            foreach (SkinnedMeshRenderer smr
                     in Resources.FindObjectsOfTypeAll(typeof(SkinnedMeshRenderer)))
            {
                if (smr.name == "helmet")
                    helmetMesh = smr.sharedMesh;
                else if (smr.name == "visor")
                    visorMesh = smr.sharedMesh;
            }
        }

 
        public void Awake()
        {
                         
        }


        public void Destroy()
        {
            
        }
       
        /// <summary>
        /// Load the list 
        /// </summary>
        private void onFlightReadyCallback()
        {            
#if DEBUG
            EvaDebug.DebugWarning("onFlightReadyCallback()");
#endif
            FetchEVAS();
        }

        /// <summary>
        /// Runs when the EVA is killed.
        /// </summary>
        /// <param name="report"></param>
        public void OnCrewKilled(EventReport report)
        {
            EvaDebug.DebugWarning(report.sender + " is dead!");
            RemoveEva(report.origin.flightID);
        }

        /// <summary>
        /// Runs when the EVA goes onboard a vessel.
        /// </summary>
        /// <param name="e"></param>
        public void OnCrewBoardVessel(GameEvents.FromToAction<Part, Part> e)
        {
            //remove kerbal
            uint flightID = e.from.flightID;

            RemoveEva(flightID);

        }

        /// <summary>
        /// Runs when the kerbal goes on EVA.
        /// </summary>
        /// <param name="e"></param>
        public void OnCrewOnEva(GameEvents.FromToAction<Part, Part> e)
        {
            //add new kerbal
            uint flightID = e.to.flightID;

            if (!ListContains(flightID))
            {
                EvaContainer kerbal = new EvaContainer(e.to.vessel);
                AddEva(kerbal);
            }
        }

        /// <summary>
        /// Fetch all evas in the universe.
        /// </summary>
        private void FetchEVAS()
        {
            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                if (!vessel.isEVA)
                    continue;

                uint flightID = vessel.parts[0].flightID;

#if DEBUG
                EvaDebug.DebugLog("Added EVA: " + vessel.name);
#endif 
                if (!ListContains(flightID))
                {
                    EvaContainer kerbal = new EvaContainer(vessel);
                    AddEva(kerbal);
                }
            }
        }

        /// <summary>
        /// Check if the current list contains the flight id.
        /// </summary>
        /// <param name="flightID"></param>
        /// <returns></returns>
        private bool ListContains(uint flightID)
        {
            for (int i = 0; i < _evaCollection.Count; i++)
            {
                if (_evaCollection[i].FlightID == flightID)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Add an EVA to the list.
        /// </summary>
        /// <param name="kerbal"></param>
        private void AddEva(EvaContainer kerbal)
        {
            _evaCollection.Add(kerbal);
        }

        /// <summary>
        /// Remove an EVA from the list.
        /// </summary>
        /// <param name="flightID"></param>
        private void RemoveEva(uint flightID)
        {
            int deleteID = -1;
            for (int i = _evaCollection.Count - 1; i >= 0; i--)
            {
                if (_evaCollection[i].FlightID == flightID)
                {
                    deleteID = i;                   
                }

                if (_evaCollection[i].Formation.Leader != null)
                {
                    if (_evaCollection[i].Formation.Leader.FlightID == flightID)
                    {
                        _evaCollection[i].Formation.Leader = null;
                        _evaCollection[i].Mode = Mode.None;
                    }
                }
            }
                  
            if(deleteID >= 0)
                _evaCollection.RemoveAt(deleteID);
        }

        /// <summary>
        /// Get an EVA from the collection.
        /// </summary>
        /// <param name="flightID"></param>
        /// <returns></returns>
        public EvaContainer GetEva(uint flightID)
        {
            for (int i = _evaCollection.Count - 1; i >= 0; i--)
            {
                if (_evaCollection[i].FlightID == flightID)
                    return _evaCollection[i];
            }
            return null;
        }

        /// <summary>
        /// Initialize the data for the target positioning.
        /// </summary>
        private void InitializeCursor()
        {
            _cursor = new GameObject().AddComponent<LineRenderer>();

            _cursor.useWorldSpace = false;
            _cursor.material = new Material(Shader.Find("Particles/Additive"));
            _cursor.SetWidth(0.05f, 0.05f);
            _cursor.SetColors(Color.green, Color.green);

            _cursor.renderer.enabled = false;
            _cursor.renderer.castShadows = false;
            _cursor.renderer.receiveShadows = false;

            int segments = 32;
            _cursor.SetVertexCount(segments);

            CreateCircle(_cursor, segments, 0.125);

        }

        public void Update()
        {
            if (!FlightGlobals.ready || PauseMenu.isOpen)
                return;

            angle += 0.1;


            int selectedKerbals = 0;
            double geeForce = FlightGlobals.currentMainBody.GeeASL;
            for (int i = _evaCollection.Count - 1; i >= 0; i--)
            {
                EvaContainer v = _evaCollection[i];

                if (v.Mode == Mode.None)
                    continue;

                #region List Cleanup
                if (v == null)
                {
                    //This will results in errors.. deleted at the end. 
                    _evaCollection.RemoveAt(i); continue;
                }
                if (v.EVA.part == null || v.EVA == null)
                {
                    _evaCollection.RemoveAt(i); continue;
                }
                #endregion

                #region Update EVA list

                if (v.Selected)
                {
                    UpdateSelectionLine(v.EVA);
                }

                //Reset after lost contact, leader is death or gone. 
                if (v.Mode == Mode.None)
                {
                    v.Animate(AnimationState.Idle, false);
                    continue;
                }


                v.Update(geeForce);

                #endregion

            }

            #region Cursor Visible...
            //Show the cursor if more than one kerbal is selected. 
            if (selectedKerbals > 0)
            {
                ShowCursor();
            }
            else
            {
                DisableCursor();
            }
            #endregion
            
            var activeVessel = FlightGlobals.ActiveVessel;
            var activeEVA = activeVessel.GetComponent<KerbalEVA>();

            //unit vectors in the up (normal to planet surface), east, and north (parallel to planet surface) directions
            //Vector3 eastUnit = activeVessel.mainBody.getRFrmVel(activeVessel.transform.position).normalized; //uses the rotation of the body's frame to determine "east"
            //Vector3 upUnit = (activeVessel.transform.position - activeVessel.mainBody.position).normalized;
            //Vector3 northUnit = Vector3d.Cross(upUnit, eastUnit); //north = up cross east

            if (!activeVessel.parts[0].GroundContact)
            {
                // No point.. Change this if you get jetpack working.
                return;
            }

            #region Handle Cursor...
            if (showCursor)
            {
                if (!_animatedCursor)
                {
                    //ray every time ?
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _cursorHit))
                    {
                        _cursorPosition = _cursorHit.point;
                        _cursorRotation = activeVessel.transform.rotation;
                        //Quaternion.eastUnit, upUnit, northUnit) ;
                    }
                }

                SetCursorProperties();
            }
            #endregion
            #region Handle Orders

            //Drag rectangle...
            #region Rectangle Handler

            if (Input.GetMouseButtonDown(_selectMouseButton))
            {
                _startClick = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(_selectMouseButton))
            {
                if (_selection.width < 0)
                {
                    _selection.x += _selection.width;
                    _selection.width = -_selection.width;
                }
                if (_selection.height < 0)
                {
                    _selection.y += _selection.height;
                    _selection.height = -_selection.height;
                }

                _startClick = -Vector3.one;
            }

            if (Input.GetMouseButton(_selectMouseButton))
            {
                _selection = new Rect(_startClick.x, InvertY(_startClick.y),
                    Input.mousePosition.x - _startClick.x, InvertY(Input.mousePosition.y) - InvertY(_startClick.y));
            }

            if (Input.GetMouseButton(_selectMouseButton))
            {
                if (_selection.width != 0 && _selection.height != 0)
                {

                    //get the kerbals in the selection.
                    foreach (EvaContainer eva in _evaCollection)
                    {
                        Vector3 camPos = Camera.main.WorldToScreenPoint(eva.EVA.transform.position);
                        camPos.y = InvertY(camPos.y);

                        EvaDebug.DebugLog("Selection: [" + _selection.x + ", " + _selection.y + ", " + _selection.width + ", " + _selection.height + "]");

                        if (_selection.Contains(camPos))
                        {
                            SelectEva(eva);
                        }
                        else
                        {
                            DeselectEva(eva);
                        }
                    }
                }
            }
            

            bool leftButton = Input.GetMouseButtonDown(_selectMouseButton);
            bool rightButton = Input.GetMouseButtonDown(_dispatchMouseButton);

            if (leftButton == false && rightButton == false)
            {
                return;
            }
#if DEBUG
            EvaDebug.DebugLog("Check Collision...");
#endif

            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

            if (!hit)
                return; //nothing to check.

            var evaCollision = hitInfo.transform.gameObject.GetComponent<KerbalEVA>();

            #endregion

            #region Handle Mouse Controls
            if (Input.GetMouseButtonDown(_selectMouseButton)) //Left button.)
            {  
                if (evaCollision != null)
                {
                    //deselect all others.
                    foreach (EvaContainer eva in _evaCollection)
                    {
                        if (eva.Selected)
                            DeselectEva(eva);
                    }

                    EvaContainer _eva = GetEva(evaCollision.part.flightID);
#if DEBUG
                        EvaDebug.DebugLog("Select: " + _eva.EVA.name);
#endif
                    SelectEva(_eva);

                }
            }

            if (Input.GetMouseButtonDown(_dispatchMouseButton)) //Middle button.
            {
                var position = (Vector3d)hitInfo.point;


#if DEBUG
                EvaDebug.DebugLog("World Position: " + position);
#endif
                for (int j = 0; j < _evaCollection.Count; j++)
                {
                    if (_evaCollection[j].Selected)
                    {
                        _evaCollection[j].Order.Move(position);
                        _evaCollection[j].Selected = false;
                        _evaCollection[j].Mode = Mode.Order;

                        _animatedCursor = true;

                        //destroy circle line
                        DestroyLine(_evaCollection[j].EVA);
                    }
                }
            }

            #endregion

            #endregion
        }

        /// <summary>
        /// Deselect an EVA, and remove the selection from the line collection.
        /// </summary>
        /// <param name="_eva"></param>
        private void DeselectEva(EvaContainer _eva)
        {
            _eva.Selected = false;

            //create circle line
            DestroyLine(_eva.EVA);
        }


        /// <summary>
        /// Select an EVA, and add the selection to the line collection.
        /// </summary>
        /// <param name="_eva"></param>
        private void SelectEva(EvaContainer _eva)
        {
            _eva.Selected = true;

            //create circle line
            CreateLine(_eva.EVA);
        }

        public static float InvertY(float y)
        {
            return Screen.height - y;
        }

        private void OnGUI()
        {
            if (_startClick != -Vector3.one)
            {
                GUI.color = new Color(1, 1, 1, 0.15f);      
                GUI.DrawTexture(_selection, _selectionHighlight);
            }
            
        }


        public void UpdateSelectionLine(KerbalEVA eva)
        {
            var lineRenderer = selectionLines[eva.name];
            SetSelectionLineProperties(eva, lineRenderer);            
        }

        private void SetSelectionLineProperties(KerbalEVA eva, LineRenderer lineRenderer)
        {
            double v = 1.5 + Math.Sin(angle) * 0.25;
            var p = eva.transform.position;
            var r = eva.transform.rotation;
            var f = eva.transform.forward;

            lineRenderer.transform.localScale = new Vector3((float)v, (float)v, (float)v);
            lineRenderer.transform.rotation = r * Quaternion.Euler(0,(float)(angle*60.0),0);;
            lineRenderer.transform.position = p;
        }
        private void SetCursorProperties()
        {
            double v = 1.5 + Math.Sin(angle) * 0.25 + Math.Sin(_animatedCursorValue)*2;
            
            _cursor.transform.localScale = new Vector3((float)v, (float)v, (float)v);
            _cursor.transform.rotation = _cursorRotation;
            _cursor.transform.position = _cursorPosition;
            
            SetCursorAnimation();
        }

        private void SetCursorAnimation()
        {
            //little hackish... 
            if (_animatedCursor)
            {
                _animatedCursorValue += (float)(Math.PI / 8.0f);

                Color c = new Color(_animatedCursorValue / 8.0f, 1.0f - _animatedCursorValue / 8.0f, 0);
                _cursor.SetColors(c, c);

                //show the animation and close the cursor 
                if (_animatedCursorValue > 20)
                {
                    DisableCursor();
                }
            }
        }

        private void ShowCursor()
        {
            showCursor = true;
            _cursor.renderer.enabled = true;
        }

        private void DisableCursor()
        {
            showCursor = false;
            _cursor.renderer.enabled = false;
            _cursor.SetColors(Color.green, Color.green);
            _animatedCursor = false;
            _animatedCursorValue = 0;
        }

        private void DestroyLine(KerbalEVA eva)
        {
#if DEBUG
            EvaDebug.DebugLog("DestroyLine(" + eva.name + ")");
#endif

            if (selectionLines.ContainsKey(eva.name))
            {
                Destroy(selectionLines[eva.name]);//destroy object
                selectionLines.Remove(eva.name); // and throw away the key.
            }
        }

        private void CreateLine(KerbalEVA eva)
        {
            if (selectionLines.ContainsKey(eva.name))
            {
                return;
            }
#if DEBUG
            EvaDebug.DebugLog("CreateLine(" + eva.name + ")");
#endif
            LineRenderer lineRenderer = new GameObject().AddComponent<LineRenderer>();

            lineRenderer.useWorldSpace = false;
            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.SetWidth(0.05f, 0.05f);
            lineRenderer.SetColors(Color.green, Color.red);

            lineRenderer.renderer.castShadows = false;
            lineRenderer.renderer.receiveShadows = false;

            int segments = 32;

            lineRenderer.SetVertexCount(segments);

            CreateCircle(lineRenderer, segments, 0.25);

            //set properties
            SetSelectionLineProperties(eva, lineRenderer);

            selectionLines.Add(eva.name, lineRenderer);

        }

        private void CreateCircle(LineRenderer line, int segments, double width)
        {
            double step = ((Math.PI * 2) / ((float)segments - 1));
            for (int i = 0; i < segments; ++i)
            {
                line.SetPosition(i,
                    new Vector3(
                    (float)(Math.Cos(step * i) * width),
                    -0.1f,
                    (float)(Math.Sin(step * i) * width)
                ));
            }
        }
    }
}
