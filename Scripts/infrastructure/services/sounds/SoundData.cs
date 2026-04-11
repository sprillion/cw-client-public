using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace infrastructure.services.sounds
{
    [CreateAssetMenu(fileName = "Sound Data", menuName = "Data/Sound Data")]
    public class SoundData : ScriptableObject
    {
        [field: SerializeField] public AudioClip Clip { get; private set; }
        [field: SerializeField] public SoundType SoundType { get; private set; }
        [field: SerializeField] public AudioMixerGroup AudioMixerGroup { get; private set; }

        [field: SerializeField, Range(0, 1)] public float Volume { get; private set; } = 1f;

        [field: SerializeField] public bool RandomPitch { get; private set; }
        [field: SerializeField, Range(-1, 2), HideIf("RandomPitch")] public float Pitch { get; private set; } = 1f;
        [field: SerializeField, Range(-1, 2), ShowIf("RandomPitch")] public float PitchMin { get; private set; } = 0.9f;
        [field: SerializeField, Range(-1, 2), ShowIf("RandomPitch")] public float PitchMax { get; private set; } = 1.1f;

        [field: SerializeField] public bool Loop { get; private set; }
        [field: SerializeField] public float Delay { get; private set; }

        [field: SerializeField, Range(0, 1)] public float SpatialBlend { get; private set; }
        [field: SerializeField] public float MinDistance { get; private set; } = 1f;
        [field: SerializeField] public float MaxDistance { get; private set; } = 50f;

        public float GetPitch() => RandomPitch ? Random.Range(PitchMin, PitchMax) : Pitch;
    }
}
