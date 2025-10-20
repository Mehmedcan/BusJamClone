using System;
using _Project.Data.Constants;
using _Project.Data.GameData;
using _Project.Data.ScriptableObjects.Data;
using _Project.Scripts.Core;
using _Project.Scripts.Gameplay.Human;
using _Project.Scripts.Systems.Pooling;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Grid
{
    public class Grid : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private HumanTypeColorData humanTypeColorData;
        
        [Space] [Header("Materials")]
        public Material fixedMaterial;
        public Material inUseMaterial;
        
        public GridType Type { get; private set; }
        private HumanType _humanType;
        private Stickman.Stickman _stickmanInstance;

        private Vector2Int _coordinates;
        private MeshRenderer _meshRenderer;
        private Action<int, int> _onClick;
        
        // Data
        private static readonly Vector3 StickmanLocalPosition = new Vector3(0, 0.5f, 0);
        private static readonly Vector3 StickmanLocalRotation = new Vector3(0, 0, 0);
        private static readonly Vector3 StickmanLocalScale = new Vector3(.68f, 3.4f, .68f);

        private IPoolManager _poolManager;
        
        public void Initialize(GridCellConfig gridCellConfig, Action<int, int> onClick)
        {
            Locator.Instance.TryResolve(out _poolManager);
            
            _meshRenderer = GetComponent<MeshRenderer>();
            _coordinates = new Vector2Int(gridCellConfig.x, gridCellConfig.y);
            _onClick = onClick;
            
            _humanType = gridCellConfig.humanType;
            Type = gridCellConfig.gridType;

            SetMaterial();
            InstantiateStickman();
        }

        public void SetType(GridType type)
        {
            Type = type;
        }
        
        private void SetMaterial()
        {
            switch (Type)
            {
                case GridType.Fixed:
                    _meshRenderer.material = fixedMaterial;
                    break;
                default:
                    _meshRenderer.material = inUseMaterial;
                    break;
            }
        }
        
        private void InstantiateStickman()
        {
            if (Type != GridType.Occupied)
            {
                return;
            }

            var stickman = _poolManager.Get(DataConstants.STICKMAN_POOL_KEY, transform);
            stickman.transform.localPosition = StickmanLocalPosition;
            stickman.transform.localEulerAngles = StickmanLocalRotation;
            stickman.transform.localScale = StickmanLocalScale;
            
            _stickmanInstance = stickman.GetComponent<Stickman.Stickman>();
            var typeMaterial = humanTypeColorData.GetMaterial(_humanType);
            _stickmanInstance.Initialize(_humanType, typeMaterial);
        }
        
        private Stickman.Stickman GetStickmanInstance()
        {
            return _stickmanInstance;
        }
        
        
        // --- helpers ---
        
        public override string ToString()
        {
            return $"GridCell ({_coordinates.x},{_coordinates.y}) | Type: {Type}";
        }

        private void OnMouseDown()
        {
            _onClick?.Invoke(_coordinates.x, _coordinates.y);
        }
    }
}
