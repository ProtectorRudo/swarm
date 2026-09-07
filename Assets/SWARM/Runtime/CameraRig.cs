using UnityEngine;

namespace Swarm
{
    public sealed class CameraRig : MonoBehaviour
    {
        private Camera _camera;
        private Transform _target;
        private SwarmController _swarm;
        private Vector3 _velocity;
        private float _baseSize = 7.6f;
        private float _growthZoom;
        private float _punch;

        public void Initialize(Camera camera, Transform target)
        {
            _camera = camera;
            _target = target;
            _camera.orthographic = true;
            _camera.orthographicSize = _baseSize;
        }

        public void BindSwarm(SwarmController swarm)
        {
            if (_swarm != null) _swarm.CountChanged -= OnSwarmCountChanged;
            _swarm = swarm;
            if (_swarm != null)
            {
                _swarm.CountChanged += OnSwarmCountChanged;
                OnSwarmCountChanged(_swarm.Count);
            }
        }

        private void OnDestroy()
        {
            if (_swarm != null) _swarm.CountChanged -= OnSwarmCountChanged;
        }

        public void Punch(float amount)
        {
            _punch = Mathf.Clamp(_punch + amount, 0f, 0.72f);
        }

        private void OnSwarmCountChanged(int count)
        {
            _growthZoom = Mathf.Min(count, 60) * 0.010f;
        }

        private void LateUpdate()
        {
            if (_target == null || _camera == null) return;

            Vector3 desired = new Vector3(_target.position.x, _target.position.y, -10f);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, 0.13f, 100f, Time.deltaTime);

            _punch = Mathf.MoveTowards(_punch, 0f, Time.deltaTime * 1.8f);
            float desiredSize = _baseSize + _growthZoom - _punch;
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, desiredSize, 1f - Mathf.Exp(-10f * Time.deltaTime));
        }
    }
}
