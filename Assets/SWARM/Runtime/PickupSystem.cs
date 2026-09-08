using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Shared food field for the full-map battle arena. Every army competes for the exact same visible pickups.
    /// </summary>
    public sealed class PickupSystem : MonoBehaviour
    {
        private const int PickupCount = 132;
        private const float PlayerCollectRadius = 0.64f;
        private const float BotCollectRadius = 0.58f;
        private const float PlayerMagnetRadius = 1.16f;

        private readonly Transform[] _pickups = new Transform[PickupCount];
        private readonly int[] _values = new int[PickupCount];
        private Transform _player;
        private SwarmController _swarm;
        private BattleArenaDirector _battle;
        private BlobPresenter _presenter;
        private CameraRig _cameraRig;
        private ToyHud _hud;
        private FeedbackDirector _feedback;
        private Vector2 _halfExtents;
        private System.Random _random;

        public void Initialize(
            Transform player,
            SwarmController swarm,
            BattleArenaDirector battle,
            BlobPresenter presenter,
            CameraRig cameraRig,
            ToyHud hud,
            FeedbackDirector feedback,
            Vector2 halfExtents)
        {
            _player = player;
            _swarm = swarm;
            _battle = battle;
            _presenter = presenter;
            _cameraRig = cameraRig;
            _hud = hud;
            _feedback = feedback;
            _halfExtents = halfExtents;
            _random = new System.Random(17031);
            BuildPool();
        }

        public bool TryFindBestPickup(Vector3 from, bool preferCenter, out Vector3 target)
        {
            target = Vector3.zero;
            float bestScore = float.MaxValue;
            bool found = false;

            for (int i = 0; i < _pickups.Length; i++)
            {
                Transform pickup = _pickups[i];
                if (pickup == null) continue;

                float distanceSq = (pickup.position - from).sqrMagnitude;
                float valueWeight = _values[i] == 5 ? 3.8f : _values[i] == 3 ? 2.3f : 1f;
                float score = distanceSq / valueWeight;
                if (preferCenter)
                {
                    float centerSq = pickup.position.sqrMagnitude;
                    score *= Mathf.Lerp(0.58f, 1f, Mathf.Clamp01(centerSq / 75f));
                }

                if (score >= bestScore) continue;
                bestScore = score;
                target = pickup.position;
                found = true;
            }

            return found;
        }

        private void BuildPool()
        {
            for (int i = 0; i < PickupCount; i++)
            {
                int value = i % 24 == 0 ? 5 : i % 7 == 0 ? 3 : 1;
                _values[i] = value;

                var go = new GameObject("Food_" + i.ToString("000"));
                go.transform.SetParent(transform, false);
                float scale = value == 5 ? 0.36f : value == 3 ? 0.27f : 0.19f + (i % 3) * 0.018f;
                go.transform.localScale = Vector3.one * scale;

                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeArt.Circle;
                RuntimeArt.Configure(renderer);
                renderer.color = value == 5
                    ? new Color(1f, 0.46f, 0.92f, 1f)
                    : value == 3
                        ? new Color(0.34f, 0.94f, 1f, 1f)
                        : new Color(1f, 0.92f, 0.42f, 0.95f);
                renderer.sortingOrder = value == 5 ? 8 : value == 3 ? 7 : 6;

                _pickups[i] = go.transform;
                Relocate(i, true);
            }
        }

        private void Update()
        {
            if (_player == null || _battle == null || !_battle.IsRunning) return;

            Vector3 playerPosition = _player.position;
            float playerCollectSq = PlayerCollectRadius * PlayerCollectRadius;
            float playerMagnetSq = PlayerMagnetRadius * PlayerMagnetRadius;
            float botCollectSq = BotCollectRadius * BotCollectRadius;

            for (int i = 0; i < _pickups.Length; i++)
            {
                Transform pickup = _pickups[i];
                Vector3 playerDelta = pickup.position - playerPosition;
                float playerDistanceSq = playerDelta.sqrMagnitude;

                if (playerDistanceSq < playerMagnetSq && playerDistanceSq > playerCollectSq)
                {
                    float magnetSpeed = 2.5f + Mathf.Min(_swarm.Count, 60) * 0.025f;
                    pickup.position = Vector3.MoveTowards(pickup.position, playerPosition, magnetSpeed * Time.deltaTime);
                    playerDistanceSq = (pickup.position - playerPosition).sqrMagnitude;
                }

                if (playerDistanceSq <= playerCollectSq)
                {
                    CollectForPlayer(i);
                    continue;
                }

                RivalBot collector = null;
                float nearestSq = botCollectSq;
                var bots = _battle.Bots;
                for (int b = 0; b < bots.Count; b++)
                {
                    RivalBot bot = bots[b];
                    if (bot == null || _battle.IsInvulnerable(bot.OwnerId)) continue;
                    float distanceSq = (pickup.position - bot.transform.position).sqrMagnitude;
                    if (distanceSq >= nearestSq) continue;
                    nearestSq = distanceSq;
                    collector = bot;
                }

                if (collector != null)
                {
                    int value = _values[i];
                    collector.AddUnits(value);
                    Relocate(i, false);
                }
            }
        }

        private void CollectForPlayer(int index)
        {
            int value = _values[index];
            Relocate(index, false);
            _swarm.AddUnits(value);
            if (_presenter != null) _presenter.Pulse(value == 5 ? 1.65f : value == 3 ? 1.35f : 0.85f);
            if (_cameraRig != null) _cameraRig.Punch(value == 5 ? 0.16f : value == 3 ? 0.10f : 0.05f);
            if (_hud != null) _hud.NotifyPickup(value);
            if (_feedback != null) _feedback.NotifyPickup(value > 1);
        }

        private void Relocate(int index, bool initial)
        {
            Transform pickup = _pickups[index];
            int value = _values[index];
            bool finalRush = _battle != null && _battle.IsFinalRush;
            double centerChance = value >= 3 ? 0.86 : finalRush ? 0.72 : initial ? 0.42 : 0.32;
            bool nearCenter = _random.NextDouble() < centerChance;

            float x;
            float y;
            if (nearCenter)
            {
                double angle = _random.NextDouble() * Mathf.PI * 2f;
                double maxRadius = value == 5 ? 3.3 : value == 3 ? 4.8 : 6.4;
                double radius = 0.7 + _random.NextDouble() * maxRadius;
                x = Mathf.Cos((float)angle) * (float)radius;
                y = Mathf.Sin((float)angle) * (float)radius;
            }
            else
            {
                x = Mathf.Lerp(-_halfExtents.x + 0.55f, _halfExtents.x - 0.55f, (float)_random.NextDouble());
                y = Mathf.Lerp(-_halfExtents.y + 0.55f, _halfExtents.y - 0.55f, (float)_random.NextDouble());
            }

            pickup.position = new Vector3(x, y, 0f);
        }
    }
}
