using System;
using System.Collections.Generic;
using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Logical territory grid. Presentation subscribes to state changes; gameplay does not depend on rendering.
    /// </summary>
    public sealed class TerritorySystem : MonoBehaviour
    {
        public const byte Neutral = 0;
        public const byte Owned = 1;
        public const byte Trail = 2;

        public const int GridWidth = 48;
        public const int GridHeight = 80;

        private readonly byte[] _cells = new byte[GridWidth * GridHeight];
        private readonly List<int> _trail = new List<int>(256);
        private readonly Queue<int> _floodQueue = new Queue<int>(GridWidth * 2 + GridHeight * 2);
        private readonly bool[] _outsideReachable = new bool[GridWidth * GridHeight];

        private Transform _player;
        private Vector2 _halfExtents;
        private int _lastCell = -1;
        private int _ownedCells;
        private bool _drawingTrail;

        public float OwnedPercent => _ownedCells / (float)_cells.Length;
        public int CaptureCount { get; private set; }
        public bool IsTrailExposed => _drawingTrail && _trail.Count > 0;

        public event Action<int, int, byte> CellStateChanged;
        public event Action<float, int> CaptureCompleted;
        public event Action<bool> TrailExposureChanged;
        public event Action TrailCut;

        public void Initialize(Transform player, Vector2 halfExtents)
        {
            _player = player;
            _halfExtents = halfExtents;
            SeedHome();
        }

        public byte GetCell(int x, int y)
        {
            if (x < 0 || x >= GridWidth || y < 0 || y >= GridHeight) return Neutral;
            return _cells[y * GridWidth + x];
        }

        public bool TryGetTrailTarget(out Vector3 worldPosition)
        {
            if (_trail.Count == 0)
            {
                worldPosition = Vector3.zero;
                return false;
            }

            int sample = _trail[Mathf.Clamp(_trail.Count / 3, 0, _trail.Count - 1)];
            worldPosition = CellToWorld(sample);
            return true;
        }

        public bool TryCutAtWorldPosition(Vector3 worldPosition)
        {
            if (!_drawingTrail || _trail.Count == 0) return false;
            WorldToCell(worldPosition, out int x, out int y);
            int index = y * GridWidth + x;
            if (_cells[index] != Trail) return false;

            for (int i = 0; i < _trail.Count; i++)
                SetCell(_trail[i], Neutral);

            _trail.Clear();
            _drawingTrail = false;
            TrailExposureChanged?.Invoke(false);
            TrailCut?.Invoke();
            return true;
        }

        private void SeedHome()
        {
            int cx = GridWidth / 2;
            int cy = GridHeight / 2;
            const float radius = 5.2f;

            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    float dx = x - cx + 0.5f;
                    float dy = y - cy + 0.5f;
                    if (dx * dx + dy * dy > radius * radius) continue;
                    SetCell(x, y, Owned);
                }
            }
        }

        private void Update()
        {
            if (_player == null) return;

            WorldToCell(_player.position, out int x, out int y);
            int index = y * GridWidth + x;
            if (index == _lastCell) return;
            _lastCell = index;

            byte state = _cells[index];
            if (state == Owned)
            {
                if (_drawingTrail && _trail.Count > 0)
                    CloseAndCapture();
                return;
            }

            if (state == Neutral)
            {
                if (!_drawingTrail)
                {
                    _drawingTrail = true;
                    TrailExposureChanged?.Invoke(true);
                }

                _trail.Add(index);
                SetCell(index, Trail);
            }
        }

        private void CloseAndCapture()
        {
            int trailCells = _trail.Count;
            for (int i = 0; i < _trail.Count; i++)
                SetCell(_trail[i], Owned);

            Array.Clear(_outsideReachable, 0, _outsideReachable.Length);
            _floodQueue.Clear();

            for (int x = 0; x < GridWidth; x++)
            {
                TrySeedOutside(x, 0);
                TrySeedOutside(x, GridHeight - 1);
            }
            for (int y = 1; y < GridHeight - 1; y++)
            {
                TrySeedOutside(0, y);
                TrySeedOutside(GridWidth - 1, y);
            }

            while (_floodQueue.Count > 0)
            {
                int current = _floodQueue.Dequeue();
                int cx = current % GridWidth;
                int cy = current / GridWidth;
                TryVisitOutside(cx - 1, cy);
                TryVisitOutside(cx + 1, cy);
                TryVisitOutside(cx, cy - 1);
                TryVisitOutside(cx, cy + 1);
            }

            int enclosed = 0;
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] != Neutral || _outsideReachable[i]) continue;
                enclosed++;
                SetCell(i, Owned);
            }

            _trail.Clear();
            _drawingTrail = false;
            CaptureCount++;
            TrailExposureChanged?.Invoke(false);
            CaptureCompleted?.Invoke(OwnedPercent, trailCells + enclosed);
        }

        private void TrySeedOutside(int x, int y)
        {
            int index = y * GridWidth + x;
            if (_cells[index] != Neutral || _outsideReachable[index]) return;
            _outsideReachable[index] = true;
            _floodQueue.Enqueue(index);
        }

        private void TryVisitOutside(int x, int y)
        {
            if (x < 0 || x >= GridWidth || y < 0 || y >= GridHeight) return;
            int index = y * GridWidth + x;
            if (_cells[index] != Neutral || _outsideReachable[index]) return;
            _outsideReachable[index] = true;
            _floodQueue.Enqueue(index);
        }

        private void SetCell(int x, int y, byte state)
        {
            SetCell(y * GridWidth + x, state);
        }

        private void SetCell(int index, byte state)
        {
            byte old = _cells[index];
            if (old == state) return;
            if (old == Owned) _ownedCells--;
            if (state == Owned) _ownedCells++;
            _cells[index] = state;
            CellStateChanged?.Invoke(index % GridWidth, index / GridWidth, state);
        }

        private Vector3 CellToWorld(int index)
        {
            int x = index % GridWidth;
            int y = index / GridWidth;
            float nx = (x + 0.5f) / GridWidth;
            float ny = (y + 0.5f) / GridHeight;
            return new Vector3(
                Mathf.Lerp(-_halfExtents.x, _halfExtents.x, nx),
                Mathf.Lerp(-_halfExtents.y, _halfExtents.y, ny),
                0f);
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
