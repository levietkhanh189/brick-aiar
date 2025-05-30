﻿using Battlehub.RTCommon;
using UnityEngine;
using UnityEngine.UIElements;

namespace Battlehub.RTHandles
{
    public class RuntimeSceneInput : RuntimeSelectionInput
    {
        public KeyCode FocusKey = KeyCode.F;
        public KeyCode FocusActiveKey = KeyCode.LeftShift;
        public KeyCode SnapToGridKey = KeyCode.G;
        public KeyCode SnapToGridKey2 = KeyCode.LeftShift;
        public KeyCode RotateKey = KeyCode.LeftAlt;
        public KeyCode RotateKey2 = KeyCode.RightAlt;
        public KeyCode RotateKey3 = KeyCode.AltGr;
        public KeyCode FreeMoveZoomKey = KeyCode.LeftAlt;
        public KeyCode FreeMoveZoomKey2 = KeyCode.RightAlt;
        public KeyCode FreeMoveZoomKey3 = KeyCode.AltGr;
        public KeyCode MoveDownKey = KeyCode.Q;
        public KeyCode MoveUpKey = KeyCode.E;
        public MouseButton PanButton = MouseButton.MiddleMouse;

        public float RotateXSensitivity = 5.0f;
        public float RotateYSensitivity = 5.0f;
        public float MoveZSensitivity = 1.0f;
        public float FreeMoveSensitivity = 1.0f;
        public float FreeZoomSensitivity = 1.0f;
        public float FreeRotateSensitivity = 5.0f;

        public bool SwapLRMB = false;

        [SerializeField]
        private bool m_beginRotateImmediately = true;
        protected bool BeginRotateImmediately
        {
            get { return m_beginRotateImmediately; }
            set { m_beginRotateImmediately = value; }
        }

        [SerializeField]
        private bool m_beginFreeMoveImmediately = true;
        protected bool BeginFreeMoveImmediately
        { 
            get { return m_beginFreeMoveImmediately; }
            set { m_beginFreeMoveImmediately = value; }
        }

        private bool m_rotate;
        private bool m_rotateActive;
        private bool m_pan;
        private bool m_freeMove;
        private bool m_freeMoveActive;
        private bool m_isActive;

        protected RuntimeSceneComponent SceneComponent
        {
            get { return (RuntimeSceneComponent)m_component; }
        }

        protected virtual bool AllowRotateAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetPointer(SwapLRMB ? 1 : 0);
        }

