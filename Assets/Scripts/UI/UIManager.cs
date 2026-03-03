using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ShouldYouShoot.Core;
using ShouldYouShoot.Data;

namespace ShouldYouShoot.UI
{
    /// <summary>
    /// Manages all on-screen UI: main menu, dilemma display, outcome panels,
    /// hint progression, and the game-over summary screen.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        // ── Panels ─────────────────────────────────────────────────────────────
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject dilemmaPanel;
        [SerializeField] private GameObject outcomePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject hintPanel;

        // ── Main Menu ──────────────────────────────────────────────────────────
        [Header("Main Menu")]
        [SerializeField] private Button startARButton;
        [SerializeField] private Button startVRButton;

        // ── Dilemma Panel ──────────────────────────────────────────────────────
        [Header("Dilemma UI")]
        [SerializeField] private TextMeshProUGUI dilemmaTitleText;
        [SerializeField] private TextMeshProUGUI dilemmaScenarioText;
        [SerializeField] private TextMeshProUGUI philosophicalQuestionText;
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private Button nextHintButton;
        [SerializeField] private Button shootButton;
        [SerializeField] private Button spareButton;
        [SerializeField] private Slider timerSlider;

        // ── Outcome Panel ──────────────────────────────────────────────────────
        [Header("Outcome UI")]
        [SerializeField] private TextMeshProUGUI outcomeHeadlineText;
        [SerializeField] private TextMeshProUGUI outcomeDescriptionText;
        [SerializeField] private TextMeshProUGUI scoreDeltaText;
        [SerializeField] private Image outcomeBackgroundImage;
        [SerializeField] private Color correctChoiceColor = new Color(0.2f, 0.7f, 0.2f, 0.85f);
        [SerializeField] private Color incorrectChoiceColor = new Color(0.8f, 0.1f, 0.1f, 0.85f);

        // ── Game Over Panel ────────────────────────────────────────────────────
        [Header("Game Over UI")]
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI correctCountText;
        [SerializeField] private TextMeshProUGUI incorrectCountText;
        [SerializeField] private Button mainMenuButton;

        // ── HUD ────────────────────────────────────────────────────────────────
        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI hudScoreText;

        // ── Runtime State ──────────────────────────────────────────────────────
        private GameManager _gameManager;
        private ScoreManager _scoreManager;
        private MoralDilemma _currentDilemma;
        private int _currentHintIndex;
        private Coroutine _timerCoroutine;

        // ── Unity Lifecycle ────────────────────────────────────────────────────
        private void Awake()
        {
            _gameManager = GameManager.Instance;
            _scoreManager = FindObjectOfType<ScoreManager>();

            WireButtons();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        // ── Event Subscriptions ────────────────────────────────────────────────

        private void SubscribeToEvents()
        {
            if (_gameManager == null) return;
            _gameManager.OnStateChanged    += HandleStateChanged;
            _gameManager.OnDilemmaStarted  += HandleDilemmaStarted;
            _gameManager.OnDecisionMade    += HandleDecisionMade;
        }

        private void UnsubscribeFromEvents()
        {
            if (_gameManager == null) return;
            _gameManager.OnStateChanged    -= HandleStateChanged;
            _gameManager.OnDilemmaStarted  -= HandleDilemmaStarted;
            _gameManager.OnDecisionMade    -= HandleDecisionMade;
        }

        // ── Button Wiring ──────────────────────────────────────────────────────

        private void WireButtons()
        {
            startARButton?.onClick.AddListener(() => _gameManager.StartARMode());
            startVRButton?.onClick.AddListener(() => _gameManager.StartVRMode());
            shootButton?.onClick.AddListener(() => _gameManager.RegisterShot());
            spareButton?.onClick.AddListener(() => _gameManager.RegisterSpare());
            nextHintButton?.onClick.AddListener(ShowNextHint);
            mainMenuButton?.onClick.AddListener(() => _gameManager.ReturnToMainMenu());
        }

        // ── State Handlers ─────────────────────────────────────────────────────

        private void HandleStateChanged(GameState state)
        {
            mainMenuPanel?.SetActive(state == GameState.MainMenu);
            dilemmaPanel?.SetActive(state == GameState.DilemmaActive);
            outcomePanel?.SetActive(state == GameState.ShowingOutcome);
            gameOverPanel?.SetActive(state == GameState.GameOver);

            if (state == GameState.GameOver)
                PopulateGameOverPanel();
        }

        private void HandleDilemmaStarted(MoralDilemma dilemma, HistoricalCharacter character)
        {
            _currentDilemma = dilemma;
            _currentHintIndex = 0;

            if (dilemmaTitleText)         dilemmaTitleText.text         = dilemma.Title;
            if (dilemmaScenarioText)      dilemmaScenarioText.text      = dilemma.ScenarioText;
            if (philosophicalQuestionText) philosophicalQuestionText.text = dilemma.PhilosophicalQuestion;

            // Show first hint
            UpdateHintDisplay();

            // Start countdown timer if applicable
            if (dilemma.DecisionTimeLimit > 0)
            {
                if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
                _timerCoroutine = StartCoroutine(RunTimerUI(dilemma.DecisionTimeLimit));
            }
            else
            {
                if (timerSlider != null) timerSlider.gameObject.SetActive(false);
            }
        }

        private void HandleDecisionMade(bool didShoot, HistoricalCharacter character)
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }

