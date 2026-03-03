using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShouldYouShoot.Data;

namespace ShouldYouShoot.Core
{
    /// <summary>
    /// Central game state machine. Controls the flow from mode selection through
    /// scenario presentation, player decision, and outcome display.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────────
        public static GameManager Instance { get; private set; }

        // ── Inspector References ───────────────────────────────────────────────
        [Header("Scene Controllers")]
        [SerializeField] private AR.ARModeController arModeController;
        [SerializeField] private VR.VRModeController vrModeController;
        [SerializeField] private UI.UIManager uiManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private Mechanics.ShootingMechanic shootingMechanic;

        [Header("Game Data")]
        [Tooltip("Name of the JSON file inside Resources/ (without extension).")]
        [SerializeField] private string gameDataFileName = "historical_characters";

        [Header("Timing")]
        [SerializeField] private float outcomeDisplayDuration = 5f;

        // ── State ──────────────────────────────────────────────────────────────
        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public GameMode CurrentMode { get; private set; } = GameMode.None;

        private GameDataCollection _gameData;
        private List<MoralDilemma> _dilemmaQueue;
        private int _currentDilemmaIndex;
        private MoralDilemma _activeDilemma;
        private HistoricalCharacter _activeCharacter;
        private bool _hasReadAllHints;
        private bool _hasReadAnyHint;
        private float _decisionStartTime;
        private Coroutine _activeDecisionTimer;

        // ── Events ─────────────────────────────────────────────────────────────
        public event Action<GameState> OnStateChanged;
        public event Action<MoralDilemma, HistoricalCharacter> OnDilemmaStarted;
        public event Action<bool, HistoricalCharacter> OnDecisionMade;   // (didShoot, character)
        public event Action OnAllDilemmasCompleted;

