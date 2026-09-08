using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Swarm
{
    public sealed class MatchDirector : MonoBehaviour
    {
        private const float MatchDuration = 60f;
        private const float FinalRushSeconds = 15f;

        private OneHandInputSource _input;
        private PlayerMotor _motor;
        private TerritorySystem _territory;
        private SwarmController _swarm;
        private ToyHud _hud;
        private BattleArenaDirector _battle;
        private PickupSystem _pickups;
        private float _timeRemaining;
        private bool _finalRushTriggered;

        public float TimeRemaining => Mathf.Max(0f, _timeRemaining);
        public bool HasStarted { get; private set; }
        public bool IsEnded { get; private set; }
        public event Action MatchEnded;

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

        public void BindBattle(BattleArenaDirector battle)
        {
            _battle = battle;
            if (_battle != null) _battle.SetRunning(false);
        }

        public void BindPickups(PickupSystem pickups)
        {
            _pickups = pickups;
        }

        private void Update()
        {
            if (!HasStarted)
            {
                if (_input != null && _input.HasEverMoved)
                {
                    HasStarted = true;
                    if (_battle != null) _battle.SetRunning(true);
                }
                return;
            }

            if (!IsEnded)
            {
                _timeRemaining -= Time.deltaTime;

                if (!_finalRushTriggered && _timeRemaining <= FinalRushSeconds)
                {
                    _finalRushTriggered = true;
                    if (_battle != null) _battle.SetFinalRush(true);
                }

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
            if (_battle != null) _battle.SetRunning(false);
            if (_hud != null) _hud.ShowResult();
            MatchEnded?.Invoke();
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
