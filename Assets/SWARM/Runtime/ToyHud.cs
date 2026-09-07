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

        private GUIStyle _titleStyle;
        private GUIStyle _instructionStyle;
        private GUIStyle _debugStyle;
        private GUIStyle _resultStyle;
        private GUIStyle _resultSubStyle;

        private float _pickupPulse;
        private float _capturePulse;
        private float _dangerPulse;
        private float _smoothedDelta;
        private string _captureMessage = string.Empty;
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

        public void BindTelemetry(FirstTestTelemetry telemetry)
        {
            _telemetry = telemetry;
        }

        public void NotifyPickup()
        {
            _pickupPulse = 1f;
        }

        public void NotifyCapture(float percent, int cells)
        {
            _capturePulse = 1f;
            _captureMessage = "+" + cells + " CELDAS  •  " + (percent * 100f).ToString("0.0") + "%";
        }

        public void NotifyTrailCut()
        {
            _dangerPulse = 1f;
        }

        public void ShowResult(float percent, int swarmCount)
        {
            _showResult = true;
            _resultPercent = percent;
            _resultSwarm = swarmCount;
        }

        private void Update()
        {
            _pickupPulse = Mathf.MoveTowards(_pickupPulse, 0f, Time.deltaTime * 3.6f);
            _capturePulse = Mathf.MoveTowards(_capturePulse, 0f, Time.deltaTime * 1.2f);
            _dangerPulse = Mathf.MoveTowards(_dangerPulse, 0f, Time.deltaTime * 1.6f);
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
            float ownedPercent = _territory != null ? _territory.OwnedPercent : 0f;
            float titleScale = 1f + _pickupPulse * 0.10f;
            _titleStyle.fontSize = Mathf.RoundToInt(46f * titleScale);

            GUI.Label(new Rect(28f, 44f, width * 0.48f, 72f), "SWARM  " + count, _titleStyle);
            GUI.Label(new Rect(width * 0.50f, 44f, width * 0.47f, 72f), "ÁREA  " + (ownedPercent * 100f).ToString("0.0") + "%", _titleStyle);

            if (_match != null)
            {
                int seconds = Mathf.CeilToInt(_match.TimeRemaining);
                GUI.Label(new Rect(width * 0.5f - 100f, 115f, 200f, 48f), seconds.ToString("00") + "s", _debugStyle);
            }

            if (_dangerPulse > 0.01f)
            {
                Color old = _resultStyle.normal.textColor;
                _resultStyle.normal.textColor = new Color(1f, 0.25f, 0.30f, Mathf.Clamp01(_dangerPulse * 1.5f));
                _resultStyle.fontSize = 62 + Mathf.RoundToInt(_dangerPulse * 14f);
                GUI.Label(new Rect(30f, height * 0.22f, width - 60f, 120f), "¡TE CORTARON!", _resultStyle);
                _resultStyle.normal.textColor = old;
                _resultStyle.fontSize = 54;
            }
            else if (_capturePulse > 0.01f && !string.IsNullOrEmpty(_captureMessage))
            {
                float alpha = Mathf.Clamp01(_capturePulse * 1.5f);
                Color old = _instructionStyle.normal.textColor;
                _instructionStyle.normal.textColor = new Color(0.45f, 1f, 0.66f, alpha);
                _instructionStyle.fontSize = 38 + Mathf.RoundToInt(_capturePulse * 10f);
                GUI.Label(new Rect(35f, height * 0.25f, width - 70f, 90f), _captureMessage, _instructionStyle);
                _instructionStyle.normal.textColor = old;
                _instructionStyle.fontSize = 34;
            }

            string instruction = GetInstruction(count, ownedPercent);
            GUI.Label(new Rect(45f, height - 220f, width - 90f, 110f), instruction, _instructionStyle);

            float fps = _smoothedDelta > 0.0001f ? 1f / _smoothedDelta : 0f;
            GUI.Label(new Rect(24f, 16f, 280f, 36f), "FIRST TEST SLICE   " + fps.ToString("0") + " FPS", _debugStyle);

            if (_showResult)
                DrawResult(width, height);
        }

        private string GetInstruction(int count, float ownedPercent)
        {
            if (_showResult) return string.Empty;
            if (_input == null || !_input.HasEverMoved)
                return "ARRASTRÁ CON UN DEDO PARA MOVERTE";
            if (count < 3)
                return "JUNTÁ LOS PUNTOS • HACÉ CRECER TU SWARM";
            if (_territory != null && _territory.CaptureCount == 0)
            {
                if (_territory.IsTrailExposed)
                    return "VOLVÉ A TU COLOR PARA CERRAR LA VUELTA";
                return "SALÍ DE TU COLOR • HACÉ UNA VUELTA • VOLVÉ";
            }
            if (_territory != null && _territory.IsTrailExposed)
                return "¡CERRÁ LA VUELTA ANTES DE QUE EL ROJO TE CORTE!";
            if (ownedPercent < 0.15f)
                return "CERRÁ VUELTAS MÁS GRANDES • CUIDATE DEL ROJO";
            return "DOMINÁ TODO LO QUE PUEDAS ANTES DE QUE TERMINE EL TIEMPO";
        }

        private void DrawResult(float width, float height)
        {
            GUI.Box(new Rect(45f, height * 0.24f, width - 90f, 570f), GUIContent.none);
            string verdict = _resultPercent >= 0.25f ? "DOMINASTE" : _resultPercent >= 0.12f ? "BUENA EXPANSIÓN" : "PODÉS CRECER MÁS";
            GUI.Label(new Rect(70f, height * 0.27f, width - 140f, 90f), verdict, _resultStyle);
            GUI.Label(new Rect(70f, height * 0.35f, width - 140f, 135f), (_resultPercent * 100f).ToString("0.0") + "% DEL MAPA\nSWARM FINAL  " + _resultSwarm, _resultSubStyle);

            if (_telemetry != null)
            {
                string firstCapture = _telemetry.TimeToFirstCapture >= 0f
                    ? _telemetry.TimeToFirstCapture.ToString("0.0") + "s"
                    : "NO";
                string metrics =
                    "PRIMERA CAPTURA  " + firstCapture +
                    "\nCAPTURAS  " + _telemetry.Captures +
                    "   •   CORTES  " + _telemetry.Cuts +
                    "   •   MÁX SWARM  " + _telemetry.MaxSwarm;
                GUI.Label(new Rect(75f, height * 0.47f, width - 150f, 120f), metrics, _debugStyle);
            }

            GUI.Label(new Rect(70f, height * 0.58f, width - 140f, 100f), "TOCÁ PARA JUGAR OTRA", _instructionStyle);
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
            _debugStyle.normal.textColor = new Color(1f, 1f, 1f, 0.55f);

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
                fontSize = 38,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            _resultSubStyle.normal.textColor = new Color(0.56f, 0.95f, 1f, 1f);
        }
    }
}
