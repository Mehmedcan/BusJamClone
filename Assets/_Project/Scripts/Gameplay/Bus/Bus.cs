using System.Collections.Generic;
using _Project.Data.Constants;
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
        private int _pendingStickmanCount;
        private HumanType _busType;

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
            _pendingStickmanCount = 0;
            
            foreach (var stickman in sittingStickmans)
            {
                stickman.gameObject.SetActive(false);
            }
        }
        
        public void EnableNextStickman()
        {
            if (_stickmanCount < GameConstants.BUS_MAX_STICMAN_COUNT)
            {
                sittingStickmans[_stickmanCount].gameObject.SetActive(true);
                _stickmanCount++;
            }
        }
        
        public bool IsFull()
        {
            return _stickmanCount >= GameConstants.BUS_MAX_STICMAN_COUNT;
        }
        
        public bool CanAcceptStickman()
        {
            return (_stickmanCount + _pendingStickmanCount) < GameConstants.BUS_MAX_STICMAN_COUNT;
        }
        
        public void ReserveStickmanSlot()
        {
            _pendingStickmanCount++;
        }
        
        public void ConfirmStickmanArrival()
        {
            if (_pendingStickmanCount > 0)
            {
                _pendingStickmanCount--;
            }
        }
        
        public int GetCurrentStickmanCount()
        {
            return _stickmanCount;
        }
        
        public int GetPendingStickmanCount()
        {
            return _pendingStickmanCount;
        }
        
        public HumanType GetBusType()
        {
            return _busType;
        }
    }
}
