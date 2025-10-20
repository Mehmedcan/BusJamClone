using System;
using _Project.Scripts.Gameplay.Human;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Holder
{
    public class Holder : MonoBehaviour
    {
        public bool IsOccupied { get; private set; }

        private HumanType _humanType;
        private int _order;
        private Action<int> _onClick;
        
        public void Initialize(int order, Action<int> onClick)
        {
            _order = order;
            _onClick = onClick;
        }
        
        public void Occupy(HumanType humanType)
        {
            IsOccupied = true;
            _humanType = humanType;
        }
        
        public void Vacate()
        {
            IsOccupied = false;
            _humanType = default;
        }
        
        public override string ToString()
        {
            return $"Holder {_order} | Occupied: {IsOccupied} | HumanType: {_humanType}";
        }
        
        private void OnMouseDown()
        {
            _onClick?.Invoke(_order);
        }
    }
}
