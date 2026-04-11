using System;
using character;
using Cysharp.Threading.Tasks;
using ui.worldUi;
using UnityEngine;

namespace infrastructure.services.mobs
{
    public class Mob : PooledObject
    {
        [SerializeField] private MobController _mobController;

        [SerializeField] private InfoPanel _infoPanel;

        private MobModel _mobModel;
        
        public ushort Id { get; private set; }
        public MobType MobType { get; private set; }
        
        public int MaxHealth { get; private set; }
        public int Level { get; private set; }
        public int CurrentHealth { get; private set; }

        public void Initialize(ushort id, MobType mobType)
        {
            Id = id;
            MobType = mobType;
            _mobController.Initialize();
            _infoPanel.Initialize();
            _infoPanel.gameObject.SetActive(true);
            SetName();
        }

        public override void Release()
        {
            _mobModel.Release();
            _mobModel = null;
            _mobController.EnemyController.Clear();
            base.Release();
        }

        public void SetModel(MobModel mobModel)
        {
            _mobModel = mobModel;
            _mobModel.transform.SetParent(transform);
            _mobController.SetMobAnimator(_mobModel.MobAnimator);
        }

        public void SetPosition(EnemySnapshot snapshot)
        {
            _mobController.EnemyController.ApplySnapshot(snapshot);
        }

        public void SetBaseValues(int maxHealth, int currentHealth, int level)
        {
            MaxHealth = maxHealth;
            _infoPanel.SetMaxHealth(MaxHealth);
            
            SetCurrentHealth(currentHealth);
            
            Level = level;
            _infoPanel.SetLevel(Level);
        }

        public void SetCurrentHealth(int value)
        {
            CurrentHealth = value;
            _infoPanel.SetHealth(CurrentHealth);
        }

        public void Attack()
        {
            _mobController.MobAnimator.Attack();
        }

        public async UniTaskVoid Death()
        {
            _infoPanel.gameObject.SetActive(false);
            _mobController.Death();
            await UniTask.Delay(TimeSpan.FromSeconds(3f));
            Release();
        }
        
        private void SetName()
        {
            _infoPanel.SetNickname($"Mobs/{(ushort)MobType}".Loc());
        }
    }
}