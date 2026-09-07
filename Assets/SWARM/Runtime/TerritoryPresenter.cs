using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Visual-only projection of TerritorySystem. The logical grid remains authoritative.
    /// </summary>
    public sealed class TerritoryPresenter : MonoBehaviour
    {
        private static readonly Color NeutralColor = new Color(0f, 0f, 0f, 0f);
        private static readonly Color OwnedColor = new Color(1f, 0.64f, 0.16f, 0.34f);
        private static readonly Color TrailColor = new Color(1f, 0.88f, 0.25f, 0.92f);

        private TerritorySystem _territory;
        private Texture2D _texture;
        private bool _dirty;

        public void Initialize(TerritorySystem territory, Vector2 halfExtents)
        {
            _territory = territory;
            _texture = new Texture2D(TerritorySystem.GridWidth, TerritorySystem.GridHeight, TextureFormat.RGBA32, false)
            {
                name = "SWARM_TerritoryGrid",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[TerritorySystem.GridWidth * TerritorySystem.GridHeight];
            for (int y = 0; y < TerritorySystem.GridHeight; y++)
            {
                for (int x = 0; x < TerritorySystem.GridWidth; x++)
                    pixels[y * TerritorySystem.GridWidth + x] = ColorFor(territory.GetCell(x, y));
            }
            _texture.SetPixels(pixels);
            _texture.Apply(false, false);

            var sprite = Sprite.Create(
                _texture,
                new Rect(0, 0, TerritorySystem.GridWidth, TerritorySystem.GridHeight),
                new Vector2(0.5f, 0.5f),
                1f);
            sprite.name = "SWARM_TerritorySprite";

            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            RuntimeArt.Configure(renderer);
            renderer.sortingOrder = -40;

            transform.position = new Vector3(0f, 0f, 0.1f);
            transform.localScale = new Vector3(
                halfExtents.x * 2f / TerritorySystem.GridWidth,
                halfExtents.y * 2f / TerritorySystem.GridHeight,
                1f);

            territory.CellStateChanged += OnCellStateChanged;
        }

        private void OnDestroy()
        {
            if (_territory != null)
                _territory.CellStateChanged -= OnCellStateChanged;
        }

        private void OnCellStateChanged(int x, int y, byte state)
        {
            if (_texture == null) return;
            _texture.SetPixel(x, y, ColorFor(state));
            _dirty = true;
        }

        private void LateUpdate()
        {
            if (!_dirty || _texture == null) return;
            _dirty = false;
            _texture.Apply(false, false);
        }

        private static Color ColorFor(byte state)
        {
            if (state == TerritorySystem.Owned) return OwnedColor;
            if (state == TerritorySystem.Trail) return TrailColor;
            return NeutralColor;
        }
    }
}
