using System;
using System.Collections.Generic;
using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// 0.3 rival: grows, paints, evaluates fights and respects anti-camping windows.
    /// It never hunts an exposed trail because trails no longer exist.
    /// </summary>
    public sealed class RivalBot : MonoBehaviour
    {
        private const int MaxVisibleFollowers = 45;
        private const float BaseSpeed = 3.15f;
        private const float CombatRadius = 0.92f;

        private readonly List<Transform> _followers = new List<Transform>(MaxVisibleFollowers);

        private TerritorySystem _territory;
        private Vector2 _halfExtents;
        private Transform _player;
        private PlayerMotor _playerMotor;
        private SwarmController _playerSwarm;
        private Transform _bodyTransform;
        private SpriteRenderer _body;
        private SpriteRenderer _ring;
        private Vector3 _moveTarget;
        private Vector2 _lastMoveDirection = Vector2.down;
        private float _decisionTimer;
        private float _paintTimer;
        private float _combatCooldown;
        private float _huntCooldown;
        private float _huntTimer;
        private float _peaceTimer = 6.5f;
        private float _phase;

        public int Count { get; private set; }
        public event Action<bool, int, int> CombatResolved;
        public event Action<bool> Defeated;

        public void Initialize(
            TerritorySystem territory,
            Vector2 halfExtents,
            Transform player,
            PlayerMotor playerMotor,
            SwarmController playerSwarm)
        {
            _territory = territory;
            _halfExtents = halfExtents;
            _player = player;
            _playerMotor = playerMotor;
            _playerSwarm = playerSwarm;
            transform.position = RivalHomePosition();
            _moveTarget = new Vector3(-halfExtents.x * 0.48f, halfExtents.y * 0.52f, 0f);
            BuildVisual();
            SetCount(10);
        }

        public void AddUnits(int amount)
        {
            if (amount <= 0) return;
            SetCount(Count + amount);
        }

        private void SetCount(int count)
        {
            Count = Mathf.Max(0, count);
            EnsureFollowerVisuals(Mathf.Min(Count, MaxVisibleFollowers));
        }

        private void RemoveUnits(int amount)
        {
            if (amount <= 0) return;
            SetCount(Mathf.Max(0, Count - amount));
        }

        private void BuildVisual()
        {
            _bodyTransform = new GameObject("RivalBody").transform;
            _bodyTransform.SetParent(transform, false);
            _bodyTransform.localScale = Vector3.one * 0.92f;
            _body = _bodyTransform.gameObject.AddComponent<SpriteRenderer>();
            _body.sprite = RuntimeArt.Circle;
            RuntimeArt.Configure(_body);
            _body.color = new Color(1f, 0.24f, 0.32f, 1f);
            _body.sortingOrder = 19;

            var ring = new GameObject("DangerRing").transform;
            ring.SetParent(transform, false);
            ring.localScale = Vector3.one * 1.42f;
            _ring = ring.gameObject.AddComponent<SpriteRenderer>();
            _ring.sprite = RuntimeArt.Circle;
            RuntimeArt.Configure(_ring);
            _ring.color = new Color(1f, 0.15f, 0.22f, 0.13f);
            _ring.sortingOrder = 18;
        }

        private void EnsureFollowerVisuals(int desired)
        {
            while (_followers.Count < desired)
            {
                int index = _followers.Count;
                var follower = new GameObject("RivalFollower_" + index.ToString("00")).transform;
                follower.SetParent(transform, false);
                follower.position = transform.position;
                var renderer = follower.gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeArt.Circle;
                RuntimeArt.Configure(renderer);
                renderer.color = new Color(1f, 0.31f + (index % 4) * 0.025f, 0.37f, 0.88f);
                renderer.sortingOrder = 12 + index % 3;
                _followers.Add(follower);
            }
        }

        private void Update()
        {
            if (_territory == null || _player == null || _playerSwarm == null) return;

            _peaceTimer = Mathf.Max(0f, _peaceTimer - Time.deltaTime);
            _combatCooldown = Mathf.Max(0f, _combatCooldown - Time.deltaTime);
            _huntCooldown = Mathf.Max(0f, _huntCooldown - Time.deltaTime);
            _huntTimer = Mathf.Max(0f, _huntTimer - Time.deltaTime);
            _decisionTimer -= Time.deltaTime;
            _paintTimer -= Time.deltaTime;

            if (_decisionTimer <= 0f)
            {
                _decisionTimer = 0.24f;
                ChooseTarget();
            }

            MoveTowardTarget();

            if (_paintTimer <= 0f)
            {
                _paintTimer = 0.10f;
                int invadedCells = _territory.PaintRival(transform.position, Count);
                if (invadedCells > 0 && Count > 3)
                {
                    int requestedCost = Mathf.Max(1, Mathf.CeilToInt(invadedCells / 8f));
                    RemoveUnits(Mathf.Min(requestedCost, Count - 3));
                }
            }

            float distanceSq = (_player.position - transform.position).sqrMagnitude;
            if (_peaceTimer <= 0f && _combatCooldown <= 0f && distanceSq <= CombatRadius * CombatRadius)
                ResolveCombat();

            UpdateVisuals();
        }

        private void ChooseTarget()
        {
            Vector3 toPlayer = _player.position - transform.position;
            float distance = toPlayer.magnitude;
            bool standingOnPlayerGround = _territory.GetOwnerAtWorldPosition(transform.position) == TerritorySystem.PlayerOwned;

            if (_peaceTimer > 0f)
            {
                if (distance < 5.2f)
                {
                    Vector3 away = distance > 0.01f ? -toPlayer.normalized : Vector3.right;
                    _moveTarget = ClampTarget(transform.position + away * 5f);
                }
                else
                {
                    ContinueWander();
                }
                return;
            }

            if ((Count + 2 < _playerSwarm.Count && distance < 4.4f) ||
                (standingOnPlayerGround && Count < Mathf.CeilToInt(_playerSwarm.Count * 1.35f)))
            {
                Vector3 away = distance > 0.01f ? -toPlayer.normalized : Vector3.right;
                _moveTarget = ClampTarget(transform.position + away * 5.5f);
                _huntTimer = 0f;
                return;
            }

            if (_huntTimer > 0f)
            {
                _moveTarget = _player.position;
                return;
            }

            if (_huntCooldown <= 0f && Count >= _playerSwarm.Count + 4 && distance < 5.2f)
            {
                _huntTimer = 1.55f;
                _huntCooldown = 5.2f;
                _moveTarget = _player.position;
                return;
            }

            ContinueWander();
        }

        private void ContinueWander()
        {
            if ((_moveTarget - transform.position).sqrMagnitude > 1.2f) return;

            float t = Time.time * 0.41f + 0.7f;
            _moveTarget = new Vector3(
                Mathf.Sin(t * 1.27f) * _halfExtents.x * 0.76f,
                Mathf.Cos(t * 0.93f) * _halfExtents.y * 0.76f,
                0f);
        }

        private void MoveTowardTarget()
        {
            Vector3 delta = _moveTarget - transform.position;
            if (delta.sqrMagnitude <= 0.01f) return;

            Vector3 direction = delta.normalized;
            _lastMoveDirection = direction;
            float speedScale = _huntTimer > 0f ? 1.08f : 1f;
            transform.position += direction * (BaseSpeed * speedScale * Time.deltaTime);

            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, -_halfExtents.x + 0.45f, _halfExtents.x - 0.45f);
            p.y = Mathf.Clamp(p.y, -_halfExtents.y + 0.45f, _halfExtents.y - 0.45f);
            transform.position = p;
        }

        private void ResolveCombat()
        {
            _combatCooldown = 0.58f;

            bool playerDefending = _territory.GetOwnerAtWorldPosition(_player.position) == TerritorySystem.PlayerOwned;
            bool rivalDefending = _territory.GetOwnerAtWorldPosition(transform.position) == TerritorySystem.RivalOwned;
            float playerPower = _playerSwarm.Count * (playerDefending ? 1.35f : 1f);
            float rivalPower = Count * (rivalDefending ? 1.35f : 1f);

            if (playerPower >= rivalPower)
            {
                int rivalBefore = Count;
                int playerBefore = _playerSwarm.Count;
                int rivalDamage = Mathf.Max(2, Mathf.CeilToInt(playerBefore * 0.20f));
                int playerDamage = Mathf.Max(1, Mathf.CeilToInt(rivalBefore * 0.07f));

                int actualRivalDamage = Mathf.Min(rivalDamage, Count);
                int playerAffordable = Mathf.Max(0, _playerSwarm.Count - 3);
                _playerSwarm.RemoveUnits(Mathf.Min(playerDamage, playerAffordable));
                RemoveUnits(actualRivalDamage);
                _playerSwarm.AddUnits(Mathf.Max(1, actualRivalDamage / 3));
                CombatResolved?.Invoke(true, _playerSwarm.Count, Count);

                if (Count <= 2)
                    DefeatRival();
            }
            else
            {
                int rivalBefore = Count;
                int playerBefore = _playerSwarm.Count;
                int playerDamage = Mathf.Max(2, Mathf.CeilToInt(rivalBefore * 0.18f));
                int rivalDamage = Mathf.Max(1, Mathf.CeilToInt(playerBefore * 0.07f));

                int actualPlayerDamage = Mathf.Min(playerDamage, _playerSwarm.Count);
                int rivalAffordable = Mathf.Max(0, Count - 3);
                RemoveUnits(Mathf.Min(rivalDamage, rivalAffordable));
                _playerSwarm.RemoveUnits(actualPlayerDamage);
                AddUnits(Mathf.Max(1, actualPlayerDamage / 3));
                CombatResolved?.Invoke(false, _playerSwarm.Count, Count);

                if (_playerSwarm.Count <= 2)
                    DefeatPlayer();
            }
        }

        private void DefeatRival()
        {
            Defeated?.Invoke(false);
            transform.position = RivalHomePosition();
            SetCount(9);
            _peaceTimer = 3.8f;
            _huntCooldown = 5.5f;
            _huntTimer = 0f;
            _moveTarget = new Vector3(_halfExtents.x * 0.35f, _halfExtents.y * 0.40f, 0f);
        }

        private void DefeatPlayer()
        {
            Defeated?.Invoke(true);
            _playerSwarm.SetCount(5);
            _playerMotor.Teleport(Vector2.zero);
            _playerSwarm.SnapHistoryToAnchor();
            _peaceTimer = 4.2f;
            _huntCooldown = 6f;
            _huntTimer = 0f;
            transform.position = RivalHomePosition();
        }

        private void UpdateVisuals()
        {
            _phase += Time.deltaTime * 3.6f;
            if (_bodyTransform != null)
            {
                float countScale = 0.92f + Mathf.Min(Count, 45) * 0.0045f;
                float pulse = 1f + Mathf.Sin(_phase) * 0.025f;
                _bodyTransform.localScale = Vector3.one * countScale * pulse;
            }

            if (_ring != null)
            {
                bool stronger = _playerSwarm != null && Count >= _playerSwarm.Count + 3;
                _ring.color = stronger
                    ? new Color(1f, 0.12f, 0.20f, 0.20f)
                    : new Color(1f, 0.20f, 0.26f, 0.08f);
            }

            int visible = Mathf.Min(Count, MaxVisibleFollowers);
            Vector2 forward = _lastMoveDirection.sqrMagnitude > 0.01f ? _lastMoveDirection.normalized : Vector2.down;
            Vector2 right = new Vector2(forward.y, -forward.x);
            int columns = visible >= 24 ? 5 : 4;

            for (int i = 0; i < _followers.Count; i++)
            {
                bool active = i < visible;
                if (_followers[i].gameObject.activeSelf != active)
                    _followers[i].gameObject.SetActive(active);
                if (!active) continue;

                int row = i / columns + 1;
                int column = i % columns;
                float center = (columns - 1) * 0.5f;
                float lane = (column - center) * 0.34f;
                Vector3 target = transform.position - (Vector3)(forward * (row * 0.40f)) + (Vector3)(right * lane);
                target += (Vector3)(right * (Mathf.Sin(Time.time * 4f + i * 1.4f) * 0.05f));
                _followers[i].position = Vector3.Lerp(_followers[i].position, target, 1f - Mathf.Exp(-12f * Time.deltaTime));
                _followers[i].localScale = Vector3.one * (0.60f + (i % 4) * 0.025f);
            }
        }

        private Vector3 RivalHomePosition()
        {
            return new Vector3(_halfExtents.x * 0.58f, _halfExtents.y * 0.56f, 0f);
        }

        private Vector3 ClampTarget(Vector3 target)
        {
            target.x = Mathf.Clamp(target.x, -_halfExtents.x + 0.8f, _halfExtents.x - 0.8f);
            target.y = Mathf.Clamp(target.y, -_halfExtents.y + 0.8f, _halfExtents.y - 0.8f);
            target.z = 0f;
            return target;
        }
    }
}