            bool isCorrect = (didShoot == character.ShootingIsCorrectChoice);
            string headline = isCorrect ? "Correct Moral Decision" : "Questionable Choice";
            string description = didShoot ? character.OutcomeIfShot : character.OutcomeIfSpared;

            if (outcomeHeadlineText)    outcomeHeadlineText.text    = headline;
            if (outcomeDescriptionText) outcomeDescriptionText.text = description;
            if (outcomeBackgroundImage) outcomeBackgroundImage.color = isCorrect ? correctChoiceColor : incorrectChoiceColor;

            var history = _scoreManager?.GetHistory();
            int latestDelta = (history != null && history.Count > 0)
                ? history[history.Count - 1].PointsDelta
                : 0;
            if (scoreDeltaText) scoreDeltaText.text = latestDelta >= 0 ? $"+{latestDelta}" : $"{latestDelta}";

            UpdateHUDScore();
        }

        // ── Hint Progression ───────────────────────────────────────────────────

        private void ShowNextHint()
        {
            if (_currentDilemma == null) return;
            _currentHintIndex = Mathf.Min(_currentHintIndex + 1, _currentDilemma.ProgressiveHints.Count - 1);
            _gameManager.RegisterHintRead(_currentHintIndex);
            UpdateHintDisplay();
        }

        private void UpdateHintDisplay()
        {
            if (_currentDilemma == null || _currentDilemma.ProgressiveHints == null ||
                _currentDilemma.ProgressiveHints.Count == 0)
            {
                hintPanel?.SetActive(false);
                return;
            }

            hintPanel?.SetActive(true);
            if (hintText) hintText.text = _currentDilemma.ProgressiveHints[_currentHintIndex];

            bool hasMoreHints = _currentHintIndex < _currentDilemma.ProgressiveHints.Count - 1;
            if (nextHintButton) nextHintButton.interactable = hasMoreHints;
        }

        // ── Timer UI ───────────────────────────────────────────────────────────

        private IEnumerator RunTimerUI(float duration)
        {
            if (timerSlider == null) yield break;
            timerSlider.gameObject.SetActive(true);
            timerSlider.maxValue = duration;
            timerSlider.value    = duration;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                timerSlider.value = duration - elapsed;
                yield return null;
            }

            timerSlider.value = 0f;
        }

        // ── Game Over ──────────────────────────────────────────────────────────

        private void PopulateGameOverPanel()
        {
            if (_scoreManager == null) return;
            if (finalScoreText)     finalScoreText.text     = $"Final Score: {_scoreManager.TotalScore}";
            if (correctCountText)   correctCountText.text   = $"Correct Decisions: {_scoreManager.CorrectDecisions}";
            if (incorrectCountText) incorrectCountText.text = $"Incorrect Decisions: {_scoreManager.IncorrectDecisions}";
        }

        // ── HUD ────────────────────────────────────────────────────────────────

        private void UpdateHUDScore()
        {
            if (hudScoreText != null && _scoreManager != null)
                hudScoreText.text = $"Score: {_scoreManager.TotalScore}";
        }
    }
}
