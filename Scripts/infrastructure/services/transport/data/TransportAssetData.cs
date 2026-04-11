using UnityEngine;

namespace infrastructure.services.transport
{
    [CreateAssetMenu(fileName = "TransportAssetData", menuName = "Data/Transport/TransportAssetData")]
    public class TransportAssetData : ScriptableObject
    {
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public TransportModelType ModelType { get; private set; }
        [field: SerializeField] public Material Material { get; private set; }
        [field: SerializeField] public Sprite PreviewIcon { get; private set; }
    }
}
