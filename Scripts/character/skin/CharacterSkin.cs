using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace character
{
    public class CharacterSkin : MonoBehaviour
    {
        private static readonly int MainTex = Shader.PropertyToID("_BaseMap");
        private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
        
        private static readonly int CapeTex = Shader.PropertyToID("_Cape");
        private static readonly int MainColor = Shader.PropertyToID("_MainColor");
        private static readonly int EmblemTex = Shader.PropertyToID("_Emblem");
        private static readonly int EmblemColor = Shader.PropertyToID("_EmblemColor");
        private static readonly int EmblemMaskTex = Shader.PropertyToID("_EmblemMask");
        private static readonly int MoveDirId = Shader.PropertyToID("_MoveDir");
        
        [SerializeField] private Renderer _mainSkin;
        [SerializeField] private Renderer _secondSkin;
        [SerializeField] private Renderer _secondHeadSkin;
        
        [SerializeField] private Renderer _cape;
        [SerializeField] private CharacterAnimator _characterAnimator;

        [SerializeField] private Renderer _headArmor;
        [SerializeField] private Renderer _bodyArmor;
        [SerializeField] private Renderer _legsArmor;
        [SerializeField] private Renderer _footArmor;
        
        private MaterialPropertyBlock _mainSkinBlock;
        private MaterialPropertyBlock _secondSkinBlock;
        private MaterialPropertyBlock _secondHeadSkinBlock;
         
        private MaterialPropertyBlock _capeBlock;

        private MaterialPropertyBlock _headArmorBlock;
        private MaterialPropertyBlock _bodyArmorBlock;
        private MaterialPropertyBlock _legsArmorBlock;
        private MaterialPropertyBlock _footArmorBlock;

        public static Func<int, UniTask<SkinData>> SkinLoader;
        public static Func<int, UniTask<CapeData>> CapeLoader;

        private ArmorData _armorData;

        private bool _initialized;
        
        public ArmorType CurrentArmorHead { get; private set; }
        public ArmorType CurrentArmorBody { get; private set; }
        public ArmorType CurrentArmorLegs { get; private set; }
        public ArmorType CurrentArmorFoot { get; private set; }

        public event Action<ArmorPlaceType> OnArmorChanged;
        public event Action OnSkinChanged;
        
        public int SkinId { get; private set; } 
        public int CapeId { get; private set; } 
        

        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            
            _mainSkinBlock = new MaterialPropertyBlock(); 
            _secondSkinBlock = new MaterialPropertyBlock(); 
            _secondHeadSkinBlock = new MaterialPropertyBlock(); 
            
            _capeBlock = new MaterialPropertyBlock();
            
            _headArmorBlock = new MaterialPropertyBlock(); 
            _bodyArmorBlock = new MaterialPropertyBlock(); 
            _legsArmorBlock = new MaterialPropertyBlock(); 
            _footArmorBlock = new MaterialPropertyBlock(); 
            
            _mainSkin.GetPropertyBlock(_mainSkinBlock);
            _secondSkin.GetPropertyBlock(_secondSkinBlock);
            _secondHeadSkin.GetPropertyBlock(_secondHeadSkinBlock);
            
            _cape.GetPropertyBlock(_capeBlock);

            _headArmor.GetPropertyBlock(_headArmorBlock);
            _bodyArmor.GetPropertyBlock(_bodyArmorBlock);
            _legsArmor.GetPropertyBlock(_legsArmorBlock);
            _footArmor.GetPropertyBlock(_footArmorBlock);

            _armorData = GameResources.Data.Character.armor_data<ArmorData>();

            if (_characterAnimator != null)
                _characterAnimator.OnMoveDirectionUpdated += OnMoveDirectionUpdated;
        }

        private void OnDestroy()
        {
            if (_characterAnimator != null)
                _characterAnimator.OnMoveDirectionUpdated -= OnMoveDirectionUpdated;
        }

        private void OnMoveDirectionUpdated(Vector3 moveDir)
        {
            if (_cape == null || !_cape.gameObject.activeSelf) return;
            _capeBlock.SetVector(MoveDirId, new Vector4(moveDir.x, moveDir.y, moveDir.z, 0));
            _cape.SetPropertyBlock(_capeBlock);
        }

        public void SetSkin(int skinId)
        {
            SkinId = skinId;
            if (SkinLoader == null) return;
            ApplySkinAsync(skinId).Forget();
        }

        private async UniTaskVoid ApplySkinAsync(int skinId)
        {
            var skinData = await SkinLoader(skinId);
            if (skinData?.Texture == null || SkinId != skinId) return;

            var texture = skinData.Texture;
            _mainSkinBlock.SetTexture(MainTex, texture);
            _secondSkinBlock.SetTexture(MainTex, texture);
            _secondHeadSkinBlock.SetTexture(MainTex, texture);

            _mainSkin.SetPropertyBlock(_mainSkinBlock);
            _secondSkin.SetPropertyBlock(_secondSkinBlock);
            _secondHeadSkin.SetPropertyBlock(_secondHeadSkinBlock);

            OnSkinChanged?.Invoke();
        }

        public void SetArmor(ArmorType armorType, ArmorPlaceType armorPlaceType)
        {
            var armorRenderer = GerArmorRendere(armorPlaceType);
            var materialBlock = GerMaterialBlock(armorPlaceType);
            SetValue(armorType, armorPlaceType);
            
            if (armorType == ArmorType.None)
            {
                armorRenderer.gameObject.SetActive(false);
                return;
            }
            
            materialBlock.SetTexture(MainTex, _armorData.GetTexture(armorType, armorPlaceType));
            materialBlock.SetTexture(BumpMap, _armorData.ArmorTexturesMap[armorType].NormalMap);

            armorRenderer.SetPropertyBlock(materialBlock);
            
            armorRenderer.gameObject.SetActive(true);
        }

        public void SetCape(int capeId)
        {
            CapeId = capeId;

            if (_cape == null) return;

            if (capeId == -1)
            {
                _cape.gameObject.SetActive(false);
                return;
            }

            if (CapeLoader == null) return;
            ApplyCapeAsync(capeId).Forget();
        }

        private async UniTaskVoid ApplyCapeAsync(int capeId)
        {
            var capeData = await CapeLoader(capeId);
            if (capeData == null || CapeId != capeId) return;

            _capeBlock.SetTexture(CapeTex, capeData.CapeTexture != null ? capeData.CapeTexture : Texture2D.whiteTexture);
            _capeBlock.SetColor(MainColor, capeData.CapeColor);
            _capeBlock.SetTexture(EmblemTex, capeData.EmblemTexture != null ? capeData.EmblemTexture : Texture2D.whiteTexture);
            _capeBlock.SetColor(EmblemColor, capeData.EmblemColor);
            _capeBlock.SetTexture(EmblemMaskTex, capeData.EmblemMaskTexture != null ? capeData.EmblemMaskTexture : Texture2D.blackTexture);

            _cape.SetPropertyBlock(_capeBlock);
            _cape.gameObject.SetActive(true);
        }

        private Renderer GerArmorRendere(ArmorPlaceType armorPlaceType)
        {
            return armorPlaceType switch
            {
                ArmorPlaceType.Head => _headArmor,
                ArmorPlaceType.Body => _bodyArmor,
                ArmorPlaceType.Legs => _legsArmor,
                ArmorPlaceType.Foot => _footArmor,
                _ => null
            };
        }
        
        private MaterialPropertyBlock GerMaterialBlock(ArmorPlaceType armorPlaceType)
        {
            return armorPlaceType switch
            {
                ArmorPlaceType.Head => _headArmorBlock,
                ArmorPlaceType.Body => _bodyArmorBlock,
                ArmorPlaceType.Legs => _legsArmorBlock,
                ArmorPlaceType.Foot => _footArmorBlock,
                _ => null
            };
        }

        private void SetValue(ArmorType armorType, ArmorPlaceType armorPlaceType)
        {
            switch (armorPlaceType)
            {
                case ArmorPlaceType.Head:
                    CurrentArmorHead = armorType;
                    break;
                case ArmorPlaceType.Body:
                    CurrentArmorBody = armorType;
                    break;
                case ArmorPlaceType.Legs:
                    CurrentArmorLegs = armorType;
                    break;
                case ArmorPlaceType.Foot:
                    CurrentArmorFoot = armorType;
                    break;
            }
            OnArmorChanged?.Invoke(armorPlaceType);
        }
    }
}