using System;
using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Authoritative ownership grid for the 0.3 core rework.
    /// Territory is painted directly by moving swarms: there are no Paper.io-style exposed trails or loop closure.
    /// </summary>
    public sealed class TerritorySystem : MonoBehaviour
    {
        public const byte Neutral = 0;
        public const byte PlayerOwned = 1;
        public const byte RivalOwned = 2;

        public const int GridWidth = 48;
        public const int GridHeight = 80;

        private readonly byte[] _cells = new byte[GridWidth * GridHeight];

        private Transform _player;
        private SwarmController _playerSwarm;
        private Vector2 _halfExtents;
        private int _lastPlayerCell = -1;
        private int _playerOwnedCells;
        private int _rivalOwnedCells;
        private int _pendingPlayerCells;
        private int _pendingEnemyCells;
        private float _feedbackCooldown;

        public float OwnedPercent => PlayerOwnedPercent;
        public float PlayerOwnedPercent => _playerOwnedCells / (float)_cells.Length;
        public float RivalOwnedPercent => _rivalOwnedCells / (float)_cells.Length;

        public event Action<int, int, byte> CellStateChanged;
        public event Action<float, int, int> PlayerExpanded;

        public void Initialize(Transform player, SwarmController playerSwarm, Vector2 halfExtents)
        {
            _player = player;
            _playerSwarm = playerSwarm;
            _halfExtents = halfExtents;
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

        /// <summary>
        /// Paints rival territory around the bot. Returns how many player-owned cells were converted,
        /// allowing RivalBot to charge a swarm cost for invading enemy ground.
        /// </summary>
        public int PaintRival(Vector3 worldPosition, int swarmCount)
        {
            int radius = BrushRadiusCells(swarmCount);
            PaintBrush(worldPosition, RivalOwned, radius, out _, out int enemyCells);
            return enemyCells;
        }

        private void Update()
        {
            _feedbackCooldown -= Time.deltaTime;

            if (_player != null && _playerSwarm != null)
            {
                WorldToCell(_player.position, out int x, out int y);
                int currentCell = y * GridWidth + x;
                if (currentCell != _lastPlayerCell)
                {
                    _lastPlayerCell = currentCell;
                    PaintPlayerAt(_player.position);
                }
            }

            if (_pendingPlayerCells > 0 && _feedbackCooldown <= 0f)
            {
                int cells = _pendingPlayerCells;
                int enemy = _pendingEnemyCells;
                _pendingPlayerCells = 0;
                _pendingEnemyCells = 0;
                _feedbackCooldown = 0.24f;
                PlayerExpanded?.Invoke(PlayerOwnedPercent, cells, enemy);
            }
        }

        private void PaintPlayerAt(Vector3 worldPosition)
        {
            int radius = BrushRadiusCells(_playerSwarm.Count);
            PaintBrush(worldPosition, PlayerOwned, radius, out int changedCells, out int enemyCells);
            if (changedCells <= 0) return;

            _pendingPlayerCells += changedCells;
            _pendingEnemyCells += enemyCells;

            // Invading red territory has a visible strategic cost, but never strips the player below a playable core.
            if (enemyCells > 0 && _playerSwarm.Count > 3)
            {
                int requestedCost = Mathf.Max(1, Mathf.CeilToInt(enemyCells / 8f));
                int affordableCost = Mathf.Min(requestedCost, _playerSwarm.Count - 3);
                _playerSwarm.RemoveUnits(affordableCost);
            }
        }

        private void PaintBrush(Vector3 worldPosition, byte owner, int radius, out int changedCells, out int enemyCells)
        {
            WorldToCell(worldPosition, out int centerX, out int centerY);
            changedCells = 0;
            enemyCells = 0;
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
                    if (old == owner) continue;

                    if (old != Neutral) enemyCells++;
                    changedCells++;
                    SetCell(index, owner);
                }
            }
        }

        private static int BrushRadiusCells(int swarmCount)
        {
            float radius = 1.25f + Mathf.Sqrt(Mathf.Max(1, swarmCount)) * 0.30f;
            return Mathf.Clamp(Mathf.RoundToInt(radius), 2, 6);
        }

        private void SeedStartingTerritories()
        {
            SeedDisc(GridWidth / 2, GridHeight / 2, 4, PlayerOwned);
            SeedDisc(Mathf.RoundToInt(GridWidth * 0.79f), Mathf.RoundToInt(GridHeight * 0.78f), 4, RivalOwned);
        }

        private void SeedDisc(int centerX, int centerY, int radius, byte owner)
        {
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

            if (old == PlayerOwned) _playerOwnedCells--;
            else if (old == RivalOwned) _rivalOwnedCells--;

            if (state == PlayerOwned) _playerOwnedCells++;
            else if (state == RivalOwned) _rivalOwnedCells++;

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
