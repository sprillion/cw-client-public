using UnityEngine;

namespace infrastructure.services.quests
{
    public class Objective
    {
        public int Id { get; set; }
        public ObjectiveType ObjectiveType { get; set; }
        public int TargetId { get; set; }
        public int TargetValue { get; set; }
        public int CurrentValue { get; set; }
        
        public bool HasPosition { get; set; }
        public Vector3 Position { get; set; }
        
        public Quest Quest { get; set; }
    }
}