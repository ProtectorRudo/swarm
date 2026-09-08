using UnityEngine;

namespace Swarm
{
    public sealed class ArenaPresenter : MonoBehaviour
    {
        public void Build(Vector2 halfExtents)
        {
            BuildFloor(halfExtents);
            BuildCenterHotZone();
            BuildBases(halfExtents);
            BuildBackgroundDots(halfExtents);
        }

        private void BuildFloor(Vector2 halfExtents)
        {
            var floor = new GameObject("Floor");
            floor.transform.SetParent(transform, false);
            floor.transform.localScale = new Vector3(halfExtents.x * 2f + 1.2f, halfExtents.y * 2f + 1.2f, 1f);
            var renderer = floor.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArt.Square;
            RuntimeArt.Configure(renderer);
            renderer.color = new Color(0.072f, 0.090f, 0.135f, 1f);
            renderer.sortingOrder = -100;
        }

        private void BuildCenterHotZone()
        {
            var halo = new GameObject("CenterHotZone");
            halo.transform.SetParent(transform, false);
            halo.transform.position = Vector3.zero;
            halo.transform.localScale = Vector3.one * 6.2f;
            var renderer = halo.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArt.Circle;
            RuntimeArt.Configure(renderer);
            renderer.color = new Color(0.58f, 0.66f, 0.82f, 0.085f);
            renderer.sortingOrder = -72;

            var core = new GameObject("CenterCore");
            core.transform.SetParent(transform, false);
            core.transform.position = Vector3.zero;
            core.transform.localScale = Vector3.one * 2.1f;
            var coreRenderer = core.AddComponent<SpriteRenderer>();
            coreRenderer.sprite = RuntimeArt.Circle;
            RuntimeArt.Configure(coreRenderer);
            coreRenderer.color = new Color(1f, 0.45f, 0.88f, 0.085f);
            coreRenderer.sortingOrder = -71;
        }

        private void BuildBases(Vector2 halfExtents)
        {
            for (int owner = 1; owner <= BattlePalette.ParticipantCount; owner++)
            {
                Color color = BattlePalette.Color(owner);
                Vector3 position = BattlePalette.BasePosition(owner, halfExtents);

                var outer = new GameObject("Base_" + BattlePalette.Name(owner));
                outer.transform.SetParent(transform, false);
                outer.transform.position = position;
                outer.transform.localScale = Vector3.one * 3.6f;
                var outerRenderer = outer.AddComponent<SpriteRenderer>();
                outerRenderer.sprite = RuntimeArt.Circle;
                RuntimeArt.Configure(outerRenderer);
                Color outerColor = color;
                outerColor.a = 0.12f;
                outerRenderer.color = outerColor;
                outerRenderer.sortingOrder = -65;

                var core = new GameObject("BaseCore_" + BattlePalette.Name(owner));
                core.transform.SetParent(transform, false);
                core.transform.position = position;
                core.transform.localScale = Vector3.one * 1.45f;
                var coreRenderer = core.AddComponent<SpriteRenderer>();
                coreRenderer.sprite = RuntimeArt.Circle;
                RuntimeArt.Configure(coreRenderer);
                Color coreColor = color;
                coreColor.a = owner == BattlePalette.PlayerOwner ? 0.38f : 0.29f;
                coreRenderer.color = coreColor;
                coreRenderer.sortingOrder = -43;
            }
        }

        private void BuildBackgroundDots(Vector2 halfExtents)
        {
            var random = new System.Random(4401);
            for (int i = 0; i < 96; i++)
            {
                var dot = new GameObject("ArenaDot_" + i.ToString("00"));
                dot.transform.SetParent(transform, false);
                dot.transform.position = new Vector3(
                    Mathf.Lerp(-halfExtents.x, halfExtents.x, (float)random.NextDouble()),
                    Mathf.Lerp(-halfExtents.y, halfExtents.y, (float)random.NextDouble()),
                    0f);
                dot.transform.localScale = Vector3.one * Mathf.Lerp(0.035f, 0.085f, (float)random.NextDouble());
                var renderer = dot.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeArt.Circle;
                RuntimeArt.Configure(renderer);
                renderer.color = new Color(0.40f, 0.48f, 0.62f, 0.22f);
                renderer.sortingOrder = -90;
            }
        }
    }
}
