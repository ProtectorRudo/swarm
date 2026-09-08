using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Lightweight presentation-only audio for the 0.3 phone test.
    /// </summary>
    public sealed class FeedbackDirector : MonoBehaviour
    {
        private AudioSource _source;
        private TerritorySystem _territory;
        private MatchDirector _match;
        private RivalBot _rival;
        private AudioClip _pickup;
        private AudioClip _bonusPickup;
        private AudioClip _expand;
        private AudioClip _combatWin;
        private AudioClip _combatLose;
        private AudioClip _matchEnd;

        public void Initialize(TerritorySystem territory, MatchDirector match, RivalBot rival)
        {
            _territory = territory;
            _match = match;
            _rival = rival;

            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;

            _pickup = CreateSweep("Pickup", 720f, 980f, 0.055f, 0.18f);
            _bonusPickup = CreateSweep("BonusPickup", 560f, 1260f, 0.11f, 0.26f);
            _expand = CreateSweep("Expand", 300f, 610f, 0.12f, 0.20f);
            _combatWin = CreateSweep("CombatWin", 350f, 920f, 0.16f, 0.31f);
            _combatLose = CreateSweep("CombatLose", 220f, 85f, 0.18f, 0.33f);
            _matchEnd = CreateSweep("MatchEnd", 390f, 760f, 0.24f, 0.30f);

            if (_territory != null)
                _territory.PlayerExpanded += OnPlayerExpanded;
            if (_rival != null)
                _rival.CombatResolved += OnCombatResolved;
            if (_match != null)
                _match.MatchEnded += OnMatchEnded;
        }

        public void NotifyPickup(bool bonus)
        {
            Play(bonus ? _bonusPickup : _pickup);
        }

        private void OnPlayerExpanded(float percent, int cells, int enemyCells)
        {
            if (cells >= 5 || enemyCells > 0)
                Play(_expand);
        }

        private void OnCombatResolved(bool playerAdvantage, int playerCount, int rivalCount)
        {
            Play(playerAdvantage ? _combatWin : _combatLose);
        }

        private void OnMatchEnded()
        {
            Play(_matchEnd);
        }

        private void Play(AudioClip clip)
        {
            if (_source == null || clip == null) return;
            _source.PlayOneShot(clip, 1f);
        }

        private static AudioClip CreateSweep(string name, float startHz, float endHz, float duration, float amplitude)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.Max(64, Mathf.CeilToInt(duration * sampleRate));
            var data = new float[sampleCount];
            float phase = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)(sampleCount - 1);
                float hz = Mathf.Lerp(startHz, endHz, t);
                phase += hz * Mathf.PI * 2f / sampleRate;
                float envelope = Mathf.Sin(Mathf.PI * t);
                envelope *= 1f - t * 0.30f;
                data[i] = Mathf.Sin(phase) * envelope * amplitude;
            }

            AudioClip clip = AudioClip.Create("SWARM_" + name, sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void OnDestroy()
        {
            if (_territory != null)
                _territory.PlayerExpanded -= OnPlayerExpanded;
            if (_rival != null)
                _rival.CombatResolved -= OnCombatResolved;
            if (_match != null)
                _match.MatchEnded -= OnMatchEnded;
        }
    }
}
