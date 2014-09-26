using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MSD.EvaFollower
{
    [KSPAddon(KSPAddon.Startup.Flight, true)]
    class EvaController : MonoBehaviour
    {
        public static EvaController fetch;
                
        private List<EvaContainer> _evaCollection = new List<EvaContainer>();       
        private Dictionary<string, LineRenderer> selectionLines = new Dictionary<string, LineRenderer>();
        private LineRenderer _cursor = new LineRenderer();

        //Selection variables
        private bool gameUIToggle = true;
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
        private int selectedKerbals = 0;
        
        //Should be bindable. 
        private int _selectMouseButton = 0;
        private int _dispatchMouseButton = 2;
        
        //animation variable
        double angle = 0;
        
        public List<EvaContainer> Collection { get { return _evaCollection; } }

        /// <summary>
        /// Runs when the object starts.
        /// </summary>
        public void Start()
        {
            try
            {
                //EvaDebug.DebugWarning("Start() Initialized.");

                fetch = this;
                GameEvents.onFlightReady.Add(onFlightReadyCallback);
                GameEvents.onCrewOnEva.Add(OnCrewOnEva);
                GameEvents.onCrewBoardVessel.Add(OnCrewBoardVessel);
                GameEvents.onCrewKilled.Add(OnCrewKilled);
                GameEvents.onVesselLoaded.Add(OnVesselLoaded);
                GameEvents.onGameStateSave.Add(OnSave);
                GameEvents.onVesselSituationChange.Add(OnVesselSituationChange);
                
                InitializeCursor();
            }
            catch
            {
                EvaDebug.DebugWarning("Start() failed");
            }

        }

        public void OnVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> action)
        {
            ReloadVessel(action.host);
        }

        
        public void OnDestroy()
        {
            try
            {
                //EvaDebug.DebugWarning("Destroy() Initialized.");

                GameEvents.onFlightReady.Remove(onFlightReadyCallback);
                GameEvents.onCrewOnEva.Remove(OnCrewOnEva);
                GameEvents.onCrewBoardVessel.Remove(OnCrewBoardVessel);               
                GameEvents.onCrewKilled.Remove(OnCrewKilled);
                GameEvents.onVesselLoaded.Remove(OnVesselLoaded);
                GameEvents.onGameStateSave.Remove(OnSave);
                GameEvents.onVesselSituationChange.Remove(OnVesselSituationChange);
            }
            catch
            {
                EvaDebug.DebugWarning("Destroy() failed");
            }
        }
        
        public void OnSave(ConfigNode node)
        {
            EvaSettings.fetch.Save();
        }
         

        /// <summary>
        /// Load the list 
        /// </summary>
        private void onFlightReadyCallback()
        {
            FetchEVAS();
            EvaSettings.fetch.Load();
        }

        /// <summary>
        /// Runs when the EVA is killed.
        /// </summary>
        /// <param name="report"></param>
        public void OnCrewKilled(EventReport report)
        {
            //EvaDebug.DebugWarning(report.sender + " is dead!");
            DestroyLine("kerbalEVA ("+report.sender+")");
        }

        /// <summary>
        /// Runs when the EVA goes onboard a vessel.
        /// </summary>
        /// <param name="e"></param>
        public void OnCrewBoardVessel(GameEvents.FromToAction<Part, Part> e)
        {
            //remove kerbal
            Guid flightID = e.from.vessel.id;

            RemoveEva(flightID);
        }

        /// <summary>
        /// Runs when the kerbal goes on EVA.
        /// </summary>
        /// <param name="e"></param>
        public void OnCrewOnEva(GameEvents.FromToAction<Part, Part> e)
        {
            //add new kerbal
            Guid flightID = e.to.vessel.id;

            if (!ListContains(flightID))
            {
                EvaContainer kerbal = new EvaContainer(e.to.vessel);
                AddEva(kerbal);
            }
        }

        public void OnVesselLoaded(Vessel vessel)
        {
            ReloadVessel(vessel);
        }

        private void ReloadVessel(Vessel vessel)
        {
            //skip if not a kerbal.
            if (!vessel.isEVA)
                return;

            if (ListContains(vessel.id))
            {
                EvaContainer kerbal = GetEva(vessel.id);

                if (kerbal != null) //make sure the list is loaded.
                {
                    kerbal.Reload(vessel);
                    EvaSettings.fetch.LoadEvaVesselConfig(vessel.id);
                }
            }
            else
            {
                EvaContainer kerbal = new EvaContainer(vessel);
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
                
                Guid flightID = vessel.id;

                if (vessel.loaded)
                {
                    //Loaded
                    if (!ListContains(flightID))
                    {
                        EvaContainer kerbal = new EvaContainer(vessel);
                        AddEva(kerbal);
                    }
                }
                else
                {
                    //Unloaded
                    if (!ListContains(vessel.id))
                    {
                        EvaContainer kerbal = new EvaContainer(vessel);
                        AddEva(kerbal);
                    }
                 }
            }
        }

        /// <summary>
        /// Check if the current list contains the flight id.
        /// </summary>
        /// <param name="flightID"></param>
        /// <returns></returns>
        private bool ListContains(Guid flightID)
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
        private void RemoveEva(Guid flightID)
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
            
            if (deleteID >= 0)
            {
                DeselectEva(_evaCollection[deleteID]);
                _evaCollection.RemoveAt(deleteID);
            }
        }

        /// <summary>
        /// Get an EVA from the collection.
        /// </summary>
        /// <param name="flightID"></param>
        /// <returns></returns>
        public EvaContainer GetEva(Guid flightID)
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

        
        /// <summary>
        /// Update the list of kerbals.
        /// </summary>
        public void Update()
        {
            if (!FlightGlobals.ready || PauseMenu.isOpen)
                return;

          
            angle += 0.1;
          
            var activeVessel = FlightGlobals.ActiveVessel;
            var activeEVA = activeVessel.GetComponent<KerbalEVA>();

            double geeForce = FlightGlobals.currentMainBody.GeeASL;
            
            //_evaCollection.RemoveAll(e => e == null);

            for (int i = _evaCollection.Count - 1; i >= 0; i--)
            {
                EvaContainer v = _evaCollection[i];
    
                #region Skip death objects.

                //Should never happen.. Or beat me.
                if (v == null)
                {
                    EvaDebug.DebugLog("V == null");
                    continue;
                }
        
                //This list contains eva's that are not loaded (out of bounds)
                //Just ignore them. 
                if (!v.Loaded){continue;}

                //Vessel unloaded
                if (v.EVA == null)
                {
                    v.Loaded = false;
                    continue;
                }

                #endregion

                #region Don't wast any time.
                 
                if (v.Selected){
                    UpdateSelectionLine(v.EVA);
                }
                                
                // Unstable
                if (activeEVA != null)
                {
                    if (v.EVA != activeEVA)
                    {
                        v.UpdateLamps();
                    }
                }

                if (v.Mode == Mode.None){continue;}

                #endregion

                #region Update EVA list

                v.Update(geeForce);

                #endregion

            }

            
            if (!activeVessel.Landed || activeVessel.GetHeightFromSurface() > 15)
            {
                DisableCursor();
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
                        if (!eva.Loaded)
                        {
                            //Can't select him.
                            continue;
                        }

                        Vector3 camPos = Camera.main.WorldToScreenPoint(eva.EVA.transform.position);
                        camPos.y = InvertY(camPos.y);

                        if (_selection.Contains(camPos))
                        {
                            SelectEva(eva);
                        }
                        else
                        {
                            if(eva.Selected)
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
            
            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

            if (!hit)
            {
                DisableCursor();
                return; //nothing to check.
            }

            var evaCollision = hitInfo.transform.gameObject.GetComponent<KerbalEVA>();

            #endregion

            #region Handle Mouse Controls
            if (Input.GetMouseButtonDown(_selectMouseButton)) //Left button.)
            {
                if (evaCollision != null)
                {
                    DeselectAllKerbals();

                    EvaContainer _eva = GetEva(evaCollision.vessel.id);
                    SelectEva(_eva);

                }
                else
                {
                    DeselectAllKerbals();

                    DisableCursor();
                }
            }

            if (Input.GetMouseButtonDown(_dispatchMouseButton)) //Middle button.
            {
                var position = (Vector3d)hitInfo.point;

                //substract active vessel
                position -= activeVessel.GetWorldPos3D();

                for (int j = 0; j < _evaCollection.Count; j++)
                {
                    if (!_evaCollection[j].Loaded)
                        continue;

                    if (_evaCollection[j].Selected)
                    {
                        //Remove current mode.
                        if (_evaCollection[j].Mode == Mode.Patrol)
                        {
                            _evaCollection[j].Patrol.Clear();
                        }

                        _evaCollection[j].Order.Move(position, Util.GetWorldPos3DSave( activeVessel ));
                        _evaCollection[j].Selected = false;
                        _evaCollection[j].Mode = Mode.Order;

                        _animatedCursor = true;

                        //destroy circle line
                        DestroyLine(_evaCollection[j].EVA.name);
                    }
                }
            }

            #endregion

            #endregion

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
        }

        private void DeselectAllKerbals()
        {
            //deselect all kerbals.
            foreach (EvaContainer eva in _evaCollection)
            {
                if (!eva.Loaded)
                    continue;

                if (eva.Selected)
                    DeselectEva(eva);
            }
        }

        /// <summary>
        /// Deselect an EVA, and remove the selection from the line collection.
        /// </summary>
        /// <param name="_eva"></param>
        private void DeselectEva(EvaContainer _eva)
        {
            --selectedKerbals;
            _eva.Selected = false;

            //create circle line
            DestroyLine(_eva.EVA.name);
        }


        /// <summary>
        /// Select an EVA, and add the selection to the line collection.
        /// </summary>
        /// <param name="_eva"></param>
        private void SelectEva(EvaContainer _eva)
        {
            ++selectedKerbals;
            _eva.Selected = true;

            //create circle line
            CreateLine(_eva.EVA);
        }

        public static float InvertY(float y)
        {
            return Screen.height - y;
        }

        /// <summary>
        /// Draw the selection box.
        /// </summary>
        private void OnGUI()
        {
            if (gameUIToggle)
            {
                if (_startClick != -Vector3.one && _selection.width != 0 && _selection.height != 0)
                {
                    GUI.color = new Color(1, 1, 1, 0.15f);
                    GUI.DrawTexture(_selection, _selectionHighlight);
                }
            }
            
        }

        void GameUIEnable()
        {
            gameUIToggle = true;
        }

        void GameUIDisable()
        {
            gameUIToggle = false;
        }

        /// <summary>
        /// Update the selection model position.
        /// </summary>
        /// <param name="eva"></param>
        public void UpdateSelectionLine(KerbalEVA eva)
        {
            if (!selectionLines.ContainsKey(eva.name))
            {
                CreateLine(eva);
            }

            var lineRenderer = selectionLines[eva.name];
            SetSelectionLineProperties(eva, lineRenderer);            
        }

        /// <summary>
        /// Set the position of the selector. 
        /// </summary>
        /// <param name="eva"></param>
        /// <param name="lineRenderer"></param>
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

        /// <summary>
        /// Set the properties of the target cursor.
        /// </summary>
        private void SetCursorProperties()
        {
            double v = 1.5 + Math.Sin(angle) * 0.25 + Math.Sin(_animatedCursorValue)*2;
            
            _cursor.transform.localScale = new Vector3((float)v, (float)v, (float)v);
            _cursor.transform.rotation = _cursorRotation;
            _cursor.transform.position = _cursorPosition;
            
            SetCursorAnimation();
        }

        /// <summary>
        /// Display a little animation when target is clicked. 
        /// </summary>
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

        /// <summary>
        /// Enable the cursor for rendering.
        /// </summary>
        private void ShowCursor()
        {
            showCursor = true;
            _cursor.renderer.enabled = false;
        }

        /// <summary>
        /// Disable the cursor for rendering.
        /// </summary>
        private void DisableCursor()
        {
            showCursor = false;
            _cursor.renderer.enabled = false;

            _cursor.SetColors(Color.green, Color.green);
            _animatedCursor = false;
            _animatedCursorValue = 0;
        }

        /// <summary>
        /// Destroy the selection model for a specific kerbal.
        /// </summary>
        /// <param name="eva"></param>
        private void DestroyLine(string evaName)
        {
            if (selectionLines.ContainsKey(evaName))
            {
                Destroy(selectionLines[evaName]);//destroy object
                selectionLines.Remove(evaName); // and throw away the key.
            }
        }

        /// <summary>
        /// Create a selection model for a specific kerbal.
        /// </summary>
        /// <param name="eva"></param>
        private void CreateLine(KerbalEVA eva)
        {
            if (selectionLines.ContainsKey(eva.name))
            {
                return;
            }

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

        /// <summary>
        /// Create the circle model. 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="segments"></param>
        /// <param name="width"></param>
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
