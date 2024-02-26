using TMPro;
using UnityEngine;

namespace _Scripts
{
    public class ScoreUpdater : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI coinsText;

        public void UpdateScore(int score)
        {
            scoreText.text = score.ToString();
        }

        public void UpdateCoins(int score)
        {
            coinsText.text = score.ToString();
        }
    }
}