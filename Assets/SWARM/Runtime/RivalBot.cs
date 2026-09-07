using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// First-test rival: wanders cheaply, then starts hunting exposed trail after the player proves the capture loop once.
    /// No per-frame world scans and no physics dependency.
    /// </summary>
    public sealed class RivalBot : MonoBehaviour
    {
        private TerritorySystem _territory;
        private Vector2 _halfExtents;
        private Vector3 _wanderTarget;
        private float _decisionTimer;
        private float _phase;
        private const float Speed = 3.35f;

        public void Initialize(TerritorySystem territory, Vector2 halfExtents)
        {
            _territory = territory;
            _halfExtents = halfExtents;
            transform.position = new Vector3(halfExtents.x * 0.68f, halfExtents.y * 0.58f, 0f);
            _wanderTarget = new Vector3(-halfExtents.x * 0.55f, halfExtents.y * 0.42f, 0f);
            BuildVisual();
        }

        private void BuildVisual()
        {
            var body = gameObject.AddComponent<SpriteRenderer>();
            body.sprite = RuntimeArt.Circle;
            RuntimeArt.Configure(body);
            body.color = new Color(1f, 0.26f, 0.34f, 1f);
            body.sortingOrder = 19;
            transform.localScale = Vector3.one * 1.05f;

            var ring = new GameObject("DangerRing").transform;
            ring.SetParent(transform, false);
            ring.localScale = Vector3.one * 1.45f;
            var ringRenderer = ring.gameObject.AddComponent<SpriteRenderer>();
            ringRenderer.sprite = RuntimeArt.Circle;
            RuntimeArt.Configure(ringRenderer);
            ringRenderer.color = new Color(1f, 0.15f, 0.22f, 0.16f);
            ringRenderer.sortingOrder = 18;
        }

        private void Update()
        {
            if (_territory == null) return;

            bool threatActive = _territory.CaptureCount > 0;

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
                float huntBoost = threatActive && _territory.IsTrailExposed ? 1.24f : 1f;
                transform.position += direction * (Speed * huntBoost * Time.deltaTime);
            }

            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, -_halfExtents.x + 0.4f, _halfExtents.x - 0.4f);
            p.y = Mathf.Clamp(p.y, -_halfExtents.y + 0.4f, _halfExtents.y - 0.4f);
            transform.position = p;

            if (threatActive)
                _territory.TryCutAtWorldPosition(transform.position);

            _phase += Time.deltaTime * (threatActive ? 4.2f : 2.4f);
            float pulse = 1f + Mathf.Sin(_phase) * (threatActive ? 0.045f : 0.020f);
            transform.localScale = Vector3.one * (1.05f * pulse);
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
