using System;
using _Project.Scripts.Gameplay.Grid;
using _Project.Scripts.Gameplay.Human;

namespace _Project.Data.GameData
{
    [Serializable]
    public class GridCellConfig
    {
        public int x;
        public int y;
        public GridType gridType = GridType.Fixed;
        public HumanType humanType;
    }
}
