using infrastructure.services.map;
using infrastructure.services.sounds;
using UnityEngine;
using Zenject;

namespace character
{
    public class FootstepController : MonoBehaviour
    {
        [SerializeField] private float _minInterval = 0.15f;

        private ISoundService _soundService;
        private LazyInject<IMapService> _mapService;
        private float _lastStepTime = float.MinValue;

        [Inject]
        public void Construct(ISoundService soundService, LazyInject<IMapService> mapService)
        {
            _soundService = soundService;
            _mapService = mapService;
        }

        private void Start()
        {
            var characterMovement = GetComponentInParent<CharacterMovement>();
            if (characterMovement != null)
                characterMovement.Landed += OnLanded;
        }

        // Called by Animation Event on the walk/run animation clips
        private void OnFootstep()
        {
            if (Time.time - _lastStepTime < _minInterval) return;
            _lastStepTime = Time.time;
            PlaySurfaceSound();
        }

        private void OnLanded()
        {
            _lastStepTime = Time.time;
            _soundService.Play(SoundType.Land);
            PlaySurfaceSound();
        }

        private void PlaySurfaceSound()
        {
            var footPos = transform.position - Vector3.up * 0.1f;
            var blockType = _mapService.Value.GetBlockType(footPos);
            var soundType = GetSoundType(blockType);
            if (soundType != SoundType.None)
                _soundService.Play3D(soundType, transform.position);
        }

        private static SoundType GetSoundType(BlockType blockType) => blockType switch
        {
            BlockType.Grass or BlockType.Leaves
                => SoundType.FootstepGrass,

            BlockType.Dirt or BlockType.Hay or BlockType.Wheat
                => SoundType.FootstepDirt,

            BlockType.Sand or BlockType.HalfSand or BlockType.Clay
                => SoundType.FootstepSand,

            BlockType.Snow or BlockType.SnowCarpet
                => SoundType.FootstepSnow,

            BlockType.WoodLog or BlockType.WoodPlanks or BlockType.WoodStairs
                or BlockType.HalfWoodPlanks or BlockType.WoodFence
                or BlockType.Workbench or BlockType.Furnace
                or BlockType.Bookshelf or BlockType.Door
                => SoundType.FootstepWood,

            BlockType.IronBlock or BlockType.GoldBlock
                or BlockType.DiamondsBlock or BlockType.EmeraldBlock or BlockType.Anvil
                => SoundType.FootstepMetal,
            
            BlockType.Stone or BlockType.StoneBrickWall or BlockType.StoneBrickWallStairs
                or BlockType.Cobblestone or BlockType.CoalOre or BlockType.Obsidian or BlockType.HalfStoneBrick
                or BlockType.HalfBrick or BlockType.BrickStairs or BlockType.BrickWall or BlockType.NetherBrick
                or BlockType.NetherBrickStairs or BlockType.HalfSmoothStone or BlockType.GoldOre or BlockType.IronOre
                => SoundType.FootstepStone,

            BlockType.Air
                => SoundType.None,

            _ => SoundType.FootstepGrass,
        };
    }
}
