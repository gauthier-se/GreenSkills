using System;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Central manager for all gamification systems: XP, EcoCoins, daily streak.
    /// Persists data via PlayerPrefs and raises events for UI updates.
    /// Implements the Singleton pattern and persists across scene loads.
    /// </summary>
    public class GamificationManager : MonoBehaviour
    {
        public static GamificationManager Instance { get; private set; }

        #region Constants — XP Rewards

        /// <summary>XP awarded for each correct answer.</summary>
        private const int XP_PER_CORRECT_ANSWER = 10;

        /// <summary>Bonus XP awarded for completing a level.</summary>
        private const int XP_LEVEL_COMPLETION_BONUS = 50;

        /// <summary>Bonus XP awarded for a perfect level (no lives lost).</summary>
        private const int XP_PERFECT_BONUS = 30;

        /// <summary>Amount of XP required per player level.</summary>
        private const int XP_PER_LEVEL = 100;

        #endregion

        #region Constants — EcoCoin Rewards

        /// <summary>EcoCoins awarded for completing a level.</summary>
        private const int COINS_PER_LEVEL = 5;

        /// <summary>Bonus EcoCoins for a perfect level (no lives lost).</summary>
        private const int COINS_PERFECT_BONUS = 10;

        /// <summary>EcoCoins awarded per consecutive streak day.</summary>
        private const int COINS_PER_STREAK_DAY = 2;

        #endregion

        #region Constants — PlayerPrefs Keys

        private const string KEY_TOTAL_XP = "Gamification_TotalXP";
        private const string KEY_ECO_COINS = "Gamification_EcoCoins";
        private const string KEY_STREAK = "Gamification_Streak";
        private const string KEY_LAST_PLAY_DATE = "Gamification_LastPlayDate";

        #endregion

        #region Events

        /// <summary>
        /// Raised when XP is gained. Parameter is the amount gained.
        /// </summary>
        public event Action<int> OnXPGained;

        /// <summary>
        /// Raised when the player levels up. Parameter is the new player level.
        /// </summary>
        public event Action<int> OnLevelUp;

        /// <summary>
        /// Raised when EcoCoins are earned. Parameter is the amount earned.
        /// </summary>
        public event Action<int> OnCoinsEarned;

        /// <summary>
        /// Raised when the daily streak is updated. Parameter is the new streak count.
        /// </summary>
        public event Action<int> OnStreakUpdated;

        #endregion

        #region State

        private int _totalXP;
        private int _ecoCoins;
        private int _streak;
        private string _lastPlayDate;

        #endregion

        #region Public Properties

        /// <summary>Total accumulated XP across all play sessions.</summary>
        public int TotalXP => _totalXP;

        /// <summary>Current player level, derived from total XP.</summary>
        public int PlayerLevel => 1 + _totalXP / XP_PER_LEVEL;

        /// <summary>XP progress within the current level (0 to XP_PER_LEVEL - 1).</summary>
        public int XPInCurrentLevel => _totalXP % XP_PER_LEVEL;

        /// <summary>XP required to complete the current level.</summary>
        public int XPRequiredForLevel => XP_PER_LEVEL;

        /// <summary>Current EcoCoin balance.</summary>
        public int EcoCoins => _ecoCoins;

        /// <summary>Current consecutive daily play streak.</summary>
        public int Streak => _streak;

        /// <summary>
        /// Multiplier applied to XP gains based on streak.
        /// 1.0x base + 0.1x per streak day, capped at 2.0x.
        /// </summary>
        public float StreakMultiplier => Mathf.Min(1f + _streak * 0.1f, 2f);

        #endregion

        #region Singleton Setup

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                if (transform.parent != null)
                {
                    transform.SetParent(null);
                    Debug.LogWarning("[GamificationManager] Was not at root level. Moved automatically.");
                }

                DontDestroyOnLoad(gameObject);

                LoadFromPrefs();
                CheckDailyStreak();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Public API — XP

        /// <summary>
        /// Awards XP for a correct answer, applying the streak multiplier.
        /// </summary>
        public void AwardCorrectAnswerXP()
        {
            int baseXP = XP_PER_CORRECT_ANSWER;
            int xpGained = Mathf.RoundToInt(baseXP * StreakMultiplier);

            AddXP(xpGained);

            Debug.Log($"[GamificationManager] Correct answer: +{xpGained} XP (x{StreakMultiplier:F1} streak)");
        }

        /// <summary>
        /// Awards XP and EcoCoins for completing a level.
        /// Call this from GameManager.WinLevel().
        /// </summary>
        /// <param name="remainingLives">Lives remaining at level end.</param>
        /// <param name="maxLives">Maximum lives the player started with.</param>
        /// <returns>A LevelRewards struct with the breakdown of all rewards earned.</returns>
        public LevelRewards AwardLevelCompletion(int remainingLives, int maxLives)
        {
            bool isPerfect = remainingLives >= maxLives;
            int previousLevel = PlayerLevel;

            // XP calculation
            int baseXP = XP_LEVEL_COMPLETION_BONUS;
            int perfectXP = isPerfect ? XP_PERFECT_BONUS : 0;
            int totalBaseXP = baseXP + perfectXP;
            int totalXP = Mathf.RoundToInt(totalBaseXP * StreakMultiplier);

            // Coin calculation
            int baseCoins = COINS_PER_LEVEL;
            int perfectCoins = isPerfect ? COINS_PERFECT_BONUS : 0;
            int streakCoins = _streak > 0 ? COINS_PER_STREAK_DAY * _streak : 0;
            int totalCoins = baseCoins + perfectCoins + streakCoins;

            // Apply rewards
            AddXP(totalXP);
            AddCoins(totalCoins);

            // Update streak date (player played today)
            UpdateLastPlayDate();

            bool didLevelUp = PlayerLevel > previousLevel;

            var rewards = new LevelRewards
            {
                xpEarned = totalXP,
                coinsEarned = totalCoins,
                isPerfect = isPerfect,
                didLevelUp = didLevelUp,
                newPlayerLevel = PlayerLevel
            };

            Debug.Log($"[GamificationManager] Level complete: +{totalXP} XP, +{totalCoins} coins" +
                      $"{(isPerfect ? " (PERFECT)" : "")}{(didLevelUp ? $" LEVEL UP -> {PlayerLevel}" : "")}");

            return rewards;
        }

        #endregion

        #region Public API — EcoCoins

        /// <summary>
        /// Spends EcoCoins if the player has enough.
        /// </summary>
        /// <param name="amount">Number of coins to spend.</param>
        /// <returns>True if the purchase succeeded, false if insufficient funds.</returns>
        public bool SpendCoins(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning("[GamificationManager] SpendCoins called with non-positive amount.");
                return false;
            }

            if (_ecoCoins < amount)
            {
                Debug.Log($"[GamificationManager] Not enough coins: have {_ecoCoins}, need {amount}");
                return false;
            }

            _ecoCoins -= amount;
            SaveToPrefs();

            Debug.Log($"[GamificationManager] Spent {amount} coins. Balance: {_ecoCoins}");
            return true;
        }

        #endregion

        #region Public API — Streak

        /// <summary>
        /// Gets a formatted display string for the streak (e.g. "5 jours").
        /// </summary>
        public string GetStreakDisplayText()
        {
            if (_streak <= 0) return "0 jour";
            return _streak == 1 ? "1 jour" : $"{_streak} jours";
        }

        #endregion

        #region Public API — Reset

        /// <summary>
        /// Resets all gamification data. Used when resetting progress.
        /// </summary>
        public void ResetAll()
        {
            _totalXP = 0;
            _ecoCoins = 0;
            _streak = 0;
            _lastPlayDate = "";

            PlayerPrefs.DeleteKey(KEY_TOTAL_XP);
            PlayerPrefs.DeleteKey(KEY_ECO_COINS);
            PlayerPrefs.DeleteKey(KEY_STREAK);
            PlayerPrefs.DeleteKey(KEY_LAST_PLAY_DATE);
            PlayerPrefs.Save();

            Debug.Log("[GamificationManager] All gamification data reset.");
        }

        #endregion

        #region Internal — XP & Coins Helpers

        /// <summary>
        /// Adds XP, checks for level-up, and saves.
        /// </summary>
        private void AddXP(int amount)
        {
            int previousLevel = PlayerLevel;

            _totalXP += amount;
            SaveToPrefs();

            OnXPGained?.Invoke(amount);

            if (PlayerLevel > previousLevel)
            {
                OnLevelUp?.Invoke(PlayerLevel);
                Debug.Log($"[GamificationManager] Level up! Now level {PlayerLevel}");
            }
        }

        /// <summary>
        /// Adds EcoCoins and saves.
        /// </summary>
        private void AddCoins(int amount)
        {
            _ecoCoins += amount;
            SaveToPrefs();

            OnCoinsEarned?.Invoke(amount);
        }

        #endregion

        #region Internal — Daily Streak

        /// <summary>
        /// Checks the daily streak on startup.
        /// If the player played yesterday, increments the streak.
        /// If they played today, keeps the streak.
        /// Otherwise, resets to 0.
        /// </summary>
        private void CheckDailyStreak()
        {
            if (string.IsNullOrEmpty(_lastPlayDate))
            {
                Debug.Log("[GamificationManager] No previous play date. Streak starts at 0.");
                return;
            }

            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

            if (_lastPlayDate == today)
            {
                // Already played today, keep streak as-is
                Debug.Log($"[GamificationManager] Already played today. Streak: {_streak}");
            }
            else if (_lastPlayDate == yesterday)
            {
                // Played yesterday — increment streak
                _streak++;
                _lastPlayDate = today;
                SaveToPrefs();

                Debug.Log($"[GamificationManager] Streak continued! Day {_streak}");
                OnStreakUpdated?.Invoke(_streak);
            }
            else
            {
                // Missed a day — reset streak
                int oldStreak = _streak;
                _streak = 0;
                SaveToPrefs();

                Debug.Log($"[GamificationManager] Streak broken (last play: {_lastPlayDate}). Reset from {oldStreak} to 0.");
                OnStreakUpdated?.Invoke(_streak);
            }
        }

        /// <summary>
        /// Updates the last play date to today. Called when the player completes a level.
        /// </summary>
        private void UpdateLastPlayDate()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            if (_lastPlayDate != today)
            {
                // First completion today — if we haven't already incremented in CheckDailyStreak
                if (string.IsNullOrEmpty(_lastPlayDate))
                {
                    _streak = 1;
                }
                else
                {
                    string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                    if (_lastPlayDate != yesterday)
                    {
                        // Gap in play — streak was already reset by CheckDailyStreak,
                        // but start a new streak of 1
                        _streak = 1;
                    }
                    // If lastPlayDate == yesterday, streak was already incremented in CheckDailyStreak
                }

                _lastPlayDate = today;
                SaveToPrefs();

                OnStreakUpdated?.Invoke(_streak);
                Debug.Log($"[GamificationManager] Play date updated to {today}. Streak: {_streak}");
            }
        }

        #endregion

        #region Persistence

        /// <summary>
        /// Loads all gamification data from PlayerPrefs.
        /// </summary>
        private void LoadFromPrefs()
        {
            _totalXP = PlayerPrefs.GetInt(KEY_TOTAL_XP, 0);
            _ecoCoins = PlayerPrefs.GetInt(KEY_ECO_COINS, 0);
            _streak = PlayerPrefs.GetInt(KEY_STREAK, 0);
            _lastPlayDate = PlayerPrefs.GetString(KEY_LAST_PLAY_DATE, "");

            Debug.Log($"[GamificationManager] Loaded — XP: {_totalXP}, Level: {PlayerLevel}, " +
                      $"Coins: {_ecoCoins}, Streak: {_streak}, Last play: {_lastPlayDate}");
        }

        /// <summary>
        /// Saves all gamification data to PlayerPrefs.
        /// </summary>
        private void SaveToPrefs()
        {
            PlayerPrefs.SetInt(KEY_TOTAL_XP, _totalXP);
            PlayerPrefs.SetInt(KEY_ECO_COINS, _ecoCoins);
            PlayerPrefs.SetInt(KEY_STREAK, _streak);
            PlayerPrefs.SetString(KEY_LAST_PLAY_DATE, _lastPlayDate);
            PlayerPrefs.Save();
        }

        #endregion
    }

    /// <summary>
    /// Data struct returned by AwardLevelCompletion with the full reward breakdown.
    /// </summary>
    [System.Serializable]
    public struct LevelRewards
    {
        /// <summary>Total XP earned from this level completion.</summary>
        public int xpEarned;

        /// <summary>Total EcoCoins earned from this level completion.</summary>
        public int coinsEarned;

        /// <summary>Whether the player completed the level without losing any lives.</summary>
        public bool isPerfect;

        /// <summary>Whether the player leveled up from this level's XP.</summary>
        public bool didLevelUp;

        /// <summary>The player's new level (relevant if didLevelUp is true).</summary>
        public int newPlayerLevel;
    }
}
