using System;
using System.Collections.Generic;
using infrastructure.services.mobs;
using infrastructure.services.npc;
using ui.map;
using UnityEngine;

namespace infrastructure.services.mapMarkers
{
    public interface IMapMarkerService
    {
        MapIconData GetIconData(MapIconType type);
        Sprite GetMobSprite(MobType mobType);
        Sprite GetNpcSprite(NpcType npcType);

        IReadOnlyList<NpcMapPoint> NpcConfigPoints { get; }
        IReadOnlyList<Vector3> MineConfigPoints { get; }
        IReadOnlyList<Vector3> LumberAreaPoints { get; }
        IReadOnlyList<Vector3> HouseConfigPoints { get; }

        bool IsTypeVisible(MapIconType type);
        void SetTypeVisible(MapIconType type, bool visible);
        event Action<MapIconType, bool> OnVisibilityChanged;
    }
}
