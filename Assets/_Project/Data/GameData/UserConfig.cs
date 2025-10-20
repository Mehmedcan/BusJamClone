using System;

namespace _Project.Data.GameData
{
    public struct UserConfig
    {
        public int gold;
        public int level;
        public int lifeCount;
        public DateTime lastFailTime;

        public UserConfig( int gold, int level, int lifeCount)
        {
            this.gold = gold;
            this.level = level;
            this.lifeCount = lifeCount;
            lastFailTime = DateTime.MinValue;
        }
    }
}
