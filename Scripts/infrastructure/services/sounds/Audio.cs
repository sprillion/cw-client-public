using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace infrastructure.services.sounds
{
    public class Audio : PooledObject
    {
        [SerializeField] private AudioSource _audioSource;

        private Coroutine _coroutine;
        private float _delay;

        public void Play(SoundData soundData, Vector3? position = null)
        {
            if (position.HasValue)
                transform.position = position.Value;

            _audioSource.clip = soundData.Clip;
            _audioSource.loop = soundData.Loop;
            _audioSource.pitch = soundData.GetPitch();
            _audioSource.volume = Mathf.Pow(soundData.Volume, 2f);
            _audioSource.outputAudioMixerGroup = soundData.AudioMixerGroup;
            _audioSource.spatialBlend = soundData.SpatialBlend;
            _audioSource.minDistance = soundData.MinDistance;
            _audioSource.maxDistance = soundData.MaxDistance;
            _delay = soundData.Delay;

            _audioSource.PlayDelayed(_delay);

            if (!soundData.Loop)
                _coroutine = StartCoroutine(DelayRelease());
        }

        public void Stop(float fadeDuration)
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }

            _audioSource.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                _audioSource.Stop();
                Release();
            });
        }

        public override void OnGetted()
        {
            _audioSource.volume = 1f;
        }

        private void OnDestroy()
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);
        }

        private IEnumerator DelayRelease()
        {
            yield return new WaitForSeconds(_audioSource.clip.length + _delay + 0.5f);
            _audioSource.Stop();
            Release();
        }
    }
}
