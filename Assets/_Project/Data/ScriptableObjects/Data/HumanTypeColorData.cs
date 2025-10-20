using System.Collections.Generic;
using _Project.Scripts.Gameplay.Human;
using UnityEngine;

namespace _Project.Data.ScriptableObjects.Data
{
    [CreateAssetMenu(fileName = "HumanTypeColorData", menuName = "Game/Configs/HumanType Color Data", order = 1)]
    public class HumanTypeColorData : ScriptableObject
    {
        [System.Serializable]
        public class HumanMaterialPair
        {
            public HumanType humanType;
            public Material material;
        }

        [Header("HumanType -> Material Map")]
        [SerializeField] private List<HumanMaterialPair> _materials = new();

        private Dictionary<HumanType, Material> _lookup;
        
        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<HumanType, Material>();
            foreach (var entry in _materials)
            {
                if (!_lookup.ContainsKey(entry.humanType))
                {
                    _lookup.Add(entry.humanType, entry.material);
                }
            }
        }
        
        public Material GetMaterial(HumanType type)
        {
            if (_lookup == null || _lookup.Count == 0)
            {
                BuildLookup();
            }

            if (_lookup != null && _lookup.TryGetValue(type, out var mat))
            {
                return mat;
            }

            Debug.LogError($"[BusColorData] Material not found for HumanType: {type}");
            return null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            BuildLookup();
        }
#endif
    }
}