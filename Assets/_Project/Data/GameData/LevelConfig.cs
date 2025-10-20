using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Human;
using UnityEngine;

namespace _Project.Data.GameData
{
    [Serializable]
    public class LevelConfig
    {
        public string levelName = "Level {0}";
        public bool enabled = true;

        [Min(1)] public int width = 5;
        [Min(1)] public int height = 5;

        public List<GridCellConfig> cells = new();

        [Min(1)] public int holderCount = 5;
        [Min(1)] public int busCount = 1;
        [Min(1)] public int goldCount = 10;
        public List<HumanType> busHumanTypes = new();
    }
}
