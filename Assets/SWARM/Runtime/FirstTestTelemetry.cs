using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Local-only telemetry for the 0.3 core test. No network, persistence or external SDK.
    /// </summary>
    public sealed class FirstTestTelemetry : MonoBehaviour
    {
        private MatchDirector _match;
        private TerritorySystem _territory;
        private SwarmController _swarm;
        private RivalBot _rival;
        private float _elapsed;
        private int _startingSwarm;

        public int ExpansionBursts { get; private set; }
        public int EnemyCellsTaken { get; private set; }
        public int FightsWon { get; private set; }
        public int FightsLost { get; private set; }
        public int MaxSwarm { get; private set; }
        public float TimeToFirstGrowth { get; private set; } = -1f;
        public float TimeToFirstFight { get; private set; } = -1f;

        public void Initialize(MatchDirector match, TerritorySystem territory, SwarmController swarm, RivalBot rival)
        {
            _match = match;
            _territory = territory;
            _swarm = swarm;
            _rival = rival;
            _startingSwarm = swarm.Count;

            territory.PlayerExpanded += OnPlayerExpanded;
            swarm.CountChanged += OnSwarmCountChanged;
            if (rival != null)
                rival.CombatResolved += OnCombatResolved;

            MaxSwarm = swarm.Count;
        }

        private void OnDestroy()
        {
            if (_territory != null)
                _territory.PlayerExpanded -= OnPlayerExpanded;
            if (_swarm != null)
                _swarm.CountChanged -= OnSwarmCountChanged;
            if (_rival != null)
                _rival.CombatResolved -= OnCombatResolved;
        }

        private void Update()
        {
            if (_match == null || !_match.HasStarted || _match.IsEnded) return;
            _elapsed += Time.deltaTime;
        }

        private void OnPlayerExpanded(float percent, int cells, int enemyCells)
        {
            ExpansionBursts++;
            EnemyCellsTaken += enemyCells;
        }

        private void OnSwarmCountChanged(int count)
        {
            if (count > MaxSwarm) MaxSwarm = count;
            if (TimeToFirstGrowth < 0f && count > _startingSwarm)
                TimeToFirstGrowth = _elapsed;
        }

        private void OnCombatResolved(bool playerAdvantage, int playerCount, int rivalCount)
        {
            if (playerAdvantage) FightsWon++;
            else FightsLost++;
            if (TimeToFirstFight < 0f)
                TimeToFirstFight = _elapsed;
        }
    }
}
