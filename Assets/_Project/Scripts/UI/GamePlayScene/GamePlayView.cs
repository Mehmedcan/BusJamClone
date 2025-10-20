using _Project.Data.GameData;
using UnityEngine;

namespace _Project.Scripts.UI.GamePlayScene
{
    public class GamePlayView : MonoBehaviour
    {
        [SerializeField] private GameObject inGameScreen;
        [SerializeField] private WinScreen winScreen;
        [SerializeField] private LoseScreen loseScreen;

        private void Awake()
        {
            inGameScreen.SetActive(true);
            winScreen.gameObject.SetActive(false);
            loseScreen.gameObject.SetActive(false);
        }
        
        public void ShowWinScreen(UserConfig userConfig, int earnedGold)
        {
            winScreen.Initialize(userConfig, earnedGold);
            
            inGameScreen.SetActive(false);
            winScreen.gameObject.SetActive(true);
            loseScreen.gameObject.SetActive(false);
        }
        
        public void ShowLoseScreen(UserConfig userConfig)
        {
            loseScreen.Initialize(userConfig);
            
            inGameScreen.SetActive(false);
            winScreen.gameObject.SetActive(false);
            loseScreen.gameObject.SetActive(true);
        }
    }
}
