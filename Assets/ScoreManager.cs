using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public TMP_Text scoreText;
    int score = 0;

    void Awake()
    {
        Instance = this;
        UpdateUI();
    }

    public static void Add(int amount)
    {
        if (Instance == null) return;
        Instance.score += amount;
        Instance.UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }
}
