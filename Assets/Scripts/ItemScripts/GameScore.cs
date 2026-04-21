using UnityEngine;
using UnityEngine.Events;

// Simple static score tracker. Call GameScore.Add() from anywhere.
// Attach this to a persistent GameObject in your scene (e.g. GameManager).

// score should be tracked by host for multiplayer

public class GameScore : MonoBehaviour
{
    public static float CurrentScore { get; private set; }

    [Header("Events")]
    [Tooltip("Fired whenever the score changes. Passes the new total score.")]
    public UnityEvent<float> onScoreChanged;

    private static GameScore instance;

    private void Awake()
    {
        instance = this;
        CurrentScore = 0f;
    }

    // Add value to the score. Call from anywhere.</summary>
    public static void Add(float amount)
    {
        // Only host should modify score
        CurrentScore += amount;
        Debug.Log($"[GameScore] Score: {CurrentScore}");
        instance?.onScoreChanged?.Invoke(CurrentScore);
    }

    public static void Reset()
    {
        CurrentScore = 0f;
        instance?.onScoreChanged?.Invoke(CurrentScore);
    }
}