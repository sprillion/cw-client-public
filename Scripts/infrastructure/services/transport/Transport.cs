using System;
using character;
using infrastructure.services.players;
using UnityEngine;

namespace infrastructure.services.transport
{
    public class Transport : PooledObject
    {
        [SerializeField] private TransportMovement _transportMovement;
        [SerializeField] private TransportInteractTrigger _interactTrigger;

        private TransportModel _model;

        public Action OnInteractRequested;

        public void EnableInteract()  => _interactTrigger.gameObject.SetActive(true);
        public void DisableInteract() => _interactTrigger.gameObject.SetActive(false);

        public void Initialize()
        {
            _interactTrigger.Initialize(this);
        }

        public void SetModel(TransportModel model)
        {
            _model = model;
            _model.transform.SetParent(transform);
            _model.transform.localPosition = Vector3.zero;
            _model.transform.localRotation = Quaternion.identity;
            _transportMovement.DisableMovement();
        }

        public void SetSpeed(float speed) => _transportMovement.SetSpeed(speed);

        public void StartAutopilot(Vector3 direction) => _transportMovement.StartAutopilot(direction);
        public void StopAutopilot()                   => _transportMovement.StopAutopilot();

        public void SetPosition(Vector3 position)
        {
            _transportMovement.SetPosition(position);
        }
        
        public void MountLocal(Character character, ICharacterService characterService)
        {
            character.CharacterMovement.DisableMovement();
            characterService.StopSendPosition();
            character.CharacterSkin.gameObject.SetActive(false);
            _model.MountRider(character.CharacterSkin);
            character.CharacterMovement.DisableCamera();
            _model.EnableCamera();
            _transportMovement.SetRider(character.CharacterMovement);
            _transportMovement.EnableMovement();
        }

        public void DismountLocal(Character character, ICharacterService characterService)
        {
            _transportMovement.DisableMovement();
            _transportMovement.ClearRider();
            _model.DismountRider();
            _model.DisableCamera();
            character.CharacterMovement.SetPosition(transform.position);
            character.CharacterMovement.EnableCamera();
            character.CharacterSkin.gameObject.SetActive(true);
            character.CharacterMovement.EnableMovement();
            characterService.ResumeSendPosition();
        }

        public override void Release()
        {
            OnInteractRequested = null;
            _interactTrigger.NotifyDestroyed();
            _interactTrigger.gameObject.SetActive(false);
            _model?.Release();
            _model = null;
            base.Release();
        }
    }
}
