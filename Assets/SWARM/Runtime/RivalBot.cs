using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// First-test rival: stays visually passive during onboarding, then activates and hunts exposed trails
    /// after the player proves the capture loop once. No per-frame world scans and no physics dependency.
    /// </summary>
    public sealed class RivalBot : MonoBehaviour
    {
        private TerritorySystem _territory;
        private Vector2 _halfExtents;
        private Vector3 _wanderTarget;
        private SpriteRenderer _body;
        private SpriteRenderer _ring;
        private float _decisionTimer;
        private float _phase;
        private bool _activated;
        private const float Speed = 3.35f;

        public void Initialize(TerritorySystem territory, Vector2 halfExtents)
        {
            _territory = territory;
            _halfExtents = halfExtents;
            transform.position = new Vector3(halfExtents.x * 0.68f, halfExtents.y * 0.58f, 0f);
            _wanderTarget = new Vector3(-halfExtents.x * 0.55f, halfExtents.y * 0.42f, 0f);
            BuildVisual();
            SetThreatVisual(false);
        }

        private void BuildVisual()
        {
            _body = gameObject.AddComponent<SpriteRenderer>();
            _body.sprite = RuntimeArt.Circle;
            RuntimeArt.Configure(_body);
            _body.sortingOrder = 19;
            transform.localScale = Vector3.one * 0.92f;

            var ring = new GameObject("DangerRing").transform;
            ring.SetParent(transform, false);
            ring.localScale = Vector3.one * 1.45f;
            _ring = ring.gameObject.AddComponent<SpriteRenderer>();
            _ring.sprite = RuntimeArt.Circle;
            RuntimeArt.Configure(_ring);
            _ring.sortingOrder = 18;
        }

        private void Update()
        {
            if (_territory == null) return;

            bool threatActive = _territory.CaptureCount > 0;
            if (threatActive && !_activated)
            {
                _activated = true;
                SetThreatVisual(true);
                _phase = Mathf.PI * 0.5f;
            }

            _decisionTimer -= Time.deltaTime;
            if (_decisionTimer <= 0f)
            {
                _decisionTimer = 0.22f;
                ChooseTarget(threatActive);
            }

            Vector3 delta = _wanderTarget - transform.position;
            if (delta.sqrMagnitude > 0.01f)
            {
                Vector3 direction = delta.normalized;
                float movementScale = threatActive ? (_territory.IsTrailExposed ? 1.24f : 1f) : 0.42f;
                transform.position += direction * (Speed * movementScale * Time.deltaTime);
            }

            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, -_halfExtents.x + 0.4f, _halfExtents.x - 0.4f);
            p.y = Mathf.Clamp(p.y, -_halfExtents.y + 0.4f, _halfExtents.y - 0.4f);
            transform.position = p;

            if (threatActive)
                _territory.TryCutAtWorldPosition(transform.position);

            _phase += Time.deltaTime * (threatActive ? 4.2f : 1.7f);
            float pulseAmount = threatActive ? 0.055f : 0.012f;
            float baseScale = threatActive ? 1.05f : 0.92f;
            float pulse = 1f + Mathf.Sin(_phase) * pulseAmount;
            transform.localScale = Vector3.one * (baseScale * pulse);
        }

        private void SetThreatVisual(bool active)
        {
            if (_body != null)
                _body.color = active
                    ? new Color(1f, 0.26f, 0.34f, 1f)
                    : new Color(0.70f, 0.30f, 0.34f, 0.28f);

            if (_ring != null)
                _ring.color = active
                    ? new Color(1f, 0.15f, 0.22f, 0.18f)
                    : new Color(1f, 0.15f, 0.22f, 0.025f);
        }

        private void ChooseTarget(bool threatActive)
        {
            if (threatActive && _territory.TryGetTrailTarget(out Vector3 trailTarget))
            {
                _wanderTarget = trailTarget;
                return;
            }

            if ((_wanderTarget - transform.position).sqrMagnitude < 1.5f)
            {
                float t = Time.time * 0.37f + _phase;
                _wanderTarget = new Vector3(
                    Mathf.Sin(t * 1.31f) * _halfExtents.x * 0.78f,
                    Mathf.Cos(t * 0.91f) * _halfExtents.y * 0.78f,
                    0f);
            }
        }
    }
}
