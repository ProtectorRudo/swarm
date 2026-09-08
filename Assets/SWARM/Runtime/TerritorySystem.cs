using System;
using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Authoritative eight-owner influence grid for Battle Arena.
    /// Territory is secondary to army combat: every participant paints simply by moving.
    /// </summary>
    public sealed class TerritorySystem : MonoBehaviour
    {
        public const byte Neutral = 0;
        public const byte PlayerOwned = 1;
        public const byte RivalOwned = 2;

        public const int GridWidth = 48;
        public const int GridHeight = 80;

        private readonly byte[] _cells = new byte[GridWidth * GridHeight];
        private readonly int[] _ownedCells = new int[BattlePalette.ParticipantCount + 1];
        private Vector2 _halfExtents;

        public float OwnedPercent => PlayerOwnedPercent;
        public float PlayerOwnedPercent => GetOwnedPercent(BattlePalette.PlayerOwner);
        public float RivalOwnedPercent
        {
            get
            {
                int rivalCells = 0;
                for (int owner = 2; owner <= BattlePalette.ParticipantCount; owner++)
                    rivalCells += _ownedCells[owner];
                return rivalCells / (float)_cells.Length;
            }
        }

        public event Action<int, int, byte> CellStateChanged;
        public event Action<float, int, int> PlayerExpanded;

        public void Initialize(Vector2 halfExtents)
        {
            _halfExtents = halfExtents;
            Array.Clear(_cells, 0, _cells.Length);
            Array.Clear(_ownedCells, 0, _ownedCells.Length);
            SeedStartingTerritories();
        }

        public byte GetCell(int x, int y)
        {
            if (x < 0 || x >= GridWidth || y < 0 || y >= GridHeight) return Neutral;
            return _cells[y * GridWidth + x];
        }

        public byte GetOwnerAtWorldPosition(Vector3 worldPosition)
        {
            WorldToCell(worldPosition, out int x, out int y);
            return GetCell(x, y);
        }

        public float GetOwnedPercent(int ownerId)
        {
            if (ownerId < 1 || ownerId > BattlePalette.ParticipantCount) return 0f;
            return _ownedCells[ownerId] / (float)_cells.Length;
        }

        public int GetOwnedCellCount(int ownerId)
        {
            if (ownerId < 1 || ownerId > BattlePalette.ParticipantCount) return 0;
            return _ownedCells[ownerId];
        }

        /// <summary>
        /// Paints influence for one participant. Returns changed cells and reports how many belonged to another player.
        /// </summary>
        public int Paint(int ownerId, Vector3 worldPosition, int swarmCount, out int enemyCells)
        {
            enemyCells = 0;
            if (ownerId < 1 || ownerId > BattlePalette.ParticipantCount) return 0;

            int radius = BrushRadiusCells(swarmCount);
            WorldToCell(worldPosition, out int centerX, out int centerY);
            int changedCells = 0;
            int radiusSq = radius * radius;

            int minX = Mathf.Max(0, centerX - radius);
            int maxX = Mathf.Min(GridWidth - 1, centerX + radius);
            int minY = Mathf.Max(0, centerY - radius);
            int maxY = Mathf.Min(GridHeight - 1, centerY + radius);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int dx = x - centerX;
                    int dy = y - centerY;
                    if (dx * dx + dy * dy > radiusSq) continue;

                    int index = y * GridWidth + x;
                    byte old = _cells[index];
                    if (old == ownerId) continue;

                    if (old != Neutral) enemyCells++;
                    changedCells++;
                    SetCell(index, (byte)ownerId);
                }
            }

            if (ownerId == BattlePalette.PlayerOwner && changedCells > 0)
                PlayerExpanded?.Invoke(PlayerOwnedPercent, changedCells, enemyCells);

            return changedCells;
        }

        public bool IsDefending(int ownerId, Vector3 worldPosition)
        {
            if (ownerId < 1 || ownerId > BattlePalette.ParticipantCount) return false;
            Vector3 home = BattlePalette.BasePosition(ownerId, _halfExtents);
            if ((worldPosition - home).sqrMagnitude <= 2.15f * 2.15f) return true;
            return GetOwnerAtWorldPosition(worldPosition) == ownerId;
        }

        private static int BrushRadiusCells(int swarmCount)
        {
            float radius = 1.15f + Mathf.Sqrt(Mathf.Max(1, swarmCount)) * 0.26f;
            return Mathf.Clamp(Mathf.RoundToInt(radius), 2, 5);
        }

        private void SeedStartingTerritories()
        {
            for (int owner = 1; owner <= BattlePalette.ParticipantCount; owner++)
                SeedDiscWorld(BattlePalette.BasePosition(owner, _halfExtents), 4, (byte)owner);
        }

        private void SeedDiscWorld(Vector3 worldPosition, int radius, byte owner)
        {
            WorldToCell(worldPosition, out int centerX, out int centerY);
            int radiusSq = radius * radius;
            for (int y = Mathf.Max(0, centerY - radius); y <= Mathf.Min(GridHeight - 1, centerY + radius); y++)
            {
                for (int x = Mathf.Max(0, centerX - radius); x <= Mathf.Min(GridWidth - 1, centerX + radius); x++)
                {
                    int dx = x - centerX;
                    int dy = y - centerY;
                    if (dx * dx + dy * dy <= radiusSq)
                        SetCell(y * GridWidth + x, owner);
                }
            }
        }

        private void SetCell(int index, byte state)
        {
            byte old = _cells[index];
            if (old == state) return;

            if (old >= 1 && old <= BattlePalette.ParticipantCount) _ownedCells[old]--;
            if (state >= 1 && state <= BattlePalette.ParticipantCount) _ownedCells[state]++;

            _cells[index] = state;
            CellStateChanged?.Invoke(index % GridWidth, index / GridWidth, state);
        }

        private void WorldToCell(Vector3 world, out int x, out int y)
        {
            float nx = Mathf.InverseLerp(-_halfExtents.x, _halfExtents.x, world.x);
            float ny = Mathf.InverseLerp(-_halfExtents.y, _halfExtents.y, world.y);
            x = Mathf.Clamp(Mathf.FloorToInt(nx * GridWidth), 0, GridWidth - 1);
            y = Mathf.Clamp(Mathf.FloorToInt(ny * GridHeight), 0, GridHeight - 1);
        }
    }
}
