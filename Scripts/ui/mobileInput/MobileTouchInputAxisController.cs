using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.InputSystem.EnhancedTouch;
using Object = UnityEngine.Object;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class MobileTouchInputAxisController : InputAxisControllerBase<MobileTouchInputAxisController.TouchReader>
{
    private void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    protected override void OnEnable()
    {
        base.OnEnable(); // calls SynchronizeControllers()

        for (int i = 0; i < Controllers.Count; i++)
        {
            switch (i)
            {
                case 0:
                    Controllers[i].Input.axis = TouchReader.Axis.Horizontal;
                    break;
                case 1:
                    Controllers[i].Input.axis = TouchReader.Axis.Vertical;
                    break;
                default:
                    Controllers[i].Enabled = false;
                    break;
            }
        }
    }

    void Update()
    {
        if (Application.isPlaying)
        {
            TouchState.Update(); // <-- ключевой момент
            UpdateControllers();
        }
    }

    // ===================== SHARED TOUCH STATE =====================
    static class TouchState
    {
        public static int cameraFingerId = -1;
        public static Vector2 deltaThisFrame;
        static Vector2 lastPos;
        static int lastFrame = -1;

        public static void Update()
        {
            if (Time.frameCount == lastFrame)
                return;

            lastFrame = Time.frameCount;
            deltaThisFrame = Vector2.zero;

            // если текущий палец пропал — сбрасываем
            if (cameraFingerId != -1 && !IsFingerAlive(cameraFingerId))
            {
                cameraFingerId = -1;
            }

            foreach (var touch in Touch.activeTouches)
            {
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    TryAssignFinger(touch);
                }

                if (touch.finger.index == cameraFingerId &&
                    touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
                {
                    Vector2 current = touch.screenPosition;
                    deltaThisFrame = current - lastPos;
                    lastPos = current;
                }
            }
        }

        static void TryAssignFinger(Touch touch)
        {
            if (cameraFingerId != -1)
                return;

            // Only right half of screen controls the camera; left half is for the joystick
            if (touch.screenPosition.x < Screen.width * 0.5f)
                return;

            if (IsTouchOverUI(touch.screenPosition))
                return;

            cameraFingerId = touch.finger.index;
            lastPos = touch.screenPosition;
        }

        static bool IsTouchOverUI(Vector2 screenPos)
        {
            if (EventSystem.current == null)
                return false;

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPos
            };

            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
                if (result.module is UnityEngine.UI.GraphicRaycaster)
                    return true;

            return false;
        }

        static bool IsFingerAlive(int id)
        {
            foreach (var t in Touch.activeTouches)
                if (t.finger.index == id)
                    return true;
            return false;
        }
    }

    // ===================== AXIS READER =====================
    [Serializable]
    public class TouchReader : IInputAxisReader
    {
        public enum Axis
        {
            Horizontal,
            Vertical
        }

        public Axis axis;
        public float sensitivity = 0.1f;

        public float GetValue(Object context, IInputAxisOwner.AxisDescriptor.Hints hint)
        {
            Vector2 delta = TouchState.deltaThisFrame;

            float value = axis == Axis.Horizontal
                ? delta.x
                : -delta.y;

            return value * sensitivity;
        }
    }
}