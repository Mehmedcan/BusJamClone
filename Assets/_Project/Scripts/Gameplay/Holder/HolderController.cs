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
        
        
        public void CreateHolders(int holderCount)
        {
            _holders ??= new List<Holder>(holderCount);

            for (var i = 0; i < holderCount; i++)
            {
                var position = new Vector3(
                    firstHolderTransform.position.x + i * (holderPrefab.transform.localScale.x + padding),
                    firstHolderTransform.position.y,
                    firstHolderTransform.position.z
                );

                var newHolder = Instantiate(holderPrefab, position, Quaternion.identity, transform);
                newHolder.Initialize(i);

                newHolder.name = $"Holder {i}";
                _holders.Add(newHolder);
            }
        }
        
        public Holder GetAndFillNextEmptyHolder(HumanType humanType, Stickman.Stickman stickman)
        {
            for (var i = 0; i < _holders.Count; i++)
            {
                if (!_holders[i].IsOccupied)
                {
                    var holder = _holders[i];
                    holder.OccupyWithStickman(humanType, stickman);
                    
                    return holder;
                }
            }
            
            return null;
        }
        
        public List<Holder> GetHoldersWithHumanType(HumanType humanType)
        {
            var matchingHolders = new List<Holder>();
            
            foreach (var holder in _holders)
            {
                if (holder.IsOccupied && holder.GetHumanType() == humanType)
                {
                    matchingHolders.Add(holder);
                }
            }
            
            return matchingHolders;
        }
        
        public bool AreAllHoldersFull()
        {
            foreach (var holder in _holders)
            {
                if (!holder.IsOccupied)
                {
                    return false;
                }
            }
            return true;
        }
        
        public bool HasHolderWithHumanType(HumanType humanType)
        {
            foreach (var holder in _holders)
            {
                if (holder.IsOccupied && holder.GetHumanType() == humanType)
                {
                    return true;
                }
            }
            return false;
        }
        
        public int GetEmptyHolderCount()
        {
            var count = 0;
            foreach (var holder in _holders)
            {
                if (!holder.IsOccupied)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
