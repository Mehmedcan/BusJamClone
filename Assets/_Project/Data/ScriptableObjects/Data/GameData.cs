using System.Collections.Generic;
using _Project.Data.GameData;
using UnityEngine;

namespace _Project.Data.ScriptableObjects.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Configs/GameConfig", order = 0)]
    public class GameData : ScriptableObject
    {
        [Tooltip("Seconds required to refill")]
        [Min(1f)] public float refillSeconds = 300f;

        public List<LevelConfig> levels = new();
    }
}