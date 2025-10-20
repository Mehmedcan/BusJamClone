using System;
using UnityEngine;

namespace _Project.Scripts.UI.GamePlayScene
{
    public class GamePlayView : MonoBehaviour
    {
        [SerializeField] private GameObject inGameScreen;
        [SerializeField] private GameObject winScreen;
        [SerializeField] private GameObject loseScreen;

        private void Awake()
        {
            inGameScreen.SetActive(true);
            winScreen.SetActive(false);
            loseScreen.SetActive(false);
        }
    }
}
