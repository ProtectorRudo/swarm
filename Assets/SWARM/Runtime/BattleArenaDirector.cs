using System;
using System.Collections.Generic;
using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Authoritative free-for-all coordinator for SWARM 0.4.
    /// Eight armies share one visible arena; combat has no attack button and no player-specific targeting rules.
    /// </summary>
    public sealed class BattleArenaDirector : MonoBehaviour
    {
        private const float CombatRadius = 0.86f;
        private const float CombatInterval = 0.52f;
        private const float StartPeaceSeconds = 2.25f;
        private const float RespawnProtectionSeconds = 2.4f;
        private const int RespawnSwarm = 5;

        private readonly List<RivalBot> _bots = new List<RivalBot>(BattlePalette.ParticipantCount - 1);
        private readonly float[,] _nextCombatTime = new float[BattlePalette.ParticipantCount + 1, BattlePalette.ParticipantCount + 1];
        private readonly float[] _invulnerableUntil = new float[BattlePalette.ParticipantCount + 1];
        private readonly int[] _kills = new int[BattlePalette.ParticipantCount + 1];
        private readonly int[] _deaths = new int[BattlePalette.ParticipantCount + 1];

        private Transform _player;
        private PlayerMotor _playerMotor;
        private SwarmController _playerSwarm;
        private TerritorySystem _territory;
        private Vector2 _halfExtents;
        private Vector3 _playerBase;
        private float _paintTimer;
        private float _startPeaceUntil;

        public bool IsRunning { get; private set; }
        public bool IsFinalRush { get; private set; }
        public IReadOnlyList<RivalBot> Bots => _bots;
        public Transform PlayerTransform => _player;
        public SwarmController PlayerSwarm => _playerSwarm;
        public Vector3 PlayerBase => _playerBase;

        public event Action<int, int, bool> CombatResolved;
        public event Action<int, int> ParticipantDefeated;
        public event Action<int, int, int> TerritoryConverted;

        public void Initialize(
            Transform player,
            PlayerMotor playerMotor,
            SwarmController playerSwarm,
            TerritorySystem territory,
            Vector2 halfExtents)
        {
            _player = player;
            _playerMotor = playerMotor;
            _playerSwarm = playerSwarm;
            _territory = territory;
            _halfExtents = halfExtents;
            _playerBase = BattlePalette.BasePosition(BattlePalette.PlayerOwner, halfExtents);
        }

        public void RegisterBot(RivalBot bot)
        {
            if (bot == null || _bots.Contains(bot)) return;
            _bots.Add(bot);
        }

        public void SetRunning(bool running)
        {
            if (IsRunning == running) return;
            IsRunning = running;
            if (running)
            {
                _startPeaceUntil = Time.time + StartPeaceSeconds;
                _paintTimer = 0f;
            }
        }

        public void SetFinalRush(bool active)
        {
            IsFinalRush = active;
        }

        public int GetCount(int ownerId)
        {
            if (ownerId == BattlePalette.PlayerOwner)
                return _playerSwarm != null ? _playerSwarm.Count : 0;
            RivalBot bot = FindBot(ownerId);
            return bot != null ? bot.Count : 0;
        }

        public Vector3 GetPosition(int ownerId)
        {
            if (ownerId == BattlePalette.PlayerOwner)
                return _player != null ? _player.position : Vector3.zero;
            RivalBot bot = FindBot(ownerId);
            return bot != null ? bot.transform.position : Vector3.zero;
        }

        public int GetKills(int ownerId)
        {
            return ownerId >= 1 && ownerId < _kills.Length ? _kills[ownerId] : 0;
        }

        public int GetDeaths(int ownerId)
        {
            return ownerId >= 1 && ownerId < _deaths.Length ? _deaths[ownerId] : 0;
        }

        /// <summary>
        /// Ranking is deliberately almost pure current army size so the win condition is instantly legible.
        /// KOs and territory only break exact ties; they already matter indirectly through absorption and defense.
        /// </summary>
        public float GetScore(int ownerId)
        {
            if (ownerId < 1 || ownerId > BattlePalette.ParticipantCount) return 0f;
            float tieBreak = _kills[ownerId] * 0.001f;
            if (_territory != null) tieBreak += _territory.GetOwnedPercent(ownerId) * 0.0001f;
            return GetCount(ownerId) + tieBreak;
        }

        public int GetLeaderOwner()
        {
            int leader = 1;
            float best = GetScore(leader);
            for (int owner = 2; owner <= BattlePalette.ParticipantCount; owner++)
            {
                float score = GetScore(owner);
                if (score > best)
                {
                    best = score;
                    leader = owner;
                }
            }
            return leader;
        }

        public float GetDefenseMultiplier(int ownerId, Vector3 position)
        {
            Vector3 home = BattlePalette.BasePosition(ownerId, _halfExtents);
            if ((position - home).sqrMagnitude <= 2.0f * 2.0f)
                return 1.45f;
            if (_territory != null && _territory.GetOwnerAtWorldPosition(position) == ownerId)
                return 1.16f;
            return 1f;
        }

        public bool IsInvulnerable(int ownerId)
        {
            return ownerId >= 1 && ownerId < _invulnerableUntil.Length && Time.time < _invulnerableUntil[ownerId];
        }

        public bool TryFindThreat(int selfOwner, Vector3 selfPosition, int selfCount, float radius, out Vector3 threatPosition)
        {
            float bestDistanceSq = radius * radius;
            int bestOwner = 0;
            threatPosition = Vector3.zero;

            for (int owner = 1; owner <= BattlePalette.ParticipantCount; owner++)
            {
                if (owner == selfOwner || IsInvulnerable(owner)) continue;
                int otherCount = GetCount(owner);
                if (otherCount < selfCount + 4) continue;

                Vector3 position = GetPosition(owner);
                float distanceSq = (position - selfPosition).sqrMagnitude;
                if (distanceSq >= bestDistanceSq) continue;

                bestDistanceSq = distanceSq;
                bestOwner = owner;
                threatPosition = position;
            }

            return bestOwner != 0;
        }

        public bool TryFindPrey(int selfOwner, Vector3 selfPosition, int selfCount, float radius, out Vector3 preyPosition, out int preyOwner)
        {
            float bestDistanceSq = radius * radius;
            preyOwner = 0;
            preyPosition = Vector3.zero;

            for (int owner = 1; owner <= BattlePalette.ParticipantCount; owner++)
            {
                if (owner == selfOwner || IsInvulnerable(owner)) continue;
                int otherCount = GetCount(owner);
                if (selfCount < otherCount + 4) continue;

                Vector3 position = GetPosition(owner);
                float distanceSq = (position - selfPosition).sqrMagnitude;
                if (distanceSq >= bestDistanceSq) continue;

                bestDistanceSq = distanceSq;
                preyOwner = owner;
                preyPosition = position;
            }

            return preyOwner != 0;
        }

        private void Update()
        {
            if (!IsRunning) return;

            _paintTimer -= Time.deltaTime;
            if (_paintTimer <= 0f)
            {
                _paintTimer = 0.11f;
                PaintAllParticipants();
            }

            if (Time.time < _startPeaceUntil) return;

            for (int i = 0; i < _bots.Count; i++)
                TryResolvePair(BattlePalette.PlayerOwner, _bots[i].OwnerId);

            for (int i = 0; i < _bots.Count; i++)
            {
                for (int j = i + 1; j < _bots.Count; j++)
                    TryResolvePair(_bots[i].OwnerId, _bots[j].OwnerId);
            }
        }

        private void PaintAllParticipants()
        {
            PaintParticipant(BattlePalette.PlayerOwner, _player != null ? _player.position : Vector3.zero, _playerSwarm);
            for (int i = 0; i < _bots.Count; i++)
            {
                RivalBot bot = _bots[i];
                if (bot == null) continue;
                PaintParticipant(bot.OwnerId, bot.transform.position, bot.Swarm);
            }
        }

        private void PaintParticipant(int ownerId, Vector3 position, SwarmController swarm)
        {
            if (_territory == null || swarm == null || swarm.Count <= 0) return;

            int changed = _territory.Paint(ownerId, position, swarm.Count, out int enemyCells);
            if (changed <= 0) return;

            if (enemyCells >= 4 && swarm.Count > 3)
            {
                int requestedCost = Mathf.Max(1, Mathf.CeilToInt(enemyCells / 14f));
                int affordable = Mathf.Min(requestedCost, swarm.Count - 3);
                swarm.RemoveUnits(affordable);
            }

            if (enemyCells > 0)
                TerritoryConverted?.Invoke(ownerId, changed, enemyCells);
        }

        private void TryResolvePair(int ownerA, int ownerB)
        {
            if (IsInvulnerable(ownerA) || IsInvulnerable(ownerB)) return;
            if (Time.time < _nextCombatTime[ownerA, ownerB]) return;

            Vector3 positionA = GetPosition(ownerA);
            Vector3 positionB = GetPosition(ownerB);
            if ((positionA - positionB).sqrMagnitude > CombatRadius * CombatRadius) return;

            _nextCombatTime[ownerA, ownerB] = Time.time + CombatInterval;
            _nextCombatTime[ownerB, ownerA] = Time.time + CombatInterval;

            int countA = GetCount(ownerA);
            int countB = GetCount(ownerB);
            if (countA <= 0 || countB <= 0) return;

            float strengthA = countA * GetDefenseMultiplier(ownerA, positionA);
            float strengthB = countB * GetDefenseMultiplier(ownerB, positionB);

            int winner = strengthA >= strengthB ? ownerA : ownerB;
            int loser = winner == ownerA ? ownerB : ownerA;
            float winnerStrength = Mathf.Max(strengthA, strengthB);
            float loserStrength = Mathf.Min(strengthA, strengthB);
            int loserBefore = GetCount(loser);
            int winnerBefore = GetCount(winner);

            bool decisive = winnerStrength >= loserStrength * 1.22f || loserBefore <= 4;
            if (decisive)
            {
                int absorbed = Mathf.Clamp(Mathf.FloorToInt(loserBefore * 0.34f), 2, 12);
                AddUnits(winner, absorbed);
                CombatResolved?.Invoke(winner, loser, true);
                DefeatParticipant(loser, winner);
                return;
            }

            int loserDamage = Mathf.Clamp(2 + Mathf.FloorToInt(winnerBefore * 0.045f), 2, 5);
            int winnerDamage = Mathf.Clamp(1 + Mathf.FloorToInt(loserBefore * 0.018f), 1, 2);
            RemoveUnits(loser, loserDamage);
            RemoveUnits(winner, winnerDamage);
            CombatResolved?.Invoke(winner, loser, false);

            RivalBot loserBot = FindBot(loser);
            if (loserBot != null) loserBot.NotifyCombatOutcome(false, GetPosition(winner));
            RivalBot winnerBot = FindBot(winner);
            if (winnerBot != null) winnerBot.NotifyCombatOutcome(true, GetPosition(loser));

            if (GetCount(loser) <= 2)
                DefeatParticipant(loser, winner);
            else if (GetCount(winner) <= 2)
                DefeatParticipant(winner, loser);
        }

        private void DefeatParticipant(int loser, int winner)
        {
            _kills[winner]++;
            _deaths[loser]++;
            _invulnerableUntil[loser] = Time.time + RespawnProtectionSeconds;

            if (loser == BattlePalette.PlayerOwner)
            {
                if (_playerSwarm != null) _playerSwarm.SetCount(RespawnSwarm);
                if (_playerMotor != null) _playerMotor.Teleport(_playerBase);
                if (_playerSwarm != null) _playerSwarm.SnapHistoryToAnchor();
            }
            else
            {
                RivalBot bot = FindBot(loser);
                if (bot != null) bot.Respawn(RespawnSwarm, RespawnProtectionSeconds);
            }

            RivalBot winnerBot = FindBot(winner);
            if (winnerBot != null) winnerBot.NotifyCombatOutcome(true, BattlePalette.BasePosition(loser, _halfExtents));

            ParticipantDefeated?.Invoke(winner, loser);
        }

        private void AddUnits(int ownerId, int amount)
        {
            if (amount <= 0) return;
            if (ownerId == BattlePalette.PlayerOwner)
            {
                if (_playerSwarm != null) _playerSwarm.AddUnits(amount);
                return;
            }

            RivalBot bot = FindBot(ownerId);
            if (bot != null) bot.AddUnits(amount);
        }

        private void RemoveUnits(int ownerId, int amount)
        {
            if (amount <= 0) return;
            if (ownerId == BattlePalette.PlayerOwner)
            {
                if (_playerSwarm != null) _playerSwarm.RemoveUnits(amount);
                return;
            }

            RivalBot bot = FindBot(ownerId);
            if (bot != null) bot.RemoveUnits(amount);
        }

        private RivalBot FindBot(int ownerId)
        {
            for (int i = 0; i < _bots.Count; i++)
            {
                RivalBot bot = _bots[i];
                if (bot != null && bot.OwnerId == ownerId) return bot;
            }
            return null;
        }
    }
}
