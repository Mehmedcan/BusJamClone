using System.Collections.Generic;
using _Project.Data.ScriptableObjects.Data;
using _Project.Scripts.Gameplay.Human;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Bus
{
    public class Bus : MonoBehaviour
    {
        [Header("Bus Related")]
        [SerializeField] private HumanTypeColorData humanTypeColorData;
        [SerializeField] private MeshRenderer bodyMeshRenderer;

        [Space] [Header("Stickman Related")] 
        [SerializeField] private List<Stickman.Stickman> sittingStickmans;

        private int _stickmanCount;
        private HumanType _busType;
        
        private const int MaxStickmanCount = 3;

        public void Initialize(HumanType busType)
        {
            _busType = busType;
            
            ApplyBusColor();
            ApplyStickManColor();
            ResetStickmans();
        }

        private void ApplyBusColor()
        {
            var typeMaterial = humanTypeColorData.GetMaterial(_busType);
            if (typeMaterial != null)
            {
                var meshMaterials = bodyMeshRenderer.materials;
                meshMaterials[1] = typeMaterial; // the second material is the bus body
                bodyMeshRenderer.materials = meshMaterials;
            }
        }
        
        private void ApplyStickManColor()
        {
            var typeMaterial = humanTypeColorData.GetMaterial(_busType);
            if (typeMaterial != null)
            {
                foreach (var stickman in sittingStickmans)
                {
                    stickman.Initialize(_busType, typeMaterial);
                }
            }
        }
        
        private void ResetStickmans()
        {
            _stickmanCount = 0;
            
            foreach (var stickman in sittingStickmans)
            {
                stickman.gameObject.SetActive(false);
            }
        }
        
        private void EnableNextStickman()
        {
            if (_stickmanCount < MaxStickmanCount)
            {
                sittingStickmans[_stickmanCount].gameObject.SetActive(true);
                _stickmanCount++;
            }
        }
    }
}
