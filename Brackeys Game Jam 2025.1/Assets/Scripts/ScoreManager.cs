using UnityEngine;
using TMPro; // For UI Text

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance; // Singleton pattern

    private int _score = 0;
    public TextMeshProUGUI scoreText; // Assign in Inspector
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddScore(int points)
    {
        _score += points;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText) scoreText.text = GetScore().ToString();
    }

    public int GetScore()
    {
        if (_score < 0) _score = 0;
        return _score;
    }
}