        protected virtual bool RotateAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetKey(RotateKey) ||
                input.GetKey(RotateKey2) ||
                input.GetKey(RotateKey3);
        }

        protected virtual bool FreeMoveZoomAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetKey(RotateKey) ||
                input.GetKey(RotateKey2) ||
                input.GetKey(RotateKey3);
        }

        protected virtual bool PanAction()
        {
            IInput input = m_component.Editor.Input;
            RuntimeTools tools = m_component.Editor.Tools;
            return input.GetPointer((int)PanButton) || input.GetPointer(SwapLRMB ? 1 : 0) && tools.Current == RuntimeTool.View && tools.ActiveTool == null;
        }

        protected virtual bool FreeMoveAction()
        {
            IInput input = m_component.Editor.Input;
            
            if (SwapLRMB)
            {
                return input.GetPointer(0) && RotateAction();
            }
            return input.GetPointer(1);
        }

        protected virtual bool FocusAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetKeyDown(FocusKey);
        }

        protected virtual bool FocusActiveAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetKey(FocusActiveKey);
        }

        protected virtual bool SnapToGridAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetKeyDown(SnapToGridKey) && input.GetKey(SnapToGridKey2);
        }

        protected virtual Vector2 RotateAxes()
        {
            IInput input = m_component.Editor.Input;
            float deltaX = input.GetAxis(InputAxis.X);
            float deltaY = input.GetAxis(InputAxis.Y);
            return new Vector2(deltaX, deltaY);
        }

        protected virtual float DeltaTime()
        {
            return Time.unscaledDeltaTime * 50;
        }

        protected virtual float ZoomAxis()
        {
            IInput input = m_component.Editor.Input;
            float deltaZ = input.GetAxis(InputAxis.Z) * DeltaTime();
            return deltaZ;
        }

        protected virtual Vector3 MoveAxes()
        {
            IInput input = m_component.Editor.Input;

            float deltaTime = DeltaTime();
            float deltaX = input.GetAxis(InputAxis.HorizontalRaw) * deltaTime;
            float deltaY = input.GetAxis(InputAxis.VerticalRaw) * deltaTime;
         
            float deltaZ = 0;
            if (input.GetKey(MoveUpKey))
            {
                deltaZ = 0.5f * deltaTime;
            }
            else if (input.GetKey(MoveDownKey))
            {
                deltaZ = -0.5f * deltaTime;
            }
            return new Vector3(deltaX, deltaY, deltaZ);
        }

        protected override void Start()
        {
            base.Start();
            m_component.Editor.ActiveWindowChanged += Editor_ActiveWindowChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_component != null && m_component.Editor != null)
            {
                m_component.Editor.ActiveWindowChanged -= Editor_ActiveWindowChanged;
            }
        }

        protected virtual void UpdateCursorState(bool isPointerOverEditorArea, bool pan, bool rotate, bool freeMove)
        {
            bool freeMoveZoom = FreeMoveZoomAction();

            SceneComponent.UpdateCursorState(isPointerOverEditorArea, pan, rotate, freeMove && !freeMoveZoom);
        }

        private void Editor_ActiveWindowChanged(RuntimeWindow deactivatedWindow)
        {
            if (m_component != null)
            {
                if (m_isActive)
                {
                    UpdateCursorState(false, false, false, false);
                    m_pan = false;
                    m_rotate = false;
                    m_freeMove = false;
                }
                m_isActive = m_component.IsWindowActive;
            }
        }

        
        protected override void LateUpdate()
        {
            if (!m_component.IsWindowActive)
            {
                return;
            }

            bool isPointerOver = m_component.Window.IsPointerOver;

            IInput input = m_component.Editor.Input;
            RuntimeTools tools = m_component.Editor.Tools;

            bool canRotate = AllowRotateAction();
            bool rotate = (RotateAction() || SwapLRMB && canRotate) && SceneComponent.CanRotate;
            bool pan = PanAction() && SceneComponent.CanPan;
            bool freeMove = FreeMoveAction() && SceneComponent.CanFreeMove;

            if (pan && tools.Current != RuntimeTool.View)
            {
                rotate = false;
            }

            bool beginRotate = m_rotate != rotate && rotate;
            if (beginRotate && !isPointerOver)
            {
                rotate = false;
                beginRotate = false;
            }
            bool endRotate = m_rotate != rotate && !rotate;
            m_rotate = rotate;
            if (!m_rotate)
            {
                m_rotateActive = false;
            }

            bool beginPan = m_pan != pan && pan;
            if (beginPan && !isPointerOver)
            {
                pan = false;
            }
            bool endPan = m_pan != pan && !pan;
            m_pan = pan;

            bool beginFreeMove = m_freeMove != freeMove && freeMove;
            if (beginFreeMove && !isPointerOver)
            {
                freeMove = false;

            }
            bool endFreeMove = m_freeMove != freeMove && !freeMove;
            m_freeMove = freeMove;
            if (!m_freeMove)
            {
                m_freeMoveActive = false;
            }

            Vector3 pointerPosition = input.GetPointerXY(0);
            tools.IsViewing = m_rotate || m_pan || m_freeMove;

         
            if (beginPan || endPan || beginRotate && m_beginRotateImmediately || endRotate || beginFreeMove && m_beginFreeMoveImmediately || endFreeMove)
            {
                UpdateCursorState(isPointerOver, m_pan, m_rotate && m_beginRotateImmediately, m_freeMove && m_beginFreeMoveImmediately);
            }

            if (m_freeMove)
            {
                Vector2 rotateAxes = RotateAxes();
                Vector3 moveAxes = MoveAxes() * FreeMoveSensitivity * 0.25f;
                float zoomAxis = ZoomAxis() * FreeZoomSensitivity * 0.25f;

                bool freeMoveZoom = FreeMoveZoomAction();
                if (freeMoveZoom)
                {
                    zoomAxis = rotateAxes.x * FreeZoomSensitivity * 0.025f;
                    rotateAxes = Vector2.zero;
                    moveAxes = Vector3.zero;
                }
                else
                {
                    rotateAxes *= FreeRotateSensitivity;
                }

                SceneComponent.FreeMove(rotateAxes, moveAxes, zoomAxis);

                if (!m_freeMoveActive && (rotateAxes != Vector2.zero || moveAxes != Vector3.zero || zoomAxis != 0))
                {
                    UpdateCursorState(isPointerOver, m_pan, m_rotate, m_freeMove);
                    m_freeMoveActive = true;
                }
            }
            else if (m_rotate)
            {
                if (canRotate)
                {
                    Vector2 orbitAxes = RotateAxes();
                    float zoomAxis = ZoomAxis();
                    SceneComponent.Orbit(orbitAxes.x * RotateXSensitivity, orbitAxes.y * RotateYSensitivity, zoomAxis * MoveZSensitivity);
                    if (!m_rotateActive && (orbitAxes != Vector2.zero || zoomAxis != 0))
                    {
                        UpdateCursorState(isPointerOver, m_pan, m_rotate, m_freeMove);
                        m_rotateActive = true;
                    }
                }
                else
                {
                    Transform camTransform = m_component.Window.Camera.transform;
                    Ray pointer = m_component.Window.Pointer;
                    SceneComponent.Zoom(ZoomAxis() * MoveZSensitivity, Quaternion.FromToRotation(Vector3.forward, camTransform.InverseTransformVector(pointer.direction).normalized));
                }
                SceneComponent.FreeMove(Vector2.zero, Vector3.zero, 0);
            }
            else if (m_pan)
            {
                if (beginPan)
                {
                    SceneComponent.BeginPan(pointerPosition);
                }
                SceneComponent.Pan(pointerPosition);
            }
            else
            {
                SceneComponent.FreeMove(Vector2.zero, Vector3.zero, 0);

                if (isPointerOver)
                {
                    SceneComponent.Zoom(ZoomAxis() * MoveZSensitivity, Quaternion.identity);
     
                    BeginSelectAction();
                    if (SelectAction())
                    {
                        SelectGO();
                    }

                    if (SnapToGridAction())
                    {
                        SceneComponent.SnapToGrid();
                    }

                    if (FocusAction())
                    {
                        if(FocusActiveAction())
                        {
                            SceneComponent.Focus(FocusMode.AllActive);
                        }
                        else
                        {
                            if (SceneComponent.Selection.activeTransform != null && SceneComponent.Selection.activeTransform.GetComponent<Terrain>() == null)
                            {
                                SceneComponent.Focus(FocusMode.Selected);
                            }
                        }   
                    }

                    if (SelectAllAction())
                    {
                        SceneComponent.SelectAll();
                    }
                }
            }
        }
    }
}

