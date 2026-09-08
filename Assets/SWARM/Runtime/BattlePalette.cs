using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Shared identities for the eight battle-arena participants.
    /// Owner 1 is always the human player; 2-8 are autonomous rivals.
    /// </summary>
    public static class BattlePalette
    {
        public const int ParticipantCount = 8;
        public const byte PlayerOwner = 1;

        public static string Name(int ownerId)
        {
            switch (ownerId)
            {
                case 1: return "VOS";
                case 2: return "ROJO";
                case 3: return "AZUL";
                case 4: return "VERDE";
                case 5: return "VIOLETA";
                case 6: return "CELESTE";
                case 7: return "ROSA";
                case 8: return "LIMA";
                default: return "NEUTRO";
            }
        }

        public static Color Color(int ownerId)
        {
            switch (ownerId)
            {
                case 1: return new Color(1.00f, 0.64f, 0.15f, 1f);
                case 2: return new Color(1.00f, 0.24f, 0.31f, 1f);
                case 3: return new Color(0.24f, 0.54f, 1.00f, 1f);
                case 4: return new Color(0.28f, 0.92f, 0.48f, 1f);
                case 5: return new Color(0.66f, 0.36f, 1.00f, 1f);
                case 6: return new Color(0.20f, 0.91f, 0.98f, 1f);
                case 7: return new Color(1.00f, 0.40f, 0.73f, 1f);
                case 8: return new Color(0.72f, 0.94f, 0.20f, 1f);
                default: return new Color(0.35f, 0.40f, 0.48f, 1f);
            }
        }

        public static Vector3 BasePosition(int ownerId, Vector2 halfExtents)
        {
            int index = Mathf.Clamp(ownerId, 1, ParticipantCount) - 1;
            float angle = (-90f + index * 45f) * Mathf.Deg2Rad;
            float radiusX = Mathf.Max(1f, halfExtents.x - 1.25f);
            float radiusY = Mathf.Max(1f, halfExtents.y - 1.75f);
            return new Vector3(Mathf.Cos(angle) * radiusX, Mathf.Sin(angle) * radiusY, 0f);
        }
    }
}
