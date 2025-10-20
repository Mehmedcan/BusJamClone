using System.Collections.Generic;
using _Project.Scripts.Gameplay.Human;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Holder
{
    public class HolderController : MonoBehaviour
    {
        [Header("Holder Related")]
        [SerializeField] private Transform firstHolderTransform;
        [SerializeField] private Holder holderPrefab;
        
        [Space] [Header("Layout Settings")]
        public float padding;
        
        private List<Holder> _holders = new();
        private int _currentEmptyIndex = 0;
  
        
        public void CreateHolders(int holderCount)
        {
            _holders ??= new List<Holder>(holderCount);
            _currentEmptyIndex = 0;

            for (var i = 0; i < holderCount; i++)
            {
                var position = new Vector3(
                    firstHolderTransform.position.x + i * (holderPrefab.transform.localScale.x + padding),
                    firstHolderTransform.position.y,
                    firstHolderTransform.position.z
                );

                var newHolder = Instantiate(holderPrefab, position, Quaternion.identity, transform);
                newHolder.Initialize(i, OnHolderClicked);

                newHolder.name = $"Holder {i}";
                _holders.Add(newHolder);
            }
        }
        
        public bool FillNextEmptyHolder(HumanType humanType)
        {
            if (_currentEmptyIndex >= _holders.Count)
            {
                Debug.Log("All holders are full!");
                return false;
            }
            
            var holder = _holders[_currentEmptyIndex];
            holder.Occupy(humanType);
            
            Debug.Log($"Filled holder {_currentEmptyIndex} with {humanType}");
            
            _currentEmptyIndex++;
            return true;
        }
        
        public bool AreAllHoldersFull()
        {
            return _currentEmptyIndex >= _holders.Count;
        }
        
        public int GetEmptyHolderCount()
        {
            return Mathf.Max(0, _holders.Count - _currentEmptyIndex);
        }
        
        private void OnHolderClicked(int index)
        {
            Debug.Log($"Holder clicked at index {index}");
        }
    }
}
