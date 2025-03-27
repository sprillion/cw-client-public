using System.Collections.Generic;
using UnityEngine;

namespace infrastructure.services.mobs
{
    public class MobModel : PooledObject
    {
        [SerializeField] private MobModelType _mobModelType;
        
        [SerializeField] private Renderer _firstLayer;
        [SerializeField] private Renderer _secondLayer;
        [SerializeField] private Renderer _secondHeadLayer;

        [SerializeField] private MobAnimator _mobAnimator;

        public MobModelType MobModelType => _mobModelType;

        public MobAnimator MobAnimator => _mobAnimator;

        public void Initialize(MobData mobData)
        {
            _mobAnimator.SetAnimatorController(mobData.AnimatorController);
            
            _firstLayer.SetMaterials(new List<Material>(){mobData.Material});
            
            _secondLayer.gameObject.SetActive(mobData.HaveSecondLayerSkin);
            _secondHeadLayer.gameObject.SetActive(mobData.HaveSecondLayerSkin);
            
            if (mobData.HaveSecondLayerSkin)
            {
                _secondLayer.SetMaterials(new List<Material>(){mobData.Material});
                _secondHeadLayer.SetMaterials(new List<Material>(){mobData.Material});
            }
        }
    }
}