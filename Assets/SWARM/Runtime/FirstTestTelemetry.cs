using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Local-only telemetry for the first playable APK. No network, persistence or external SDK.
    /// </summary>
    public sealed class FirstTestTelemetry : MonoBehaviour
    {
        private MatchDirector _match;
        private TerritorySystem _territory;
        private SwarmController _swarm;
        private float _elapsed;

        public int Captures { get; private set; }
        public int Cuts { get; private set; }
        public int MaxSwarm { get; private set; }
        public float TimeToFirstCapture { get; private set; } = -1f;

        public void Initialize(MatchDirector match, TerritorySystem territory, SwarmController swarm)
        {
            _match = match;
            _territory = territory;
            _swarm = swarm;

            territory.CaptureCompleted += OnCaptureCompleted;
            territory.TrailCut += OnTrailCut;
            swarm.CountChanged += OnSwarmCountChanged;
            MaxSwarm = swarm.Count;
        }

        private void OnDestroy()
        {
            if (_territory != null)
            {
                _territory.CaptureCompleted -= OnCaptureCompleted;
                _territory.TrailCut -= OnTrailCut;
            }
            if (_swarm != null)
                _swarm.CountChanged -= OnSwarmCountChanged;
        }

        private void Update()
        {
            if (_match == null || !_match.HasStarted || _match.IsEnded) return;
            _elapsed += Time.deltaTime;
        }

        private void OnCaptureCompleted(float percent, int cells)
        {
            Captures++;
            if (TimeToFirstCapture < 0f)
                TimeToFirstCapture = _elapsed;
        }

        private void OnTrailCut()
        {
            Cuts++;
        }

        private void OnSwarmCountChanged(int count)
        {
            if (count > MaxSwarm) MaxSwarm = count;
        }
    }
}
