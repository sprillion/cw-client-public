using UnityEngine;

namespace infrastructure.services.sounds
{
    public interface ISoundService
    {
        Audio Play(SoundType soundType);
        Audio Play3D(SoundType soundType, Vector3 position);
        void PlayMusic(SoundType soundType);
        void StopMusic(float fadeDuration = 1f);
    }
}
