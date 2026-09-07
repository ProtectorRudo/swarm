using UnityEngine;

namespace Swarm
{
    /// <summary>
    /// Lightweight presentation-only audio for the first phone test. Uses generated clips so the prototype has
    /// reward/threat feedback without committing to production audio assets.
    /// </summary>
    public sealed class FeedbackDirector : MonoBehaviour
    {
        private AudioSource _source;
        private TerritorySystem _territory;
        private MatchDirector _match;
        private AudioClip _pickup;
        private AudioClip _bonusPickup;
        private AudioClip _capture;
        private AudioClip _cut;
        private AudioClip _matchEnd;

        public void Initialize(TerritorySystem territory, MatchDirector match)
        {
            _territory = territory;
            _match = match;

            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;

            _pickup = CreateSweep("Pickup", 720f, 980f, 0.055f, 0.18f);
            _bonusPickup = CreateSweep("BonusPickup", 560f, 1260f, 0.11f, 0.26f);
            _capture = CreateSweep("Capture", 280f, 650f, 0.19f, 0.34f);
            _cut = CreateSweep("TrailCut", 190f, 72f, 0.23f, 0.36f);
            _matchEnd = CreateSweep("MatchEnd", 390f, 760f, 0.24f, 0.30f);

            if (_territory != null)
            {
                _territory.CaptureCompleted += OnCapture;
                _territory.TrailCut += OnTrailCut;
            }
            if (_match != null)
                _match.MatchEnded += OnMatchEnded;
        }

        public void NotifyPickup(bool bonus)
        {
            Play(bonus ? _bonusPickup : _pickup);
        }

        private void OnCapture(float percent, int cells)
        {
            Play(_capture);
        }

        private void OnTrailCut()
        {
            Play(_cut);
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
            {
                _territory.CaptureCompleted -= OnCapture;
                _territory.TrailCut -= OnTrailCut;
            }
            if (_match != null)
                _match.MatchEnded -= OnMatchEnded;
        }
    }
}
