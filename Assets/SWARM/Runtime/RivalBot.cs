using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// One autonomous army in the 0.4 free-for-all. Bots have no preference for the human player:
    /// they collect, flee stronger armies, hunt weaker armies, shoot nearby armies and contest the center.
    /// </summary>
    public sealed class RivalBot : MonoBehaviour
    {
        private BattleArenaDirector _battle;
        private PickupSystem _pickups;
        private TerritorySystem _territory;
        private Vector2 _halfExtents;
        private Vector3 _home;
        private Vector3 _moveTarget;
        private Vector2 _moveDirection = Vector2.down;
        private SwarmController _swarm;
        private Transform _bodyRoot;
        private SpriteRenderer _body;
        private SpriteRenderer _ring;
        private float _decisionTimer;
        private float _fleeTimer;
        private float _huntTimer;
        private float _respawnCalmTimer;
        private float _underFireTimer;
        private float _phase;
        private float _baseSpeed;
        private int _huntOwner;

        public int OwnerId { get; private set; }
        public int Count => _swarm != null ? _swarm.Count : 0;
        public SwarmController Swarm => _swarm;
        public Vector3 Home => _home;

        public void Initialize(
            int ownerId,
            TerritorySystem territory,
            Vector2 halfExtents,
            BattleArenaDirector battle)
        {
            OwnerId = ownerId;
            _territory = territory;
            _halfExtents = halfExtents;
            _battle = battle;
            _home = BattlePalette.BasePosition(ownerId, halfExtents);
            _baseSpeed = 3.65f + (ownerId % 3) * 0.17f;
            transform.position = _home;
            _moveTarget = Vector3.Lerp(_home, Vector3.zero, 0.34f);

            BuildVisual();
            BuildSwarm();
            SetCount(3);
        }

        public void BindPickups(PickupSystem pickups)
        {
            _pickups = pickups;
        }

        public void AddUnits(int amount)
        {
            if (amount > 0 && _swarm != null) _swarm.AddUnits(amount);
        }

        public void RemoveUnits(int amount)
        {
            if (amount > 0 && _swarm != null) _swarm.RemoveUnits(amount);
        }

        public void SetCount(int count)
        {
            if (_swarm != null) _swarm.SetCount(Mathf.Max(0, count));
        }

        public void Respawn(int count, float calmSeconds)
        {
            transform.position = _home;
            SetCount(count);
            if (_swarm != null) _swarm.SnapHistoryToAnchor();
            _respawnCalmTimer = Mathf.Max(_respawnCalmTimer, calmSeconds);
            _fleeTimer = 0f;
            _huntTimer = 0f;
            _underFireTimer = 0f;
            _huntOwner = 0;
            _moveTarget = Vector3.Lerp(_home, Vector3.zero, 0.28f);
        }

        public void NotifyCombatOutcome(bool won, Vector3 otherPosition)
        {
            if (won)
            {
                _huntTimer = 0.75f;
                _fleeTimer = 0f;
                return;
            }

            _huntTimer = 0f;
            _huntOwner = 0;
            _fleeTimer = 1.55f;
            Vector3 away = transform.position - otherPosition;
            if (away.sqrMagnitude < 0.01f) away = _home - transform.position;
            _moveTarget = ClampTarget(transform.position + away.normalized * 4.5f);
        }

        public void NotifyUnderFire(Vector3 shooterPosition)
        {
            _underFireTimer = 0.85f;
            _decisionTimer = 0f;

            if (Count <= 8)
            {
                _fleeTimer = Mathf.Max(_fleeTimer, 0.95f);
                Vector3 away = transform.position - shooterPosition;
                Vector3 homeDirection = (_home - transform.position).sqrMagnitude > 0.01f
                    ? (_home - transform.position).normalized
                    : Vector3.zero;
                Vector3 escape = away.sqrMagnitude > 0.01f
                    ? (away.normalized * 0.45f + homeDirection * 0.55f).normalized
                    : homeDirection;
                _moveTarget = ClampTarget(transform.position + escape * 4.4f);
            }
        }

        /// <summary>
        /// Shooting target query is symmetric: nearest valid army in range, regardless of whether it is human or bot.
        /// </summary>
        public bool TryGetFireDirection(out Vector2 direction)
        {
            direction = _moveDirection.sqrMagnitude > 0.01f ? _moveDirection.normalized : Vector2.down;
            if (_battle == null || !_battle.CanFight || Count <= 0 || _respawnCalmTimer > 0f) return false;

            float range = _battle.IsFinalRush ? 9.2f : 7.2f;
            if (!_battle.TryFindNearestTarget(OwnerId, transform.position, range, out Vector3 target, out _))
                return false;

            Vector2 delta = target - transform.position;
            if (delta.sqrMagnitude <= 0.02f) return false;
            direction = delta.normalized;
            return true;
        }

        private void BuildVisual()
        {
            Color color = BattlePalette.Color(OwnerId);

            _bodyRoot = new GameObject(BattlePalette.Name(OwnerId) + "_Body").transform;
            _bodyRoot.SetParent(transform, false);
            _bodyRoot.localScale = Vector3.one * 0.68f;

            _body = _bodyRoot.gameObject.AddComponent<SpriteRenderer>();
            _body.sprite = RuntimeArt.Circle;
            RuntimeArt.Configure(_body);
            _body.color = color;
            _body.sortingOrder = 24;

            var eyeLeft = CreateEye("Eye_L", new Vector2(-0.18f, 0.13f));
            var eyeRight = CreateEye("Eye_R", new Vector2(0.18f, 0.13f));
            eyeLeft.localScale = eyeRight.localScale = Vector3.one * 0.16f;

            var ring = new GameObject("PowerRing").transform;
            ring.SetParent(transform, false);
            ring.localScale = Vector3.one * 0.92f;
            _ring = ring.gameObject.AddComponent<SpriteRenderer>();
            _ring.sprite = RuntimeArt.Circle;
            RuntimeArt.Configure(_ring);
            Color ringColor = color;
            ringColor.a = 0.14f;
            _ring.color = ringColor;
            _ring.sortingOrder = 22;
        }

        private Transform CreateEye(string name, Vector2 localPosition)
        {
            var eye = new GameObject(name).transform;
            eye.SetParent(_bodyRoot, false);
            eye.localPosition = localPosition;
            var renderer = eye.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArt.Circle;
            RuntimeArt.Configure(renderer);
            renderer.color = new Color(0.08f, 0.10f, 0.15f, 1f);
            renderer.sortingOrder = 25;
            return eye;
        }

        private void BuildSwarm()
        {
            var visuals = new GameObject(BattlePalette.Name(OwnerId) + "_Swarm");
            visuals.transform.SetParent(transform.parent, false);
            _swarm = visuals.AddComponent<SwarmController>();
            _swarm.Initialize(transform, null);
            _swarm.SetVisualColor(BattlePalette.Color(OwnerId));
        }

        private void Update()
        {
            UpdateVisuals();
            if (_battle == null || !_battle.IsRunning) return;

            _respawnCalmTimer = Mathf.Max(0f, _respawnCalmTimer - Time.deltaTime);
            _fleeTimer = Mathf.Max(0f, _fleeTimer - Time.deltaTime);
            _huntTimer = Mathf.Max(0f, _huntTimer - Time.deltaTime);
            _underFireTimer = Mathf.Max(0f, _underFireTimer - Time.deltaTime);
            _decisionTimer -= Time.deltaTime;

            if (_decisionTimer <= 0f)
            {
                _decisionTimer = 0.20f + (OwnerId % 4) * 0.018f;
                ChooseTarget();
            }

            MoveTowardTarget();
        }

        private void ChooseTarget()
        {
            if (_respawnCalmTimer > 0f)
            {
                _moveTarget = Vector3.Lerp(_home, Vector3.zero, 0.24f);
                return;
            }

            if (_underFireTimer > 0f && Count <= 8)
            {
                _moveTarget = _home;
                return;
            }

            if (_battle.TryFindThreat(OwnerId, transform.position, Count, 3.7f, out Vector3 threat))
            {
                _fleeTimer = Mathf.Max(_fleeTimer, 1.2f);
                Vector3 away = transform.position - threat;
                Vector3 safeDirection = (_home - transform.position).normalized;
                if (away.sqrMagnitude > 0.01f)
                    safeDirection = (safeDirection * 0.70f + away.normalized * 0.30f).normalized;
                _moveTarget = ClampTarget(transform.position + safeDirection * 5.2f);
                return;
            }

            if (_fleeTimer > 0f)
            {
                _moveTarget = _home;
                return;
            }

            float huntRadius = _battle.IsFinalRush ? 10f : 5.5f + (OwnerId % 3) * 0.55f;
            if (Count >= 8 && _battle.TryFindPrey(OwnerId, transform.position, Count, huntRadius, out Vector3 prey, out int preyOwner))
            {
                _huntOwner = preyOwner;
                _huntTimer = 1.25f;
                _moveTarget = prey;
                return;
            }

            if (_huntTimer > 0f && _huntOwner != 0)
            {
                _moveTarget = _battle.GetPosition(_huntOwner);
                return;
            }

            bool preferCenter = _battle.IsFinalRush || Count >= 14;
            if (_pickups != null && _pickups.TryFindBestPickup(transform.position, preferCenter, out Vector3 pickupTarget))
            {
                _moveTarget = pickupTarget;
                return;
            }

            if ((_moveTarget - transform.position).sqrMagnitude < 0.8f)
            {
                float t = Time.time * (0.37f + OwnerId * 0.013f);
                _moveTarget = ClampTarget(new Vector3(
                    Mathf.Sin(t + OwnerId * 0.83f) * _halfExtents.x * 0.68f,
                    Mathf.Cos(t * 0.83f + OwnerId * 0.51f) * _halfExtents.y * 0.68f,
                    0f));
            }
        }

        private void MoveTowardTarget()
        {
            Vector3 delta = _moveTarget - transform.position;
            if (delta.sqrMagnitude <= 0.015f) return;

            Vector3 direction = delta.normalized;
            _moveDirection = direction;
            if (_swarm != null) _swarm.SetFacingHint(_moveDirection);

            float speed = _baseSpeed;
            if (_fleeTimer > 0f) speed *= 1.12f;
            else if (_huntTimer > 0f) speed *= 1.06f;
            if (_battle.IsFinalRush) speed *= 1.04f;

            transform.position += direction * (speed * Time.deltaTime);
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, -_halfExtents.x + 0.45f, _halfExtents.x - 0.45f);
            p.y = Mathf.Clamp(p.y, -_halfExtents.y + 0.45f, _halfExtents.y - 0.45f);
            transform.position = p;
        }

        private void UpdateVisuals()
        {
            if (_bodyRoot == null) return;

            _phase += Time.deltaTime * 3.2f;
            float countScale = 0.68f + Mathf.Min(Count, 60) * 0.0038f;
            float pulse = 1f + Mathf.Sin(_phase + OwnerId) * 0.028f;
            _bodyRoot.localScale = Vector3.one * countScale * pulse;

            if (_ring != null)
            {
                Color c = BattlePalette.Color(OwnerId);
                c.a = Count >= 25 ? 0.25f : Count >= 12 ? 0.16f : 0.09f;
                _ring.color = c;
                float ringScale = 0.92f + Mathf.Min(Count, 70) * 0.008f;
                _ring.transform.localScale = Vector3.one * ringScale;
            }
        }

        private Vector3 ClampTarget(Vector3 target)
        {
            target.x = Mathf.Clamp(target.x, -_halfExtents.x + 0.75f, _halfExtents.x - 0.75f);
            target.y = Mathf.Clamp(target.y, -_halfExtents.y + 0.75f, _halfExtents.y - 0.75f);
            target.z = 0f;
            return target;
        }
    }
}
