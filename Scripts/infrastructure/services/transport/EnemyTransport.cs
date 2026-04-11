using character;
using UnityEngine;

namespace infrastructure.services.transport
{
    public class EnemyTransport : PooledObject
    {
        [SerializeField] private EnemyController _enemyController;

        private TransportModel _model;

        public void Initialize()
        {
            _enemyController.Clear();
        }

        public void SetModel(TransportModel model)
        {
            _model = model;
            _model.transform.SetParent(transform);
            _model.transform.localPosition = Vector3.zero;
            _model.transform.localRotation = Quaternion.identity;
            _model.DisableCamera();
        }

        public void ApplySnapshot(EnemySnapshot snapshot)
        {
            _enemyController.ApplySnapshot(snapshot);
        }

        public void Mount(CharacterSkin characterSkin)
        {
            _model.MountRider(characterSkin);
        }

        public void Dismount()
        {
            _model.DismountRider();
        }

        public override void Release()
        {
            _model?.Release();
            _model = null;
            _enemyController.Clear();
            base.Release();
        }
    }
}
