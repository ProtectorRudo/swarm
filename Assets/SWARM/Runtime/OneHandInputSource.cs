using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Reads one pointer directly and emits movement intent. The UI is never the authority for movement.
    /// Touch is primary; mouse and keyboard exist only for Editor iteration.
    /// </summary>
    public sealed class OneHandInputSource : MonoBehaviour
    {
        [SerializeField] private float deadZonePixels = 10f;
        [SerializeField] private float maxDragPixels = 135f;

        private Vector2 _origin;
        private bool _pointerActive;

        public Vector2 MoveIntent { get; private set; }
        public float Strength { get; private set; }
        public bool HasActivePointer => _pointerActive;
        public bool HasEverMoved { get; private set; }
        public bool PressedThisFrame { get; private set; }

        private void Update()
        {
            PressedThisFrame = false;

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
                EndPointer();
            }

            if (!_pointerActive)
                ReadEditorKeys();
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
                    EndPointer();
                    break;
            }
        }

        private void BeginPointer(Vector2 screenPosition)
        {
            _pointerActive = true;
            _origin = screenPosition;
            MoveIntent = Vector2.zero;
            Strength = 0f;
            PressedThisFrame = true;
        }

        private void MovePointer(Vector2 screenPosition)
        {
            if (!_pointerActive)
            {
                BeginPointer(screenPosition);
                return;
            }

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

        private void EndPointer()
        {
            _pointerActive = false;
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
