using I2.Loc;
using infrastructure.services.map;
using infrastructure.services.players;
using infrastructure.services.quests;
using TMPro;
using UnityEngine;
using Zenject;

namespace ui.waypoint
{
    public class QuestObjectiveIndicator : MonoBehaviour
    {
        [SerializeField] private RectTransform _indicatorRoot;
        [SerializeField] private TMP_Text _distanceText;
        [SerializeField] private GameObject _objectiveInfoRoot;
        [SerializeField] private TMP_Text _objectiveText;
        [SerializeField] private TMP_Text _countText;

        [SerializeField] private float _edgePadding = 50f;

        private IQuestMarkerService _questMarkerService;
        private ICharacterService _characterService;
        private IMapService _mapService;
        private Camera _camera;

        private float? _surfaceY;

        [Inject]
        public void Construct(IQuestMarkerService questMarkerService, ICharacterService characterService,
            IMapService mapService)
        {
            _questMarkerService = questMarkerService;
            _characterService = characterService;
            _mapService = mapService;
        }

        private void Start()
        {
            _camera = Camera.main;
            _questMarkerService.OnMarkedObjectiveChanged += OnMarkedObjectiveChanged;
            _mapService.ChunksUpdated += OnChunksUpdated;
            OnMarkedObjectiveChanged(_questMarkerService.MarkedObjective);
        }

        private void OnDestroy()
        {
            _questMarkerService.OnMarkedObjectiveChanged -= OnMarkedObjectiveChanged;
            _mapService.ChunksUpdated -= OnChunksUpdated;
        }

        private void OnMarkedObjectiveChanged(Objective objective)
        {
            if (objective != null && objective.HasPosition)
                _surfaceY = _mapService.GetSurfaceHeight(objective.Position.x, objective.Position.z);
            else
                _surfaceY = null;
        }

        private void OnChunksUpdated()
        {
            var obj = _questMarkerService.MarkedObjective;
            if (obj != null && obj.HasPosition)
                _surfaceY = _mapService.GetSurfaceHeight(obj.Position.x, obj.Position.z);
        }

        private void Update()
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return;

            var objective = _questMarkerService.MarkedObjective;
            bool hasObjective = objective != null && objective.HasPosition && _characterService.CurrentCharacter != null;
            _objectiveInfoRoot.SetActive(hasObjective);
            if (!hasObjective)
            {
                _indicatorRoot.gameObject.SetActive(false);
                return;
            }

            _indicatorRoot.gameObject.SetActive(true);

            _objectiveText.text = LocalizationManager.GetTranslation($"Quest/Objective/{objective.ObjectiveType}");
            _countText.text = $"{objective.CurrentValue}/{objective.TargetValue}";

            var playerPos = _characterService.CurrentCharacter.transform.position;
            float distance = new Vector2(objective.Position.x - playerPos.x, objective.Position.z - playerPos.z).magnitude;
            _distanceText.text = $"{Mathf.RoundToInt(distance)}m";

            float waypointY = _surfaceY ?? objective.Position.y;
            var waypointPos = new Vector3(objective.Position.x, waypointY, objective.Position.z);

            var screenPos3D = _camera.WorldToScreenPoint(waypointPos);
            bool isBehind = screenPos3D.z < 0f;
            if (isBehind)
                screenPos3D = -screenPos3D;

            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            var screenPos2D = new Vector2(screenPos3D.x, screenPos3D.y);

            bool isOnScreen = !isBehind
                && screenPos3D.x >= _edgePadding && screenPos3D.x <= Screen.width - _edgePadding
                && screenPos3D.y >= _edgePadding && screenPos3D.y <= Screen.height - _edgePadding;

            Vector2 dir = screenPos2D - center;
            if (dir.sqrMagnitude < 0.001f) dir = Vector2.up;
            dir.Normalize();

            Vector2 indicatorPos;
            if (isOnScreen)
            {
                indicatorPos = screenPos2D;
            }
            else
            {
                float halfW = center.x - _edgePadding;
                float halfH = center.y - _edgePadding;
                float tx = dir.x != 0 ? halfW / Mathf.Abs(dir.x) : float.MaxValue;
                float ty = dir.y != 0 ? halfH / Mathf.Abs(dir.y) : float.MaxValue;
                float t = Mathf.Min(tx, ty);
                indicatorPos = center + dir * t;
            }

            _indicatorRoot.position = indicatorPos;
        }
    }
}
