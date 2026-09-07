using UnityEngine;
using UnityEngine.SceneManagement;

namespace Swarm
{
    public sealed class MatchDirector : MonoBehaviour
    {
        private const float MatchDuration = 60f;

        private OneHandInputSource _input;
        private PlayerMotor _motor;
        private TerritorySystem _territory;
        private SwarmController _swarm;
        private ToyHud _hud;
        private RivalBot _rival;
        private float _timeRemaining;

        public float TimeRemaining => Mathf.Max(0f, _timeRemaining);
        public bool IsEnded { get; private set; }

        public void Initialize(
            OneHandInputSource input,
            PlayerMotor motor,
            TerritorySystem territory,
            SwarmController swarm,
            ToyHud hud)
        {
            _input = input;
            _motor = motor;
            _territory = territory;
            _swarm = swarm;
            _hud = hud;
            _timeRemaining = MatchDuration;
            hud.BindMatch(this, territory);
        }

        public void BindRival(RivalBot rival)
        {
            _rival = rival;
        }

        private void Update()
        {
            if (!IsEnded)
            {
                _timeRemaining -= Time.deltaTime;
                if (_timeRemaining <= 0f)
                    EndMatch();
                return;
            }

            if (_input != null && _input.PressedThisFrame)
                Restart();
        }

        private void EndMatch()
        {
            IsEnded = true;
            _timeRemaining = 0f;
            if (_motor != null) _motor.SetMovementEnabled(false);
            if (_territory != null) _territory.enabled = false;
            if (_rival != null) _rival.enabled = false;
            if (_hud != null)
                _hud.ShowResult(_territory != null ? _territory.OwnedPercent : 0f, _swarm != null ? _swarm.Count : 0);
        }

        private static void Restart()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex >= 0)
                SceneManager.LoadScene(scene.buildIndex);
            else
                SceneManager.LoadScene(scene.name);
        }
    }
}
