using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Placeholder presenter only. Gameplay never depends on this object, so it can later be replaced by a rigged 2D or 3D presenter.
    /// </summary>
    public sealed class BlobPresenter : MonoBehaviour
    {
        private Transform _visualRoot;
        private Transform _leftEye;
        private Transform _rightEye;
        private SpriteRenderer _body;
        private PlayerMotor _motor;
        private SwarmController _swarm;
        private float _pulse;
        private float _speed01;
        private float _growthScale = 1f;
        private Vector2 _facing = Vector2.up;

        public void Initialize(PlayerMotor motor)
        {
            _motor = motor;
            BuildVisuals();
            motor.MotionChanged += OnMotionChanged;
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
            if (_motor != null) _motor.MotionChanged -= OnMotionChanged;
            if (_swarm != null) _swarm.CountChanged -= OnSwarmCountChanged;
        }

        public void Pulse(float amount = 1f)
        {
            _pulse = Mathf.Max(_pulse, amount);
        }

        private void BuildVisuals()
        {
            _visualRoot = new GameObject("VisualRoot").transform;
            _visualRoot.SetParent(transform, false);

            _body = _visualRoot.gameObject.AddComponent<SpriteRenderer>();
            _body.sprite = RuntimeArt.Circle;
            RuntimeArt.Configure(_body);
            _body.color = new Color(0.97f, 0.75f, 0.24f, 1f);
            _body.sortingOrder = 20;
            _visualRoot.localScale = Vector3.one * 1.15f;

            _leftEye = CreateEye("Eye_L", new Vector2(-0.22f, 0.18f));
            _rightEye = CreateEye("Eye_R", new Vector2(0.22f, 0.18f));
        }

        private Transform CreateEye(string name, Vector2 localPosition)
        {
            var eye = new GameObject(name).transform;
            eye.SetParent(_visualRoot, false);
            eye.localPosition = localPosition;
            eye.localScale = Vector3.one * 0.23f;

            var white = eye.gameObject.AddComponent<SpriteRenderer>();
            white.sprite = RuntimeArt.Circle;
            RuntimeArt.Configure(white);
            white.color = Color.white;
            white.sortingOrder = 21;

            var pupil = new GameObject("Pupil").transform;
            pupil.SetParent(eye, false);
            pupil.localPosition = new Vector3(0f, 0.03f, 0f);
            pupil.localScale = Vector3.one * 0.48f;
            var black = pupil.gameObject.AddComponent<SpriteRenderer>();
            black.sprite = RuntimeArt.Circle;
            RuntimeArt.Configure(black);
            black.color = new Color(0.12f, 0.14f, 0.18f, 1f);
            black.sortingOrder = 22;
            return eye;
        }

        private void OnMotionChanged(Vector2 facing, float speed01)
        {
            _facing = facing;
            _speed01 = speed01;
        }

        private void OnSwarmCountChanged(int count)
        {
            float target = 1f + Mathf.Min(count, 50) * 0.006f;
            _growthScale = target;

            if (_body != null)
            {
                if (count >= 30) _body.color = new Color(1f, 0.48f, 0.18f, 1f);
                else if (count >= 12) _body.color = new Color(1f, 0.66f, 0.18f, 1f);
                else _body.color = new Color(0.97f, 0.75f, 0.24f, 1f);
            }

            if (count == 8 || count == 20 || count == 40)
                Pulse(1.8f);
        }

        private void Update()
        {
            if (_visualRoot == null) return;

            _pulse = Mathf.MoveTowards(_pulse, 0f, Time.deltaTime * 5.5f);
            float breathing = 1f + Mathf.Sin(Time.time * 4.5f) * 0.018f;
            float pulseScale = 1f + _pulse * 0.13f;
            float stretch = 1f + _speed01 * 0.08f;
            float squash = 1f - _speed01 * 0.045f;
            float baseScale = 1.15f * _growthScale;
            _visualRoot.localScale = new Vector3(baseScale * squash, baseScale * stretch, 1f) * breathing * pulseScale;

            Vector3 eyeOffset = (Vector3)(_facing * 0.075f);
            if (_leftEye != null) _leftEye.localPosition = new Vector3(-0.22f, 0.18f, 0f) + eyeOffset;
            if (_rightEye != null) _rightEye.localPosition = new Vector3(0.22f, 0.18f, 0f) + eyeOffset;
        }
    }
}
