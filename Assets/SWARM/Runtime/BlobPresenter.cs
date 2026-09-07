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
        private float _pulse;
        private float _speed01;
        private Vector2 _facing = Vector2.up;

        public void Initialize(PlayerMotor motor)
        {
            BuildVisuals();
            motor.MotionChanged += OnMotionChanged;
        }

        private void OnDestroy()
        {
            var motor = GetComponent<PlayerMotor>();
            if (motor != null) motor.MotionChanged -= OnMotionChanged;
        }

        public void Pulse(float amount = 1f)
        {
            _pulse = Mathf.Max(_pulse, amount);
        }

        private void BuildVisuals()
        {
            _visualRoot = new GameObject("VisualRoot").transform;
            _visualRoot.SetParent(transform, false);

            var body = _visualRoot.gameObject.AddComponent<SpriteRenderer>();
            body.sprite = RuntimeArt.Circle;
            body.color = new Color(0.97f, 0.75f, 0.24f, 1f);
            body.sortingOrder = 20;
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
            white.color = Color.white;
            white.sortingOrder = 21;

            var pupil = new GameObject("Pupil").transform;
            pupil.SetParent(eye, false);
            pupil.localPosition = new Vector3(0f, 0.03f, 0f);
            pupil.localScale = Vector3.one * 0.48f;
            var black = pupil.gameObject.AddComponent<SpriteRenderer>();
            black.sprite = RuntimeArt.Circle;
            black.color = new Color(0.12f, 0.14f, 0.18f, 1f);
            black.sortingOrder = 22;
            return eye;
        }

        private void OnMotionChanged(Vector2 facing, float speed01)
        {
            _facing = facing;
            _speed01 = speed01;
        }

        private void Update()
        {
            if (_visualRoot == null) return;

            _pulse = Mathf.MoveTowards(_pulse, 0f, Time.deltaTime * 5.5f);
            float breathing = 1f + Mathf.Sin(Time.time * 4.5f) * 0.018f;
            float pulseScale = 1f + _pulse * 0.13f;
            float stretch = 1f + _speed01 * 0.08f;
            float squash = 1f - _speed01 * 0.045f;
            _visualRoot.localScale = new Vector3(1.15f * squash, 1.15f * stretch, 1f) * breathing * pulseScale;

            Vector3 eyeOffset = (Vector3)(_facing * 0.075f);
            if (_leftEye != null) _leftEye.localPosition = new Vector3(-0.22f, 0.18f, 0f) + eyeOffset;
            if (_rightEye != null) _rightEye.localPosition = new Vector3(0.22f, 0.18f, 0f) + eyeOffset;
        }
    }
}
