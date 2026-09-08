using System.Collections.Generic;
using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Central pooled projectile simulation for the whole battle arena. No projectile owns an Update method.
    /// Human fire is one-thumb double-tap-and-hold; bots use the exact same projectile/damage rules.
    /// </summary>
    public sealed class ShootingSystem : MonoBehaviour
    {
        private sealed class ProjectileProxy
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public bool Active;
            public int OwnerId;
            public Vector2 Velocity;
            public float Life;
        }

        private const int MaxProjectiles = 192;
        private const float BulletSpeed = 12.5f;
        private const float BulletLifetime = 1.30f;
        private const float HitRadius = 0.46f;
        private const float PlayerShotInterval = 0.20f;
        private const float BotShotInterval = 0.34f;
        private const float BotFinalRushInterval = 0.28f;
        private const float AimAssistRange = 8.8f;
        private const float AimAssistDot = 0.72f;

        private readonly List<ProjectileProxy> _pool = new List<ProjectileProxy>(MaxProjectiles);
        private readonly float[] _nextShotTime = new float[BattlePalette.ParticipantCount + 1];

        private OneHandInputSource _input;
        private BattleArenaDirector _battle;
        private Vector2 _halfExtents;
        private Vector2 _lastPlayerAim = Vector2.up;
        private int _poolCursor;
        private bool _wasRunning;

        public int ActiveProjectileCount { get; private set; }

        public void Initialize(OneHandInputSource input, BattleArenaDirector battle, Vector2 halfExtents)
        {
            _input = input;
            _battle = battle;
            _halfExtents = halfExtents;
            BuildPool();

            for (int owner = 2; owner <= BattlePalette.ParticipantCount; owner++)
                _nextShotTime[owner] = Time.time + owner * 0.035f;
        }

        private void BuildPool()
        {
            for (int i = 0; i < MaxProjectiles; i++)
            {
                var go = new GameObject("Projectile_" + i.ToString("000"));
                go.transform.SetParent(transform, false);
                go.transform.localScale = new Vector3(0.18f, 0.42f, 1f);

                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeArt.Circle;
                RuntimeArt.Configure(renderer);
                renderer.sortingOrder = 34;

                go.SetActive(false);
                _pool.Add(new ProjectileProxy
                {
                    Transform = go.transform,
                    Renderer = renderer,
                    Active = false
                });
            }
        }

        private void Update()
        {
            if (_battle == null) return;

            if (!_battle.IsRunning)
            {
                if (_wasRunning) DeactivateAll();
                _wasRunning = false;
                return;
            }

            _wasRunning = true;
            UpdatePlayerAim();

            if (_battle.CanFight)
            {
                TryFirePlayer();
                TryFireBots();
            }

            SimulateProjectiles();
        }

        private void UpdatePlayerAim()
        {
            if (_input == null) return;
            if (_input.MoveIntent.sqrMagnitude > 0.05f)
                _lastPlayerAim = _input.MoveIntent.normalized;
        }

        private void TryFirePlayer()
        {
            if (_input == null || !_input.FireHeld) return;
            int owner = BattlePalette.PlayerOwner;
            if (Time.time < _nextShotTime[owner]) return;

            Vector2 direction = _lastPlayerAim.sqrMagnitude > 0.01f ? _lastPlayerAim.normalized : Vector2.up;
            direction = ApplyPlayerAimAssist(_battle.GetPosition(owner), direction);
            Fire(owner, direction);
            _nextShotTime[owner] = Time.time + PlayerShotInterval;
        }

        private void TryFireBots()
        {
            var bots = _battle.Bots;
            for (int i = 0; i < bots.Count; i++)
            {
                RivalBot bot = bots[i];
                if (bot == null) continue;
                int owner = bot.OwnerId;
                if (Time.time < _nextShotTime[owner]) continue;
                if (!bot.TryGetFireDirection(out Vector2 direction)) continue;

                Fire(owner, direction);
                float interval = _battle.IsFinalRush ? BotFinalRushInterval : BotShotInterval;
                interval += (owner % 3) * 0.015f;
                _nextShotTime[owner] = Time.time + interval;
            }
        }

        private Vector2 ApplyPlayerAimAssist(Vector3 origin, Vector2 rawDirection)
        {
            int bestOwner = 0;
            float bestScore = float.MaxValue;
            Vector2 bestDirection = rawDirection;
            float maxRangeSq = AimAssistRange * AimAssistRange;

            for (int owner = 2; owner <= BattlePalette.ParticipantCount; owner++)
            {
                if (_battle.IsInvulnerable(owner) || _battle.GetCount(owner) <= 0) continue;
                Vector2 delta = _battle.GetPosition(owner) - origin;
                float distanceSq = delta.sqrMagnitude;
                if (distanceSq <= 0.01f || distanceSq > maxRangeSq) continue;

                Vector2 direction = delta.normalized;
                float dot = Vector2.Dot(rawDirection, direction);
                if (dot < AimAssistDot) continue;

                float score = distanceSq * Mathf.Lerp(1.45f, 0.72f, Mathf.InverseLerp(AimAssistDot, 1f, dot));
                if (score >= bestScore) continue;

                bestScore = score;
                bestOwner = owner;
                bestDirection = direction;
            }

            return bestOwner != 0 ? bestDirection : rawDirection;
        }

        private void Fire(int ownerId, Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.001f || _battle.GetCount(ownerId) <= 0) return;
            direction.Normalize();

            ProjectileProxy projectile = AcquireProjectile();
            Vector3 origin = _battle.GetPosition(ownerId) + (Vector3)(direction * 0.56f);
            projectile.OwnerId = ownerId;
            projectile.Velocity = direction * BulletSpeed;
            projectile.Life = BulletLifetime;
            projectile.Active = true;
            projectile.Transform.position = origin;
            projectile.Transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
            projectile.Renderer.color = BattlePalette.Color(ownerId);
            projectile.Transform.gameObject.SetActive(true);
            ActiveProjectileCount++;

            _battle.NotifyShotFired(ownerId);
        }

        private ProjectileProxy AcquireProjectile()
        {
            for (int attempt = 0; attempt < _pool.Count; attempt++)
            {
                int index = (_poolCursor + attempt) % _pool.Count;
                ProjectileProxy projectile = _pool[index];
                if (projectile.Active) continue;
                _poolCursor = (index + 1) % _pool.Count;
                return projectile;
            }

            ProjectileProxy reused = _pool[_poolCursor];
            _poolCursor = (_poolCursor + 1) % _pool.Count;
            if (reused.Active)
            {
                reused.Active = false;
                reused.Transform.gameObject.SetActive(false);
                ActiveProjectileCount = Mathf.Max(0, ActiveProjectileCount - 1);
            }
            return reused;
        }

        private void SimulateProjectiles()
        {
            float hitRadiusSq = HitRadius * HitRadius;
            float dt = Time.deltaTime;

            for (int i = 0; i < _pool.Count; i++)
            {
                ProjectileProxy projectile = _pool[i];
                if (!projectile.Active) continue;

                projectile.Life -= dt;
                projectile.Transform.position += (Vector3)(projectile.Velocity * dt);
                Vector3 position = projectile.Transform.position;

                if (projectile.Life <= 0f ||
                    Mathf.Abs(position.x) > _halfExtents.x + 0.7f ||
                    Mathf.Abs(position.y) > _halfExtents.y + 0.7f)
                {
                    Deactivate(projectile);
                    continue;
                }

                bool consumed = false;
                for (int owner = 1; owner <= BattlePalette.ParticipantCount; owner++)
                {
                    if (owner == projectile.OwnerId || _battle.GetCount(owner) <= 0 || _battle.IsInvulnerable(owner)) continue;
                    Vector3 target = _battle.GetPosition(owner);
                    if ((target - position).sqrMagnitude > hitRadiusSq) continue;

                    _battle.ApplyProjectileHit(projectile.OwnerId, owner);
                    Deactivate(projectile);
                    consumed = true;
                    break;
                }

                if (consumed) continue;
            }
        }

        private void Deactivate(ProjectileProxy projectile)
        {
            if (projectile == null || !projectile.Active) return;
            projectile.Active = false;
            projectile.Transform.gameObject.SetActive(false);
            ActiveProjectileCount = Mathf.Max(0, ActiveProjectileCount - 1);
        }

        private void DeactivateAll()
        {
            for (int i = 0; i < _pool.Count; i++)
                Deactivate(_pool[i]);
            ActiveProjectileCount = 0;
        }
    }
}
