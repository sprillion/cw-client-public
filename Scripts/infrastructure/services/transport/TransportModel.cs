using System.Collections.Generic;
using character;
using UnityEngine;

namespace infrastructure.services.transport
{
    public class TransportModel : PooledObject
    {
        [SerializeField] private TransportModelType _modelType;
        [SerializeField] private List<Renderer> _renderers;
        [SerializeField] private CharacterSkin _riderSkin;
        [SerializeField] private CharacterCamera _camera;

        public TransportModelType ModelType => _modelType;

        public void Initialize(TransportAssetData data)
        {
            var materials = new List<Material> { data.Material };
            foreach (var renderer in _renderers)
                renderer.SetMaterials(materials);
            _riderSkin.Initialize();
            _riderSkin.gameObject.SetActive(false);
        }

        public void MountRider(CharacterSkin sourceSkin)
        {
            _riderSkin.SetSkin(sourceSkin.SkinId);
            _riderSkin.SetCape(sourceSkin.CapeId);
            _riderSkin.SetArmor(sourceSkin.CurrentArmorHead, ArmorPlaceType.Head);
            _riderSkin.SetArmor(sourceSkin.CurrentArmorBody, ArmorPlaceType.Body);
            _riderSkin.SetArmor(sourceSkin.CurrentArmorLegs, ArmorPlaceType.Legs);
            _riderSkin.SetArmor(sourceSkin.CurrentArmorFoot, ArmorPlaceType.Foot);
            _riderSkin.gameObject.SetActive(true);
        }

        public void DismountRider()
        {
            _riderSkin.gameObject.SetActive(false);
        }

        public void EnableCamera()  => _camera.EnableCamera();
        public void DisableCamera() => _camera.DisableCamera();
    }
}
