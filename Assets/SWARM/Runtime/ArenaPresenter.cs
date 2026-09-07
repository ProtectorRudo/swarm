using UnityEngine;

namespace Swarm
{
    public sealed class ArenaPresenter : MonoBehaviour
    {
        public void Build(Vector2 halfExtents)
        {
            var floor = new GameObject("Floor");
            floor.transform.SetParent(transform, false);
            floor.transform.localScale = new Vector3(halfExtents.x * 2f + 1.5f, halfExtents.y * 2f + 1.5f, 1f);
            var floorRenderer = floor.AddComponent<SpriteRenderer>();
            floorRenderer.sprite = RuntimeArt.Square;
            floorRenderer.color = new Color(0.095f, 0.12f, 0.18f, 1f);
            floorRenderer.sortingOrder = -100;

            var random = new System.Random(4401);
            for (int i = 0; i < 70; i++)
            {
                var dot = new GameObject("ArenaDot_" + i.ToString("00"));
                dot.transform.SetParent(transform, false);
                dot.transform.position = new Vector3(
                    Mathf.Lerp(-halfExtents.x, halfExtents.x, (float)random.NextDouble()),
                    Mathf.Lerp(-halfExtents.y, halfExtents.y, (float)random.NextDouble()),
                    0f);
                dot.transform.localScale = Vector3.one * Mathf.Lerp(0.05f, 0.12f, (float)random.NextDouble());
                var renderer = dot.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeArt.Circle;
                renderer.color = new Color(0.30f, 0.38f, 0.50f, 0.28f);
                renderer.sortingOrder = -90;
            }
        }
    }
}
