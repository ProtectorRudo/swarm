using UnityEngine;

namespace Swarm
{
    public sealed class PickupSystem : MonoBehaviour
    {
        private const int PickupCount = 72;
        private const float CollectRadius = 0.72f;
        private const float MagnetRadius = 1.75f;
        private const float RivalCollectRadius = 0.66f;

        private readonly Transform[] _pickups = new Transform[PickupCount];
        private readonly bool[] _bonus = new bool[PickupCount];
        private Transform _player;
        private SwarmController _swarm;
        private RivalBot _rival;
        private BlobPresenter _presenter;
        private CameraRig _cameraRig;
        private ToyHud _hud;
        private FeedbackDirector _feedback;
        private Vector2 _halfExtents;
        private System.Random _random;

        public void Initialize(
            Transform player,
            SwarmController swarm,
            RivalBot rival,
            BlobPresenter presenter,
            CameraRig cameraRig,
            ToyHud hud,
            FeedbackDirector feedback,
            Vector2 halfExtents)
        {
            _player = player;
            _swarm = swarm;
            _rival = rival;
            _presenter = presenter;
            _cameraRig = cameraRig;
            _hud = hud;
            _feedback = feedback;
            _halfExtents = halfExtents;
            _random = new System.Random(17031);
            BuildPool();
        }

        private void BuildPool()
        {
            for (int i = 0; i < PickupCount; i++)
            {
                bool bonus = i % 10 == 0;
                _bonus[i] = bonus;

                var go = new GameObject("Pickup_" + i.ToString("00"));
                go.transform.SetParent(transform, false);
                go.transform.localScale = Vector3.one * (bonus ? 0.43f : 0.29f + (i % 3) * 0.025f);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeArt.Circle;
                RuntimeArt.Configure(renderer);
                renderer.color = bonus
                    ? new Color(0.40f, 0.95f, 1f, 1f)
                    : new Color(1f, 0.92f, 0.48f, 0.94f);
                renderer.sortingOrder = 5;
                _pickups[i] = go.transform;
                Relocate(go.transform, i < 12);
            }
        }

        private void Update()
        {
            if (_player == null) return;

            Vector3 playerPosition = _player.position;
            Vector3 rivalPosition = _rival != null ? _rival.transform.position : new Vector3(999f, 999f, 0f);
            float collectRadiusSq = CollectRadius * CollectRadius;
            float magnetRadiusSq = MagnetRadius * MagnetRadius;
            float rivalCollectRadiusSq = RivalCollectRadius * RivalCollectRadius;

            for (int i = 0; i < _pickups.Length; i++)
            {
                Transform pickup = _pickups[i];
                Vector3 playerDelta = pickup.position - playerPosition;
                float playerDistanceSq = playerDelta.sqrMagnitude;
                float rivalDistanceSq = (pickup.position - rivalPosition).sqrMagnitude;

                // The player keeps the satisfying one-hand magnet. The rival has to physically reach food.
                if (playerDistanceSq < magnetRadiusSq && playerDistanceSq > collectRadiusSq &&
                    playerDistanceSq <= rivalDistanceSq)
                {
                    float magnetSpeed = 2.8f + Mathf.Min(_swarm.Count, 50) * 0.035f;
                    pickup.position = Vector3.MoveTowards(pickup.position, playerPosition, magnetSpeed * Time.deltaTime);
                    playerDelta = pickup.position - playerPosition;
                    playerDistanceSq = playerDelta.sqrMagnitude;
                    rivalDistanceSq = (pickup.position - rivalPosition).sqrMagnitude;
                }

                if (playerDistanceSq <= collectRadiusSq)
                {
                    CollectForPlayer(i, pickup);
                    continue;
                }

                if (_rival != null && rivalDistanceSq <= rivalCollectRadiusSq)
                {
                    bool bonus = _bonus[i];
                    int value = bonus ? 3 : 1;
                    Relocate(pickup, false);
                    _rival.AddUnits(value);
                }
            }
        }

        private void CollectForPlayer(int index, Transform pickup)
        {
            bool bonus = _bonus[index];
            int value = bonus ? 3 : 1;
            Relocate(pickup, false);
            _swarm.AddUnits(value);
            _presenter.Pulse(bonus ? 1.45f : 1f);
            _cameraRig.Punch(bonus ? 0.24f : 0.14f);
            _hud.NotifyPickup();
            if (_feedback != null) _feedback.NotifyPickup(bonus);
        }

        private void Relocate(Transform pickup, bool nearCenter)
        {
            float x;
            float y;
            if (nearCenter)
            {
                double angle = _random.NextDouble() * Mathf.PI * 2f;
                double radius = 1.1 + _random.NextDouble() * 3.2;
                x = (float)(Mathf.Cos((float)angle) * radius);
                y = (float)(Mathf.Sin((float)angle) * radius);
            }
            else
            {
                x = Mathf.Lerp(-_halfExtents.x + 0.7f, _halfExtents.x - 0.7f, (float)_random.NextDouble());
                y = Mathf.Lerp(-_halfExtents.y + 0.7f, _halfExtents.y - 0.7f, (float)_random.NextDouble());
            }
            pickup.position = new Vector3(x, y, 0f);
        }
    }
}
