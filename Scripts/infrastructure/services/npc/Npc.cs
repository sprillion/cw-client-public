using System;
using System.Collections.Generic;
using environment;
using UnityEngine;

namespace infrastructure.services.npc
{
    public class Npc : PooledObject, IInteractable
    {
        [SerializeField] private List<Renderer> _renderers;
        
        [field: SerializeField] public bool DisablePanel { get; private set; }
        [field: SerializeField] public bool DisableButton { get; private set; }
        public NpcData NpcData { get; private set; }
        
        public event Action<IInteractable> OnDestroyed;

        public override void Release()
        {
            OnDestroyed?.Invoke(this);
            OnDestroyed = null;
            base.Release();
        }

        public void SetData(NpcData data)
        {
            NpcData = data;
            var materials = new List<Material>() { NpcData.Material };
            _renderers.ForEach(r => r.SetMaterials(materials));
        }

        public void Interact()
        {
            
        }
    }
}