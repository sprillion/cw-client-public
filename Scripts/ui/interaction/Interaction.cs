using System;
using System.Collections.Generic;
using System.Linq;
using environment;
using infrastructure.services.players;
using UnityEngine;

namespace ui.interaction
{
    public class Interaction
    {
        private readonly HashSet<IInteractable> _interactions = new HashSet<IInteractable>();

        private readonly ICharacterService _characterService;
        private readonly InteractButton _interactButton;
        private readonly GameObject _interactPanel;

        public event Action<IInteractable> OnInteract;

        public Interaction(ICharacterService characterService, InteractButton interactButton, GameObject panel)
        {
            _characterService = characterService;
            _interactButton = interactButton;
            _interactPanel = panel;
            
            _characterService.CurrentCharacter.InteractDetector.OnInteractEnter += AddInteraction;
            _characterService.CurrentCharacter.InteractDetector.OnInteractExit += RemoveInteraction;
        }
        
        public void Interact()
        {
            var interactable = _interactions.LastOrDefault();
            interactable?.Interact();
            OnInteract?.Invoke(interactable);
            CheckPanelToDisable(interactable);
        }

        private void AddInteraction(IInteractable interaction)
        {
            _interactions.Add(interaction);

            interaction.OnDestroyed += RemoveInteraction;
            
            if (interaction is not MonoBehaviour mb) return;
            _interactPanel.transform.position = mb.transform.position;
            _interactPanel.SetActive(true);
            _interactButton.gameObject.SetActive(true);
        }

        private void RemoveInteraction(IInteractable interaction)
        {
            _interactions.Remove(interaction);
            
            interaction.OnDestroyed -= RemoveInteraction;
            
            if (_interactions.Count == 0)
            {
                _interactButton.gameObject.SetActive(false);
                _interactPanel.SetActive(false);
            }
            else
            {
                if (_interactions.LastOrDefault() is not MonoBehaviour mb) return;
                _interactPanel.transform.position = mb.transform.position;
            }
        }

        private void CheckPanelToDisable(IInteractable interactable)
        {
            _interactPanel.SetActive(!interactable.DisablePanel);
            _interactButton.gameObject.SetActive(!interactable.DisableButton);
        }
    }
}