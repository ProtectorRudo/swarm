using UnityEngine;

namespace Swarm
{
    public sealed class PickupSystem : MonoBehaviour
    {
        private const int PickupCount = 48;
        private const float CollectRadius = 0.70f;

        private readonly Transform[] _pickups = new Transform[PickupCount];
        private Transform _player;
        private SwarmController _swarm;
        private BlobPresenter _presenter;
        private CameraRig _cameraRig;
        private ToyHud _hud;
        private Vector2 _halfExtents;
        private System.Random _random;

        public void Initialize(
            Transform player,
            SwarmController swarm,
            BlobPresenter presenter,
            CameraRig cameraRig,
            ToyHud hud,
            Vector2 halfExtents)
        {
            _player = player;
            _swarm = swarm;
            _presenter = presenter;
            _cameraRig = cameraRig;
            _hud = hud;
            _halfExtents = halfExtents;
            _random = new System.Random(17031);
            BuildPool();
        }

        private void BuildPool()
        {
            for (int i = 0; i < PickupCount; i++)
            {
                var go = new GameObject("Pickup_" + i.ToString("00"));
                go.transform.SetParent(transform, false);
                go.transform.localScale = Vector3.one * (0.28f + (i % 3) * 0.025f);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeArt.Circle;
                renderer.color = i % 8 == 0
                    ? new Color(0.47f, 0.93f, 1f, 0.96f)
                    : new Color(1f, 0.92f, 0.48f, 0.94f);
                renderer.sortingOrder = 5;
                _pickups[i] = go.transform;
                Relocate(go.transform, i < 6);
            }
        }

        private void Update()
        {
            if (_player == null) return;

            Vector3 playerPosition = _player.position;
            float collectRadiusSq = CollectRadius * CollectRadius;
            for (int i = 0; i < _pickups.Length; i++)
            {
                Transform pickup = _pickups[i];
                Vector3 delta = pickup.position - playerPosition;
                if (delta.sqrMagnitude > collectRadiusSq) continue;

                Relocate(pickup, false);
                _swarm.AddUnits(1);
                _presenter.Pulse();
                _cameraRig.Punch(0.14f);
                _hud.NotifyPickup();
            }
        }

        private void Relocate(Transform pickup, bool nearCenter)
        {
            float x;
            float y;
            if (nearCenter)
            {
                double angle = _random.NextDouble() * Mathf.PI * 2f;
                double radius = 1.4 + _random.NextDouble() * 2.2;
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
