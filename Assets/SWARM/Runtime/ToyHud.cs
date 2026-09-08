using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Battle Arena HUD: all rivals are labeled in-world, while the top bar keeps the human focused on army size,
    /// remaining time and current position in the free-for-all.
    /// </summary>
    public sealed class ToyHud : MonoBehaviour
    {
        private OneHandInputSource _input;
        private SwarmController _swarm;
        private TerritorySystem _territory;
        private MatchDirector _match;
        private FirstTestTelemetry _telemetry;
        private BattleArenaDirector _battle;

        private GUIStyle _titleStyle;
        private GUIStyle _instructionStyle;
        private GUIStyle _debugStyle;
        private GUIStyle _resultStyle;
        private GUIStyle _resultSubStyle;
        private GUIStyle _worldLabelStyle;
        private GUIStyle _rankStyle;

        private float _pickupPulse;
        private float _eventPulse;
        private float _smoothedDelta;
        private string _eventMessage = string.Empty;
        private Color _eventColor = Color.white;
        private bool _showResult;

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

        public void BindBattle(BattleArenaDirector battle)
        {
            if (_battle != null)
            {
                _battle.CombatResolved -= OnCombatResolved;
                _battle.ParticipantDefeated -= OnParticipantDefeated;
                _battle.TerritoryConverted -= OnTerritoryConverted;
            }

            _battle = battle;
            if (_battle != null)
            {
                _battle.CombatResolved += OnCombatResolved;
                _battle.ParticipantDefeated += OnParticipantDefeated;
                _battle.TerritoryConverted += OnTerritoryConverted;
            }
        }

        public void BindTelemetry(FirstTestTelemetry telemetry)
        {
            _telemetry = telemetry;
        }

        public void NotifyPickup(int value)
        {
            _pickupPulse = 1f;
            if (value >= 5)
            {
                _eventPulse = 1f;
                _eventMessage = "+5 • MEGA BICHO";
                _eventColor = new Color(1f, 0.52f, 0.92f, 1f);
            }
            else if (value >= 3)
            {
                _eventPulse = 0.75f;
                _eventMessage = "+3 • BICHO RARO";
                _eventColor = new Color(0.38f, 0.95f, 1f, 1f);
            }
        }

        public void NotifyExpansion(float percent, int cells, int enemyCells)
        {
            if (enemyCells < 5) return;
            _eventPulse = 0.62f;
            _eventMessage = "ROBASTE " + enemyCells + " CELDAS";
            _eventColor = new Color(0.48f, 1f, 0.68f, 1f);
        }

        public void ShowResult()
        {
            _showResult = true;
        }

        private void OnDestroy()
        {
            if (_battle != null)
            {
                _battle.CombatResolved -= OnCombatResolved;
                _battle.ParticipantDefeated -= OnParticipantDefeated;
                _battle.TerritoryConverted -= OnTerritoryConverted;
            }
        }

        private void OnCombatResolved(int winner, int loser, bool decisive)
        {
            if (winner != BattlePalette.PlayerOwner && loser != BattlePalette.PlayerOwner) return;

            _eventPulse = decisive ? 1.35f : 0.82f;
            if (winner == BattlePalette.PlayerOwner)
            {
                _eventMessage = decisive
                    ? "¡TE COMISTE A " + BattlePalette.Name(loser) + "!"
                    : "¡LO ESTÁS GANANDO!";
                _eventColor = new Color(0.48f, 1f, 0.68f, 1f);
            }
            else
            {
                _eventMessage = decisive
                    ? BattlePalette.Name(winner) + " TE COMIÓ"
                    : "¡ES MÁS FUERTE!";
                _eventColor = new Color(1f, 0.30f, 0.34f, 1f);
            }
        }

        private void OnParticipantDefeated(int winner, int loser)
        {
            if (winner != BattlePalette.PlayerOwner && loser != BattlePalette.PlayerOwner) return;
            _eventPulse = 1.55f;
            if (winner == BattlePalette.PlayerOwner)
            {
                _eventMessage = "KO " + BattlePalette.Name(loser) + " • ABSORBISTE SU SWARM";
                _eventColor = new Color(0.50f, 1f, 0.66f, 1f);
            }
            else
            {
                _eventMessage = "TE ELIMINÓ " + BattlePalette.Name(winner) + " • VOLVÉS CON 5";
                _eventColor = new Color(1f, 0.27f, 0.32f, 1f);
            }
        }

        private void OnTerritoryConverted(int owner, int changed, int enemyCells)
        {
            if (owner == BattlePalette.PlayerOwner)
                NotifyExpansion(_territory != null ? _territory.PlayerOwnedPercent : 0f, changed, enemyCells);
        }

        private void Update()
        {
            _pickupPulse = Mathf.MoveTowards(_pickupPulse, 0f, Time.deltaTime * 3.8f);
            _eventPulse = Mathf.MoveTowards(_eventPulse, 0f, Time.deltaTime * 1.25f);
            _smoothedDelta += (Time.unscaledDeltaTime - _smoothedDelta) * 0.08f;
        }

        private void OnGUI()
        {
            EnsureStyles();
            float scale = Mathf.Clamp(Screen.width / 1080f, 0.65f, 1.4f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = Screen.width / scale;
            float height = Screen.height / scale;

            DrawTopBar(width);
            DrawBotLabels(scale, width, height);
            DrawRanking(width);

            if (_eventPulse > 0.01f && !string.IsNullOrEmpty(_eventMessage))
            {
                Color old = _instructionStyle.normal.textColor;
                _instructionStyle.normal.textColor = new Color(_eventColor.r, _eventColor.g, _eventColor.b, Mathf.Clamp01(_eventPulse * 1.4f));
                _instructionStyle.fontSize = 36 + Mathf.RoundToInt(_eventPulse * 8f);
                GUI.Label(new Rect(35f, height * 0.18f, width - 70f, 90f), _eventMessage, _instructionStyle);
                _instructionStyle.normal.textColor = old;
                _instructionStyle.fontSize = 31;
            }

            if (!_showResult)
                GUI.Label(new Rect(42f, height - 180f, width - 84f, 105f), GetInstruction(), _instructionStyle);

            float fps = _smoothedDelta > 0.0001f ? 1f / _smoothedDelta : 0f;
            GUI.Label(new Rect(18f, 10f, 310f, 34f), "BATTLE ARENA 0.4   " + fps.ToString("0") + " FPS", _debugStyle);

            if (_showResult)
                DrawResult(width, height);
        }

        private void DrawTopBar(float width)
        {
            int count = _swarm != null ? _swarm.Count : 0;
            float owned = _territory != null ? _territory.PlayerOwnedPercent * 100f : 0f;
            float titleScale = 1f + _pickupPulse * 0.08f;
            _titleStyle.fontSize = Mathf.RoundToInt(42f * titleScale);

            GUI.Label(new Rect(22f, 42f, width * 0.42f, 64f), "SWARM  " + count, _titleStyle);
            GUI.Label(new Rect(width * 0.60f, 42f, width * 0.38f, 64f), "MAPA  " + owned.ToString("0.0") + "%", _titleStyle);

            if (_match != null)
            {
                int seconds = Mathf.CeilToInt(_match.TimeRemaining);
                string timer = seconds.ToString("00") + "s";
                if (_battle != null && _battle.IsFinalRush) timer = "⚔ " + timer + " ⚔";
                GUI.Label(new Rect(width * 0.5f - 90f, 48f, 180f, 52f), timer, _rankStyle);
            }
        }

        private void DrawBotLabels(float scale, float width, float height)
        {
            if (_battle == null || Camera.main == null || _showResult) return;
            var bots = _battle.Bots;
            for (int i = 0; i < bots.Count; i++)
            {
                RivalBot bot = bots[i];
                if (bot == null) continue;
                Vector3 screen = Camera.main.WorldToScreenPoint(bot.transform.position);
                if (screen.z <= 0f) continue;

                float x = screen.x / scale;
                float y = height - screen.y / scale;
                if (x < -90f || x > width + 90f || y < -90f || y > height + 90f) continue;

                Color old = _worldLabelStyle.normal.textColor;
                _worldLabelStyle.normal.textColor = BattlePalette.Color(bot.OwnerId);
                GUI.Label(new Rect(x - 90f, y - 64f, 180f, 42f), BattlePalette.Name(bot.OwnerId) + "  " + bot.Count, _worldLabelStyle);
                _worldLabelStyle.normal.textColor = old;
            }
        }

        private void DrawRanking(float width)
        {
            if (_battle == null || _showResult) return;
            int[] top = GetTopOwners(4);
            float y = 108f;
            for (int i = 0; i < top.Length; i++)
            {
                int owner = top[i];
                Color old = _rankStyle.normal.textColor;
                Color color = BattlePalette.Color(owner);
                color.a = owner == BattlePalette.PlayerOwner ? 1f : 0.78f;
                _rankStyle.normal.textColor = color;
                string line = (i + 1) + "  " + BattlePalette.Name(owner) + "  " + _battle.GetCount(owner);
                GUI.Label(new Rect(20f, y + i * 30f, 250f, 30f), line, _rankStyle);
                _rankStyle.normal.textColor = old;
            }
        }

        private string GetInstruction()
        {
            if (_input == null || !_input.HasEverMoved)
                return "ARRASTRÁ CON UN DEDO • TODOS SALEN A LA VEZ";

            int count = _swarm != null ? _swarm.Count : 0;
            if (count < 8)
                return "JUNTÁ BICHITOS • HACÉ CRECER TU EJÉRCITO";

            if (_battle != null && _battle.TryFindThreat(BattlePalette.PlayerOwner, _battle.PlayerTransform.position, count, 4.2f, out _))
                return "HAY UNO MÁS GRANDE CERCA • ESCAPÁ A TU BASE";

            if (_battle != null && _battle.TryFindPrey(BattlePalette.PlayerOwner, _battle.PlayerTransform.position, count, 5.2f, out _, out int preyOwner))
                return "SOS MÁS GRANDE QUE " + BattlePalette.Name(preyOwner) + " • CHOCALO Y COMÉTELO";

            if (_battle != null && _battle.IsFinalRush)
                return "BATALLA FINAL • EL CENTRO TIENE LOS MEJORES BICHITOS";

            return "JUNTÁ • CRECÉ • DOMINÁ • COMÉ AL MÁS CHICO";
        }

        private void DrawResult(float width, float height)
        {
            int leader = _battle != null ? _battle.GetLeaderOwner() : BattlePalette.PlayerOwner;
            int playerRank = GetPlayerRank();
            GUI.Box(new Rect(45f, height * 0.20f, width - 90f, 720f), GUIContent.none);

            string verdict = leader == BattlePalette.PlayerOwner ? "¡GANASTE LA BATALLA!" : "GANÓ " + BattlePalette.Name(leader);
            GUI.Label(new Rect(70f, height * 0.23f, width - 140f, 90f), verdict, _resultStyle);

            string summary =
                "PUESTO  " + playerRank + "/8" +
                "\nSWARM FINAL  " + (_swarm != null ? _swarm.Count : 0) +
                "   •   MAPA  " + (_territory != null ? (_territory.PlayerOwnedPercent * 100f).ToString("0.0") : "0.0") + "%";
            GUI.Label(new Rect(70f, height * 0.32f, width - 140f, 145f), summary, _resultSubStyle);

            if (_battle != null)
            {
                string combat = "KOs  " + _battle.GetKills(BattlePalette.PlayerOwner) + "   •   MUERTES  " + _battle.GetDeaths(BattlePalette.PlayerOwner);
                GUI.Label(new Rect(80f, height * 0.45f, width - 160f, 70f), combat, _resultSubStyle);
            }

            if (_telemetry != null)
            {
                string metrics =
                    "PRIMER +10  " + (_telemetry.TimeToTen >= 0f ? _telemetry.TimeToTen.ToString("0.0") + "s" : "NO") +
                    "   •   MÁX SWARM  " + _telemetry.MaxSwarm +
                    "\nPRIMER COMBATE  " + (_telemetry.TimeToFirstCombat >= 0f ? _telemetry.TimeToFirstCombat.ToString("0.0") + "s" : "NO");
                GUI.Label(new Rect(75f, height * 0.53f, width - 150f, 110f), metrics, _debugStyle);
            }

            GUI.Label(new Rect(70f, height * 0.64f, width - 140f, 90f), "TOCÁ PARA OTRA BATALLA", _instructionStyle);
        }

        private int GetPlayerRank()
        {
            if (_battle == null) return 1;
            float playerScore = _battle.GetScore(BattlePalette.PlayerOwner);
            int rank = 1;
            for (int owner = 2; owner <= BattlePalette.ParticipantCount; owner++)
                if (_battle.GetScore(owner) > playerScore) rank++;
            return rank;
        }

        private int[] GetTopOwners(int count)
        {
            int take = Mathf.Clamp(count, 1, BattlePalette.ParticipantCount);
            int[] owners = new int[BattlePalette.ParticipantCount];
            for (int i = 0; i < owners.Length; i++) owners[i] = i + 1;

            for (int i = 0; i < owners.Length - 1; i++)
            {
                int best = i;
                for (int j = i + 1; j < owners.Length; j++)
                {
                    if (_battle.GetScore(owners[j]) > _battle.GetScore(owners[best]))
                        best = j;
                }
                int temp = owners[i];
                owners[i] = owners[best];
                owners[best] = temp;
            }

            int[] result = new int[take];
            for (int i = 0; i < take; i++) result[i] = owners[i];
            return result;
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null) return;

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 42,
                fontStyle = FontStyle.Bold
            };
            _titleStyle.normal.textColor = Color.white;

            _instructionStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 31,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            _instructionStyle.normal.textColor = new Color(1f, 0.90f, 0.46f, 1f);

            _debugStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                wordWrap = true
            };
            _debugStyle.normal.textColor = new Color(1f, 1f, 1f, 0.58f);

            _resultStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 52,
                fontStyle = FontStyle.Bold
            };
            _resultStyle.normal.textColor = Color.white;

            _resultSubStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            _resultSubStyle.normal.textColor = new Color(0.64f, 0.94f, 1f, 1f);

            _worldLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };

            _rankStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 19,
                fontStyle = FontStyle.Bold
            };
            _rankStyle.normal.textColor = Color.white;
        }
    }
}
