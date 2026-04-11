using UnityEngine;

namespace infrastructure.services.mobs
{
    [CreateAssetMenu(fileName = "MobData", menuName = "Data/Mobs/MobData")]
    public class MobData : ScriptableObject
    {
        [field:SerializeField] public MobType MobType { get; private set; }
        [field:SerializeField] public MobModelType MobModelType { get; private set; }

        [field:SerializeField] public RuntimeAnimatorController AnimatorController { get; private set; }
        [field:SerializeField] public Material Material { get; private set; }
        [field:SerializeField] public bool HaveSecondLayerSkin { get; private set; }
    }
}