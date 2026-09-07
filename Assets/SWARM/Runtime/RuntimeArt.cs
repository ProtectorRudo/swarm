using UnityEngine;

namespace Swarm
{
    public static class RuntimeArt
    {
        private static Sprite _circle;
        private static Sprite _square;

        public static Sprite Circle
        {
            get
            {
                if (_circle == null)
                {
                    _circle = CreateCircleSprite(64);
                }
                return _circle;
            }
        }

        public static Sprite Square
        {
            get
            {
                if (_square == null)
                {
                    var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                    {
                        name = "SWARM_RuntimeSquare",
                        filterMode = FilterMode.Bilinear,
                        wrapMode = TextureWrapMode.Clamp
                    };
                    texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
                    texture.Apply(false, true);
                    _square = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
                    _square.name = "SWARM_RuntimeSquareSprite";
                }
                return _square;
            }
        }

        private static Sprite CreateCircleSprite(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "SWARM_RuntimeCircle",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float radius = size * 0.46f;
            float feather = 1.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01((radius - distance + feather) / feather);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = "SWARM_RuntimeCircleSprite";
            return sprite;
        }
    }
}
