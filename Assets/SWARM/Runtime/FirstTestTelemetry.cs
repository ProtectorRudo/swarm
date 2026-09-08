using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Local-only telemetry for the 0.4 battle-arena phone test.
    /// </summary>
    public sealed class FirstTestTelemetry : MonoBehaviour
    {
        private MatchDirector _match;
        private TerritorySystem _territory;
        private SwarmController _swarm;
        private BattleArenaDirector _battle;
        private float _elapsed;

        public int MaxSwarm { get; private set; }
        public int PlayerKOs { get; private set; }
        public int PlayerDeaths { get; private set; }
        public float MaxTerritoryPercent { get; private set; }
        public float TimeToTen { get; private set; } = -1f;
        public float TimeToFirstCombat { get; private set; } = -1f;
        public float TimeToFirstKO { get; private set; } = -1f;

        public void Initialize(
            MatchDirector match,
            TerritorySystem territory,
            SwarmController swarm,
            BattleArenaDirector battle)
        {
            _match = match;
            _territory = territory;
            _swarm = swarm;
            _battle = battle;

            if (_swarm != null)
            {
                _swarm.CountChanged += OnSwarmCountChanged;
                MaxSwarm = _swarm.Count;
                if (_swarm.Count >= 10) TimeToTen = 0f;
            }

            if (_battle != null)
            {
                _battle.CombatResolved += OnCombatResolved;
                _battle.ParticipantDefeated += OnParticipantDefeated;
            }
        }

        private void OnDestroy()
        {
            if (_swarm != null)
                _swarm.CountChanged -= OnSwarmCountChanged;
            if (_battle != null)
            {
                _battle.CombatResolved -= OnCombatResolved;
                _battle.ParticipantDefeated -= OnParticipantDefeated;
            }
        }

        private void Update()
        {
            if (_match == null || !_match.HasStarted || _match.IsEnded) return;
            _elapsed += Time.deltaTime;
            if (_territory != null)
                MaxTerritoryPercent = Mathf.Max(MaxTerritoryPercent, _territory.PlayerOwnedPercent);
        }

        private void OnSwarmCountChanged(int count)
        {
            if (count > MaxSwarm) MaxSwarm = count;
            if (TimeToTen < 0f && count >= 10)
                TimeToTen = _elapsed;
        }

        private void OnCombatResolved(int winner, int loser, bool decisive)
        {
            if (winner != BattlePalette.PlayerOwner && loser != BattlePalette.PlayerOwner) return;
            if (TimeToFirstCombat < 0f)
                TimeToFirstCombat = _elapsed;
        }

        private void OnParticipantDefeated(int winner, int loser)
        {
            if (winner == BattlePalette.PlayerOwner)
            {
                PlayerKOs++;
                if (TimeToFirstKO < 0f) TimeToFirstKO = _elapsed;
            }
            if (loser == BattlePalette.PlayerOwner)
                PlayerDeaths++;
        }
    }
}
