using UnityEngine;

namespace character
{
    public class CharacterSkinPreview : MonoBehaviour
    {
        [SerializeField] private CharacterSkin _characterSkin;


        public void SetSkin(int skinId)
        {
            SetFront();
            _characterSkin.SetSkin(skinId);
        }

        public void SetCape(int capeId)
        {
            SetBack();
            _characterSkin.SetCape(capeId);
        }

        private void SetFront()
        {
            _characterSkin.transform.eulerAngles = Vector3.zero;
        }

        private void SetBack()
        {
            _characterSkin.transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }
}