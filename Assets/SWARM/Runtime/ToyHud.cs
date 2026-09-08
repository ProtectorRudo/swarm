using UnityEngine;

namespace Swarm
{
    public sealed class ToyHud : MonoBehaviour
    {
        private OneHandInputSource _input;
        private SwarmController _swarm;
        private TerritorySystem _territory;
        private MatchDirector _match;
        private FirstTestTelemetry _telemetry;
        private RivalBot _rival;

        private GUIStyle _titleStyle;
        private GUIStyle _instructionStyle;
        private GUIStyle _debugStyle;
        private GUIStyle _resultStyle;
        private GUIStyle _resultSubStyle;
        private GUIStyle _rivalStyle;

        private float _pickupPulse;
        private float _expansionPulse;
        private float _combatPulse;
        private float _smoothedDelta;
        private string _eventMessage = string.Empty;
        private Color _eventColor = Color.white;
        private bool _showResult;
        private float _resultPercent;
        private int _resultSwarm;

        public void Initialize(OneHandInputSource input, SwarmController swarm)
        {
            _input = input;
            _swarm = swarm;
        }

        public void BindMatch(MatchDirector match, TerritorySystem territory)
        {
            _match = match;
            _territory = territory;
        }

        public void BindRival(RivalBot rival)
        {
            if (_rival != null)
            {
                _rival.CombatResolved -= OnCombatResolved;
                _rival.Defeated -= OnDefeated;
            }

            _rival = rival;
            if (_rival != null)
            {
                _rival.CombatResolved += OnCombatResolved;
                _rival.Defeated += OnDefeated;
            }
        }

        public void BindTelemetry(FirstTestTelemetry telemetry)
        {
            _telemetry = telemetry;
        }

        public void NotifyPickup()
        {
            _pickupPulse = 1f;
        }

        public void NotifyExpansion(float percent, int cells, int enemyCells)
        {
            _expansionPulse = 1f;
            if (enemyCells > 0)
            {
                _eventMessage = "¡ROBASTE " + enemyCells + " CELDAS ROJAS!";
                _eventColor = new Color(0.48f, 1f, 0.68f, 1f);
            }
            else if (cells >= 5)
            {
                _eventMessage = "+" + cells + " MAPA  •  " + (percent * 100f).ToString("0.0") + "%";
                _eventColor = new Color(0.48f, 1f, 0.68f, 1f);
            }
        }

        public void ShowResult(float percent, int swarmCount)
        {
            _showResult = true;
            _resultPercent = percent;
            _resultSwarm = swarmCount;
        }

        private void OnCombatResolved(bool playerAdvantage, int playerCount, int rivalCount)
        {
            _combatPulse = 1f;
            if (playerAdvantage)
            {
                _eventMessage = "¡LO ESTÁS COMIENDO!  " + playerCount + " vs " + rivalCount;
                _eventColor = new Color(0.48f, 1f, 0.68f, 1f);
            }
            else
            {
                _eventMessage = "¡ES MÁS FUERTE!  " + playerCount + " vs " + rivalCount;
                _eventColor = new Color(1f, 0.30f, 0.34f, 1f);
            }
        }

        private void OnDefeated(bool playerDefeated)
        {
            _combatPulse = 1.5f;
            if (playerDefeated)
            {
                _eventMessage = "TE COMIÓ • VOLVÉS CON 5";
                _eventColor = new Color(1f, 0.30f, 0.34f, 1f);
            }
            else
            {
                _eventMessage = "¡TE COMISTE AL ROJO!";
                _eventColor = new Color(0.48f, 1f, 0.68f, 1f);
            }
        }

        private void OnDestroy()
        {
            if (_rival != null)
            {
                _rival.CombatResolved -= OnCombatResolved;
                _rival.Defeated -= OnDefeated;
            }
        }

        private void Update()
        {
            _pickupPulse = Mathf.MoveTowards(_pickupPulse, 0f, Time.deltaTime * 3.6f);
            _expansionPulse = Mathf.MoveTowards(_expansionPulse, 0f, Time.deltaTime * 1.8f);
            _combatPulse = Mathf.MoveTowards(_combatPulse, 0f, Time.deltaTime * 1.3f);
            _smoothedDelta += (Time.unscaledDeltaTime - _smoothedDelta) * 0.08f;
        }

