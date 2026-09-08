using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Full-arena camera for 0.4. The map never follows one participant; every base, army and pickup remains readable.
    /// </summary>
    public sealed class CameraRig : MonoBehaviour
    {
        private Camera _camera;
        private Vector2 _arenaHalfExtents;
        private float _punch;

        public void Initialize(Camera camera, Transform target, Vector2 arenaHalfExtents)
        {
            _camera = camera;
            _arenaHalfExtents = arenaHalfExtents;
            _camera.orthographic = true;
            transform.position = new Vector3(0f, 0f, -10f);
            FitArena(true);
        }

        public void BindSwarm(SwarmController swarm)
        {
            // Intentionally no-op in Battle Arena: army growth must never hide the rest of the map.
        }

        public void Punch(float amount)
        {
            _punch = Mathf.Clamp(_punch + amount, 0f, 0.30f);
        }

        private void LateUpdate()
        {
            if (_camera == null) return;
            _punch = Mathf.MoveTowards(_punch, 0f, Time.deltaTime * 2.8f);
            FitArena(false);
        }

        private void FitArena(bool snap)
        {
            float aspect = Mathf.Max(0.35f, _camera.aspect);
            float requiredForHeight = _arenaHalfExtents.y + 0.75f;
            float requiredForWidth = (_arenaHalfExtents.x + 0.75f) / aspect;
            float targetSize = Mathf.Max(requiredForHeight, requiredForWidth) - _punch * 0.16f;

            _camera.orthographicSize = snap
                ? targetSize
                : Mathf.Lerp(_camera.orthographicSize, targetSize, 1f - Mathf.Exp(-10f * Time.deltaTime));

            transform.position = new Vector3(0f, 0f, -10f);
        }
    }
}
