using UnityEngine;

namespace Swarm
{
    public sealed class ToyHud : MonoBehaviour
    {
        private OneHandInputSource _input;
        private SwarmController _swarm;
        private GUIStyle _titleStyle;
        private GUIStyle _instructionStyle;
        private GUIStyle _debugStyle;
        private float _pickupPulse;
        private float _smoothedDelta;

        public void Initialize(OneHandInputSource input, SwarmController swarm)
        {
            _input = input;
            _swarm = swarm;
        }

        public void NotifyPickup()
        {
            _pickupPulse = 1f;
        }

        private void Update()
        {
            _pickupPulse = Mathf.MoveTowards(_pickupPulse, 0f, Time.deltaTime * 3.6f);
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
            float titleScale = 1f + _pickupPulse * 0.10f;
            _titleStyle.fontSize = Mathf.RoundToInt(48f * titleScale);
            GUI.Label(new Rect(0f, 52f, width, 80f), "SWARM  " + count, _titleStyle);

            string instruction;
            if (_input == null || !_input.HasEverMoved)
                instruction = "ARRASTRÁ PARA MOVERTE";
            else if (count < 3)
                instruction = "JUNTÁ LOS PUNTOS";
            else if (count < 12)
                instruction = "JUNTÁ • CRECÉ • SEGUÍ";
            else
                instruction = "¿HASTA CUÁNTO PODÉS CRECER?";

            GUI.Label(new Rect(45f, height - 220f, width - 90f, 110f), instruction, _instructionStyle);

            float fps = _smoothedDelta > 0.0001f ? 1f / _smoothedDelta : 0f;
            GUI.Label(new Rect(24f, 20f, 240f, 40f), "0.1 TOY   " + fps.ToString("0") + " FPS", _debugStyle);
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null) return;

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 48,
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
                alignment = TextAnchor.UpperLeft,
                fontSize = 18
            };
            _debugStyle.normal.textColor = new Color(1f, 1f, 1f, 0.45f);
        }
    }
}
