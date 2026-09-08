using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Lightweight generated audio for the Battle Arena prototype. Production sound is intentionally deferred.
    /// </summary>
    public sealed class FeedbackDirector : MonoBehaviour
    {
        private AudioSource _source;
        private TerritorySystem _territory;
        private MatchDirector _match;
        private BattleArenaDirector _battle;
        private AudioClip _pickup;
        private AudioClip _bonusPickup;
        private AudioClip _expand;
        private AudioClip _shot;
        private AudioClip _hit;
        private AudioClip _hurt;
        private AudioClip _shield;
        private AudioClip _ko;
        private AudioClip _matchEnd;

        public void Initialize(TerritorySystem territory, MatchDirector match, BattleArenaDirector battle)
        {
            _territory = territory;
            _match = match;
            _battle = battle;

            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;

            _pickup = CreateSweep("Pickup", 720f, 980f, 0.050f, 0.15f);
            _bonusPickup = CreateSweep("BonusPickup", 560f, 1320f, 0.10f, 0.24f);
            _expand = CreateSweep("Expand", 300f, 610f, 0.11f, 0.17f);
            _shot = CreateSweep("Shot", 980f, 620f, 0.045f, 0.10f);
            _hit = CreateSweep("Hit", 520f, 820f, 0.065f, 0.15f);
            _hurt = CreateSweep("Hurt", 260f, 120f, 0.085f, 0.18f);
            _shield = CreateSweep("Shield", 860f, 1140f, 0.055f, 0.10f);
            _ko = CreateSweep("KO", 260f, 1080f, 0.21f, 0.34f);
            _matchEnd = CreateSweep("MatchEnd", 390f, 760f, 0.24f, 0.28f);

            if (_territory != null)
                _territory.PlayerExpanded += OnPlayerExpanded;
            if (_battle != null)
            {
                _battle.ShotFired += OnShotFired;
                _battle.ShotHit += OnShotHit;
                _battle.ParticipantDefeated += OnParticipantDefeated;
            }
            if (_match != null)
                _match.MatchEnded += OnMatchEnded;
        }

        public void NotifyPickup(bool bonus)
        {
            Play(bonus ? _bonusPickup : _pickup, 1f);
        }

        private void OnPlayerExpanded(float percent, int cells, int enemyCells)
        {
            if (enemyCells >= 5)
                Play(_expand, 0.72f);
        }

        private void OnShotFired(int owner)
        {
            if (owner == BattlePalette.PlayerOwner)
                Play(_shot, 0.62f);
        }

        private void OnShotHit(int shooter, int target, int remaining, bool defended)
        {
            if (shooter != BattlePalette.PlayerOwner && target != BattlePalette.PlayerOwner) return;

            if (defended)
                Play(_shield, 0.58f);
            else if (shooter == BattlePalette.PlayerOwner)
                Play(_hit, 0.72f);
            else if (target == BattlePalette.PlayerOwner)
                Play(_hurt, 0.78f);
        }

        private void OnParticipantDefeated(int winner, int loser)
        {
            if (winner == BattlePalette.PlayerOwner || loser == BattlePalette.PlayerOwner)
                Play(_ko, 1f);
        }

        private void OnMatchEnded()
        {
            Play(_matchEnd, 1f);
        }

        private void Play(AudioClip clip, float volume)
        {
            if (_source == null || clip == null) return;
            _source.PlayOneShot(clip, Mathf.Clamp01(volume));
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
            if (_battle != null)
            {
                _battle.ShotFired -= OnShotFired;
                _battle.ShotHit -= OnShotHit;
                _battle.ParticipantDefeated -= OnParticipantDefeated;
            }
            if (_match != null)
                _match.MatchEnded -= OnMatchEnded;
        }
    }
}