        // ── Unity Lifecycle ────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadGameData();
        }

        private void Start()
        {
            TransitionTo(GameState.MainMenu);
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Start the game in AR mode.</summary>
        public void StartARMode()
        {
            CurrentMode = GameMode.AR;
            BeginGame();
        }

        /// <summary>Start the game in VR mode.</summary>
        public void StartVRMode()
        {
            CurrentMode = GameMode.VR;
            BeginGame();
        }

        /// <summary>
        /// Called when the player fires their weapon at a character.
        /// </summary>
        public void RegisterShot()
        {
            if (CurrentState != GameState.DilemmaActive) return;
            ProcessDecision(didShoot: true);
        }

        /// <summary>
        /// Called when the player explicitly chooses to spare the character.
        /// </summary>
        public void RegisterSpare()
        {
            if (CurrentState != GameState.DilemmaActive) return;
            ProcessDecision(didShoot: false);
        }

        /// <summary>
        /// Called by the UI when the player has finished reading a hint.
        /// </summary>
        public void RegisterHintRead(int hintIndex)
        {
            if (_activeDilemma == null) return;
            _hasReadAnyHint = true;
            if (hintIndex >= _activeDilemma.ProgressiveHints.Count - 1)
                _hasReadAllHints = true;
        }

        /// <summary>Restart from the main menu.</summary>
        public void ReturnToMainMenu()
        {
            scoreManager.ResetScore();
            TransitionTo(GameState.MainMenu);
        }

        // ── Private Logic ──────────────────────────────────────────────────────

        private void LoadGameData()
        {
            TextAsset jsonAsset = Resources.Load<TextAsset>(gameDataFileName);
            if (jsonAsset == null)
            {
                Debug.LogError($"[GameManager] Could not load game data from Resources/{gameDataFileName}.json");
                return;
            }
            _gameData = JsonUtility.FromJson<GameDataCollection>(jsonAsset.text);
            Debug.Log($"[GameManager] Loaded {_gameData.Characters.Count} characters and {_gameData.Dilemmas.Count} dilemmas.");
        }

        private void BeginGame()
        {
            _dilemmaQueue = new List<MoralDilemma>(_gameData.Dilemmas);
            ShuffleDilemmas(_dilemmaQueue);
            _currentDilemmaIndex = 0;
            scoreManager.ResetScore();

            ActivateMode();
            TransitionTo(GameState.Loading);
            StartCoroutine(LoadFirstDilemma());
        }

        private void ActivateMode()
        {
            if (arModeController != null) arModeController.gameObject.SetActive(CurrentMode == GameMode.AR);
            if (vrModeController != null) vrModeController.gameObject.SetActive(CurrentMode == GameMode.VR);
        }

        private IEnumerator LoadFirstDilemma()
        {
            yield return new WaitForSeconds(1f);
            PresentNextDilemma();
        }

        private void PresentNextDilemma()
        {
            if (_currentDilemmaIndex >= _dilemmaQueue.Count)
            {
                TransitionTo(GameState.GameOver);
                OnAllDilemmasCompleted?.Invoke();
                return;
            }

            _activeDilemma = _dilemmaQueue[_currentDilemmaIndex];
            _activeCharacter = FindCharacter(_activeDilemma.CharacterId);
            _hasReadAllHints = false;
            _hasReadAnyHint = false;
            _decisionStartTime = Time.time;

            if (_activeCharacter == null)
            {
                Debug.LogWarning($"[GameManager] Character '{_activeDilemma.CharacterId}' not found, skipping.");
                _currentDilemmaIndex++;
                PresentNextDilemma();
                return;
            }

            SpawnCharacter(_activeCharacter);
            TransitionTo(GameState.DilemmaActive);
            OnDilemmaStarted?.Invoke(_activeDilemma, _activeCharacter);
            shootingMechanic.EnableShooting(true);

            if (_activeDilemma.DecisionTimeLimit > 0)
                _activeDecisionTimer = StartCoroutine(DecisionTimer(_activeDilemma.DecisionTimeLimit));
        }

        private void SpawnCharacter(HistoricalCharacter character)
        {
            if (CurrentMode == GameMode.AR && arModeController != null)
                arModeController.SpawnCharacter(character);
            else if (CurrentMode == GameMode.VR && vrModeController != null)
                vrModeController.SpawnCharacter(character);
        }

        private void ProcessDecision(bool didShoot)
        {
            shootingMechanic.EnableShooting(false);

            if (_activeDecisionTimer != null)
            {
                StopCoroutine(_activeDecisionTimer);
                _activeDecisionTimer = null;
            }

            float decisionTime = Time.time - _decisionStartTime;
            scoreManager.RecordDecision(
                _activeCharacter,
                _activeDilemma,
                didShoot,
                _hasReadAllHints,
                _hasReadAnyHint,
                decisionTime);

            TransitionTo(GameState.ShowingOutcome);
            OnDecisionMade?.Invoke(didShoot, _activeCharacter);

            StartCoroutine(ShowOutcomeThenAdvance(didShoot));
        }

        private IEnumerator ShowOutcomeThenAdvance(bool didShoot)
        {
            yield return new WaitForSeconds(outcomeDisplayDuration);
            _currentDilemmaIndex++;
            PresentNextDilemma();
        }

        private IEnumerator DecisionTimer(float timeLimit)
        {
            yield return new WaitForSeconds(timeLimit);
            if (CurrentState == GameState.DilemmaActive)
            {
                // Time expired — force a "spare" (did nothing)
                scoreManager.RecordTimeout(_activeCharacter, _activeDilemma);
                TransitionTo(GameState.ShowingOutcome);
                StartCoroutine(ShowOutcomeThenAdvance(false));
            }
        }

        private void TransitionTo(GameState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        private HistoricalCharacter FindCharacter(string id)
        {
            return _gameData?.Characters.Find(c => c.Id == id);
        }

        private static void ShuffleDilemmas(List<MoralDilemma> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }

    // ── Enumerations ───────────────────────────────────────────────────────────

    public enum GameState
    {
        MainMenu,
        Loading,
        DilemmaActive,
        ShowingOutcome,
        GameOver
    }

    public enum GameMode
    {
        None,
        AR,
        VR
    }
}
