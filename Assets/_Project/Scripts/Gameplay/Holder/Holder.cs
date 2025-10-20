using _Project.Scripts.Gameplay.Human;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Holder
{
    public class Holder : MonoBehaviour
    {
        public bool IsOccupied { get; private set; }

        private HumanType _humanType;
        private int _order;
        private Stickman.Stickman _stickmanInstance;
        
        public void Initialize(int order)
        {
            _order = order;
        }

        public void OccupyWithStickman(HumanType humanType, Stickman.Stickman stickman)
        {
            IsOccupied = true;
            _humanType = humanType;
            _stickmanInstance = stickman;
        }
        
        public void Vacate()
        {
            IsOccupied = false;
            _humanType = default;
            _stickmanInstance = null;
        }
        
        public HumanType GetHumanType()
        {
            return _humanType;
        }
        
        public Stickman.Stickman GetStickmanInstance()
        {
            return _stickmanInstance;
        }
        
        public override string ToString()
        {
            return $"Holder {_order} | Occupied: {IsOccupied} | HumanType: {_humanType}";
        }
    }
}