        private void OnGUI()
        {
            EnsureStyles();
            float scale = Mathf.Clamp(Screen.width / 1080f, 0.65f, 1.4f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = Screen.width / scale;
            float height = Screen.height / scale;

            int count = _swarm != null ? _swarm.Count : 0;
            float ownedPercent = _territory != null ? _territory.PlayerOwnedPercent : 0f;
            float titleScale = 1f + _pickupPulse * 0.10f;
            _titleStyle.fontSize = Mathf.RoundToInt(46f * titleScale);

            GUI.Label(new Rect(28f, 44f, width * 0.47f, 72f), "SWARM  " + count, _titleStyle);
            GUI.Label(new Rect(width * 0.50f, 44f, width * 0.47f, 72f), "TU MAPA  " + (ownedPercent * 100f).ToString("0.0") + "%", _titleStyle);

            if (_match != null)
            {
                int seconds = Mathf.CeilToInt(_match.TimeRemaining);
                GUI.Label(new Rect(width * 0.5f - 100f, 115f, 200f, 48f), seconds.ToString("00") + "s", _debugStyle);
            }

            DrawRivalLabel(scale, width, height);

            float eventPulse = Mathf.Max(_combatPulse, _expansionPulse);
            if (eventPulse > 0.01f && !string.IsNullOrEmpty(_eventMessage))
            {
                Color old = _instructionStyle.normal.textColor;
                _instructionStyle.normal.textColor = new Color(_eventColor.r, _eventColor.g, _eventColor.b, Mathf.Clamp01(eventPulse * 1.4f));
                _instructionStyle.fontSize = 38 + Mathf.RoundToInt(eventPulse * 9f);
                GUI.Label(new Rect(35f, height * 0.23f, width - 70f, 105f), _eventMessage, _instructionStyle);
                _instructionStyle.normal.textColor = old;
                _instructionStyle.fontSize = 34;
            }

            string instruction = GetInstruction(count);
            GUI.Label(new Rect(45f, height - 220f, width - 90f, 110f), instruction, _instructionStyle);

            float fps = _smoothedDelta > 0.0001f ? 1f / _smoothedDelta : 0f;
            GUI.Label(new Rect(24f, 16f, 330f, 36f), "CORE REWORK 0.3   " + fps.ToString("0") + " FPS", _debugStyle);

            if (_showResult)
                DrawResult(width, height);
        }

        private void DrawRivalLabel(float scale, float width, float height)
        {
            if (_rival == null || Camera.main == null || _showResult) return;
            Vector3 screen = Camera.main.WorldToScreenPoint(_rival.transform.position);
            if (screen.z <= 0f) return;

            float x = screen.x / scale;
            float y = height - screen.y / scale;
            if (x < -80f || x > width + 80f || y < -80f || y > height + 80f) return;

            GUI.Label(new Rect(x - 100f, y - 92f, 200f, 50f), "ROJO  " + _rival.Count, _rivalStyle);
        }

        private string GetInstruction(int count)
        {
            if (_showResult) return string.Empty;
            if (_input == null || !_input.HasEverMoved)
                return "ARRASTRÁ CON UN DEDO PARA MOVERTE";
            if (count < 8)
                return "JUNTÁ BICHITOS • HACÉ CRECER TU EJÉRCITO";

            if (_rival != null)
            {
                if (count >= _rival.Count + 3)
                    return "SOS MÁS GRANDE • ACERCATE AL ROJO Y COMÉTELO";
                if (_rival.Count >= count + 3)
                {
                    bool defending = _territory != null &&
                        _territory.GetOwnerAtWorldPosition(_swarm != null ? _swarm.AnchorPosition : Vector3.zero) == TerritorySystem.PlayerOwned;
                    return defending
                        ? "EL ROJO ES MÁS GRANDE • EN TU COLOR TENÉS DEFENSA"
                        : "EL ROJO ES MÁS GRANDE • CRECÉ O VOLVÉ A TU COLOR";
                }
            }

            return "MOVETE PARA PINTAR • MÁS SWARM = MÁS MAPA";
        }

        private void DrawResult(float width, float height)
        {
            GUI.Box(new Rect(45f, height * 0.22f, width - 90f, 650f), GUIContent.none);
            float rivalPercent = _territory != null ? _territory.RivalOwnedPercent : 0f;
            string verdict = _resultPercent > rivalPercent ? "GANASTE EL MAPA" : "EL ROJO GANÓ EL MAPA";
            GUI.Label(new Rect(70f, height * 0.25f, width - 140f, 90f), verdict, _resultStyle);
            GUI.Label(
                new Rect(70f, height * 0.33f, width - 140f, 150f),
                "VOS  " + (_resultPercent * 100f).ToString("0.0") + "%   •   ROJO  " + (rivalPercent * 100f).ToString("0.0") + "%\nSWARM FINAL  " + _resultSwarm,
                _resultSubStyle);

            if (_telemetry != null)
            {
                string firstGrowth = _telemetry.TimeToFirstGrowth >= 0f
                    ? _telemetry.TimeToFirstGrowth.ToString("0.0") + "s"
                    : "NO";
                string metrics =
                    "PRIMER CRECIMIENTO  " + firstGrowth +
                    "\nEXPANSIONES  " + _telemetry.ExpansionBursts +
                    "   •   ROJAS ROBADAS  " + _telemetry.EnemyCellsTaken +
                    "\nPELEAS +  " + _telemetry.FightsWon +
                    "   •   PELEAS -  " + _telemetry.FightsLost +
                    "   •   MÁX SWARM  " + _telemetry.MaxSwarm;
                GUI.Label(new Rect(75f, height * 0.46f, width - 150f, 160f), metrics, _debugStyle);
            }

            GUI.Label(new Rect(70f, height * 0.62f, width - 140f, 100f), "TOCÁ PARA JUGAR OTRA", _instructionStyle);
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null) return;

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 46,
                fontStyle = FontStyle.Bold
            };
            _titleStyle.normal.textColor = Color.white;

            _instructionStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 34,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            _instructionStyle.normal.textColor = new Color(1f, 0.90f, 0.46f, 1f);

            _debugStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                wordWrap = true
            };
            _debugStyle.normal.textColor = new Color(1f, 1f, 1f, 0.58f);

            _resultStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 54,
                fontStyle = FontStyle.Bold
            };
            _resultStyle.normal.textColor = Color.white;

            _resultSubStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 36,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            _resultSubStyle.normal.textColor = new Color(0.56f, 0.95f, 1f, 1f);

            _rivalStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 26,
                fontStyle = FontStyle.Bold
            };
            _rivalStyle.normal.textColor = new Color(1f, 0.36f, 0.40f, 1f);
        }
    }
}
