using System;
using UnityEngine;

namespace Swarm
{
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private float maxSpeed = 6.2f;
        [SerializeField] private float acceleration = 42f;
        [SerializeField] private float deceleration = 55f;

        private OneHandInputSource _input;
        private Vector2 _velocity;
        private Vector2 _arenaHalfExtents;
        private bool _movementEnabled = true;

        public Vector2 Velocity => _velocity;
        public float Speed01 => Mathf.Clamp01(_velocity.magnitude / Mathf.Max(0.01f, maxSpeed));
        public event Action<Vector2, float> MotionChanged;

        public void Initialize(OneHandInputSource input, Vector2 arenaHalfExtents)
        {
            _input = input;
            _arenaHalfExtents = arenaHalfExtents;
        }

        public void SetMovementEnabled(bool enabled)
        {
            _movementEnabled = enabled;
            if (!enabled) _velocity = Vector2.zero;
        }

        public void Teleport(Vector2 worldPosition)
        {
            _velocity = Vector2.zero;
            transform.position = new Vector3(
                Mathf.Clamp(worldPosition.x, -_arenaHalfExtents.x, _arenaHalfExtents.x),
                Mathf.Clamp(worldPosition.y, -_arenaHalfExtents.y, _arenaHalfExtents.y),
                transform.position.z);
        }

        private void Update()
        {
            if (_input == null) return;

            Vector2 targetVelocity = _movementEnabled
                ? _input.MoveIntent * (maxSpeed * _input.Strength)
                : Vector2.zero;
            float rate = targetVelocity.sqrMagnitude > 0.001f ? acceleration : deceleration;
            _velocity = Vector2.MoveTowards(_velocity, targetVelocity, rate * Time.deltaTime);

            Vector3 next = transform.position + (Vector3)(_velocity * Time.deltaTime);
            next.x = Mathf.Clamp(next.x, -_arenaHalfExtents.x, _arenaHalfExtents.x);
            next.y = Mathf.Clamp(next.y, -_arenaHalfExtents.y, _arenaHalfExtents.y);
            transform.position = next;

            Vector2 facing = _velocity.sqrMagnitude > 0.001f ? _velocity.normalized : Vector2.up;
            MotionChanged?.Invoke(facing, Speed01);
        }
    }
}
