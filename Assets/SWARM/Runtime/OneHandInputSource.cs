using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Reads exactly one pointer. Drag moves. A quick tap followed by a second press-and-hold enables fire while
    /// that same thumb can continue dragging to steer. No second finger or attack button is required.
    /// </summary>
    public sealed class OneHandInputSource : MonoBehaviour
    {
        [SerializeField] private float deadZonePixels = 10f;
        [SerializeField] private float maxDragPixels = 135f;
        [SerializeField] private float doubleTapWindow = 0.34f;
        [SerializeField] private float firstTapMaxDuration = 0.22f;
        [SerializeField] private float firstTapMaxTravel = 42f;
        [SerializeField] private float secondTapMaxDistance = 105f;

        private Vector2 _origin;
        private Vector2 _pointerDownPosition;
        private Vector2 _lastTapPosition;
        private float _pointerDownTime;
        private float _lastTapReleaseTime = -10f;
        private float _maxPointerTravel;
        private bool _pointerActive;
        private bool _fireGesture;

        public Vector2 MoveIntent { get; private set; }
        public float Strength { get; private set; }
        public bool HasActivePointer => _pointerActive;
        public bool HasEverMoved { get; private set; }
        public bool HasEverFired { get; private set; }
        public bool PressedThisFrame { get; private set; }
        public bool FireHeld { get; private set; }
        public bool FirePressedThisFrame { get; private set; }

        private void Update()
        {
            PressedThisFrame = false;
            FirePressedThisFrame = false;

            if (Input.touchCount > 0)
            {
                ReadTouch(Input.GetTouch(0));
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                BeginPointer(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                MovePointer(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                EndPointer(Input.mousePosition);
            }

            if (!_pointerActive)
                ReadEditorKeys();

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space))
            {
                FireHeld = true;
                FirePressedThisFrame = true;
                HasEverFired = true;
            }
            else if (Input.GetKey(KeyCode.Space))
            {
                FireHeld = true;
            }
            else if (Input.GetKeyUp(KeyCode.Space) && !_fireGesture)
            {
                FireHeld = false;
            }
#endif
        }

        private void ReadTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    BeginPointer(touch.position);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    MovePointer(touch.position);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    EndPointer(touch.position);
                    break;
            }
        }

        private void BeginPointer(Vector2 screenPosition)
        {
            _pointerActive = true;
            _origin = screenPosition;
            _pointerDownPosition = screenPosition;
            _pointerDownTime = Time.unscaledTime;
            _maxPointerTravel = 0f;
            MoveIntent = Vector2.zero;
            Strength = 0f;
            PressedThisFrame = true;

            bool insideWindow = Time.unscaledTime - _lastTapReleaseTime <= doubleTapWindow;
            bool closeEnough = (screenPosition - _lastTapPosition).sqrMagnitude <= secondTapMaxDistance * secondTapMaxDistance;
            _fireGesture = insideWindow && closeEnough;
            FireHeld = _fireGesture;
            if (_fireGesture)
            {
                FirePressedThisFrame = true;
                HasEverFired = true;
                _lastTapReleaseTime = -10f;
            }
        }

        private void MovePointer(Vector2 screenPosition)
        {
            if (!_pointerActive)
            {
                BeginPointer(screenPosition);
                return;
            }

            _maxPointerTravel = Mathf.Max(_maxPointerTravel, (screenPosition - _pointerDownPosition).magnitude);

            Vector2 delta = screenPosition - _origin;
            float distance = delta.magnitude;

            if (distance > maxDragPixels && distance > 0.001f)
            {
                Vector2 direction = delta / distance;
                _origin = screenPosition - direction * maxDragPixels;
                delta = screenPosition - _origin;
                distance = maxDragPixels;
            }

            if (distance <= deadZonePixels)
            {
                MoveIntent = Vector2.zero;
                Strength = 0f;
                return;
            }

            float usableRange = Mathf.Max(1f, maxDragPixels - deadZonePixels);
            Strength = Mathf.Clamp01((distance - deadZonePixels) / usableRange);
            MoveIntent = delta.normalized;
            HasEverMoved = true;
        }

        private void EndPointer(Vector2 screenPosition)
        {
            if (!_pointerActive) return;

            _maxPointerTravel = Mathf.Max(_maxPointerTravel, (screenPosition - _pointerDownPosition).magnitude);
            float duration = Time.unscaledTime - _pointerDownTime;

            if (!_fireGesture && duration <= firstTapMaxDuration && _maxPointerTravel <= firstTapMaxTravel)
            {
                _lastTapReleaseTime = Time.unscaledTime;
                _lastTapPosition = screenPosition;
            }

            _pointerActive = false;
            _fireGesture = false;
            FireHeld = false;
            MoveIntent = Vector2.zero;
            Strength = 0f;
        }

        private void ReadEditorKeys()
        {
            Vector2 value = Vector2.zero;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) value.x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) value.x += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) value.y -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) value.y += 1f;

            if (value.sqrMagnitude > 0.001f)
            {
                MoveIntent = value.normalized;
                Strength = 1f;
                HasEverMoved = true;
            }
            else if (!_pointerActive)
            {
                MoveIntent = Vector2.zero;
                Strength = 0f;
            }
        }
    }
}
