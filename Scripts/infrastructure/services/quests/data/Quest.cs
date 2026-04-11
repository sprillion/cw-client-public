using System.Collections.Generic;
using System.Linq;
using infrastructure.services.npc;
using UnityEngine;

namespace infrastructure.services.quests
{
    public class Quest
    {
        public int Id { get; set; }
        public NpcType NpcType { get; set; }
        public bool HasNpcPosition { get; set; }
        public Vector3 NpcPosition { get; set; }
        
        public bool IsLoaded { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsCompleted { get; set; }


        public List<Objective> Objectives { get; set; } = new List<Objective>();
        public List<Reward> Rewards { get; set; } = new List<Reward>();

        public bool IsProgressCompleted => Objectives.All(o => o.CurrentValue >= o.TargetValue);
    }
}