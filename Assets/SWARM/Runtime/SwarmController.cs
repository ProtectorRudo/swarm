using System;
using System.Collections.Generic;
using UnityEngine;

namespace Swarm
{
    public sealed class SwarmController : MonoBehaviour
    {
        private sealed class FollowerProxy
        {
            public Transform Transform;
            public float Spawn;
        }

        private const int MaxVisibleFollowers = 80;
        private const int HistoryLength = 256;
        private const float HistoryStep = 0.025f;

        private readonly List<FollowerProxy> _followers = new List<FollowerProxy>(MaxVisibleFollowers);
        private readonly Vector3[] _history = new Vector3[HistoryLength];

        private Transform _anchor;
        private PlayerMotor _motor;
        private int _historyHead;
        private float _historyTimer;

        public int Count { get; private set; }
        public int VisibleCount => Mathf.Min(Count, MaxVisibleFollowers);
        public event Action<int> CountChanged;

        public void Initialize(Transform anchor, PlayerMotor motor)
        {
            _anchor = anchor;
            _motor = motor;
            SnapHistoryToAnchor();
        }

        public void AddUnits(int amount)
        {
            if (amount <= 0) return;
            SetCount(Count + amount);
        }

        public void RemoveUnits(int amount)
        {
            if (amount <= 0 || Count <= 0) return;
            SetCount(Mathf.Max(0, Count - amount));
        }

        public void SetCount(int count)
        {
            int next = Mathf.Max(0, count);
            if (next == Count) return;
            Count = next;
            EnsureVisualCount(VisibleCount);
            CountChanged?.Invoke(Count);
        }

        public void SnapHistoryToAnchor()
        {
            if (_anchor == null) return;
            _historyHead = 0;
            _historyTimer = 0f;
            for (int i = 0; i < HistoryLength; i++) _history[i] = _anchor.position;
            for (int i = 0; i < _followers.Count; i++)
                _followers[i].Transform.position = _anchor.position;
        }

        private void EnsureVisualCount(int desired)
        {
            while (_followers.Count < desired)
            {
                int index = _followers.Count;
                var go = new GameObject("Follower_" + index.ToString("000"));
                go.transform.SetParent(transform, false);
                go.transform.position = _anchor != null ? _anchor.position : Vector3.zero;
                go.transform.localScale = Vector3.zero;

                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeArt.Circle;
                RuntimeArt.Configure(renderer);
                float hueShift = (index % 7) * 0.012f;
                renderer.color = new Color(1f, 0.62f + hueShift, 0.16f, 0.93f);
                renderer.sortingOrder = 10 + index % 3;

                _followers.Add(new FollowerProxy { Transform = go.transform, Spawn = 0f });
            }
        }

        private void Update()
        {
            if (_anchor == null) return;

            RecordHistory();
            Vector2 forward = _motor != null && _motor.Velocity.sqrMagnitude > 0.01f
                ? _motor.Velocity.normalized
                : Vector2.up;
            Vector2 right = new Vector2(forward.y, -forward.x);

            int active = VisibleCount;
            int columns = active >= 30 ? 6 : active >= 12 ? 5 : 4;

            for (int i = 0; i < _followers.Count; i++)
            {
                var follower = _followers[i];
                bool shouldBeVisible = i < active;
                if (follower.Transform.gameObject.activeSelf != shouldBeVisible)
                    follower.Transform.gameObject.SetActive(shouldBeVisible);
                if (!shouldBeVisible) continue;

                follower.Spawn = Mathf.MoveTowards(follower.Spawn, 1f, Time.deltaTime * 7f);

                int row = i / columns;
                int column = i % columns;
                int delaySteps = 3 + row * 3;
                int historyIndex = _historyHead - delaySteps;
                while (historyIndex < 0) historyIndex += HistoryLength;
                historyIndex %= HistoryLength;

                float laneCenter = (columns - 1) * 0.5f;
                float lane = (column - laneCenter) * 0.38f;
                float spread = Mathf.Min(0.28f, row * 0.018f);
                lane *= 1f + spread;
                float wobble = Mathf.Sin(Time.time * 3.7f + i * 1.91f) * 0.07f;
                Vector3 target = _history[historyIndex] + (Vector3)(right * (lane + wobble));

                float follow = 1f - Mathf.Exp(-14f * Time.deltaTime);
                follower.Transform.position = Vector3.Lerp(follower.Transform.position, target, follow);

                float sizeNoise = 0.66f + (i % 5) * 0.035f;
                float targetScale = sizeNoise * follower.Spawn;
                follower.Transform.localScale = Vector3.one * targetScale;
            }
        }

        private void RecordHistory()
        {
            _historyTimer += Time.deltaTime;
            while (_historyTimer >= HistoryStep)
            {
                _historyTimer -= HistoryStep;
                _historyHead = (_historyHead + 1) % HistoryLength;
                _history[_historyHead] = _anchor.position;
            }
        }
    }
}
