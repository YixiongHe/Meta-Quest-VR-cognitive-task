using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class NumberBoardTask : MonoBehaviour
{
    private enum TaskMode
    {
        None,
        Schulte,
        MatchingNumbers
    }

    private class MatchingTurnRecord
    {
        public int number;
        public string actor;
        public bool correct;
        public float duration;
    }

    [Header("Button Settings")]
    public GameObject numberButtonPrefab;
    public Transform taskRoot;

    [Header("Result Display")]
    public GameObject resultPanel;
    public TMP_Text resultText;

    [Header("Level Settings")]
    [Range(3, 6)]
    public int selectedGridSize = 3;
    [Range(1, 10)]
    public int resultsPerPage = 6;

    [Header("Task State")]
    public int totalNumbers;
    public int currentTarget = 1;

    [Header("Matching Numbers")]
    public float ghostTurnDelay = 0.8f;
    public Color matchingPlayerColor = new Color(0.1f, 0.45f, 0.95f, 1f);
    public Color matchingGhostColor = new Color(0.95f, 0.42f, 0.1f, 1f);
    public Color matchingNeutralColor = new Color(0.2f, 0.48f, 0.7f, 1f);

    [Header("Schulte Options")]
    [Tooltip("The ghost automatically selects every second number after the player selects one.")]
    public bool schulteGhostEnabled;
    [Tooltip("Odd numbers are blue and even numbers are orange.")]
    public bool schulteOddEvenColorsEnabled;

    private TaskMode currentMode;
    private int matchingDifficulty;
    private bool ghostTurn;
    private bool taskEnded;
    private int resultPage;
    private int correctClicks;
    private int wrongClicks;
    private float startTime;
    private float lastCorrectTime;
    private float totalCorrectReactionTime;
    private Coroutine ghostTurnCoroutine;
    private GameObject turnIndicator;
    private TextMesh turnIndicatorText;

    private readonly List<GameObject> spawnedButtons = new List<GameObject>();
    private readonly List<GameObject> actionButtons = new List<GameObject>();
    private readonly List<float> reactionTimes = new List<float>();
    private readonly List<MatchingTurnRecord> matchingRecords = new List<MatchingTurnRecord>();
    private readonly Dictionary<int, NumberButton> matchingButtons =
        new Dictionary<int, NumberButton>();

    private void Start()
    {
        resultsPerPage = Mathf.Clamp(resultsPerPage, 1, 6);
        EnsureResultTextFont();
        PositionResultTextAboveControls();
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        ResetToMenu();

        CreateActionButton(
            "Level_3x3",
            "3 x 3",
            BoardActionButton.ActionType.ConfigureSchulteLevel,
            3,
            new Vector3(0f, 0.8f, -0.06f),
            new Vector3(2.6f, 0.24f, 0.08f)
        );
        CreateActionButton(
            "Level_4x4",
            "4 x 4",
            BoardActionButton.ActionType.ConfigureSchulteLevel,
            4,
            new Vector3(0f, 0.38f, -0.06f),
            new Vector3(2.6f, 0.24f, 0.08f)
        );
        CreateActionButton(
            "Level_5x5",
            "5 x 5",
            BoardActionButton.ActionType.ConfigureSchulteLevel,
            5,
            new Vector3(0f, -0.04f, -0.06f),
            new Vector3(2.6f, 0.24f, 0.08f)
        );
        CreateActionButton(
            "Level_6x6",
            "6 x 6",
            BoardActionButton.ActionType.ConfigureSchulteLevel,
            6,
            new Vector3(0f, -0.46f, -0.06f),
            new Vector3(2.6f, 0.24f, 0.08f)
        );
        CreateActionButton(
            "ExitAppButton",
            "EXIT APP",
            BoardActionButton.ActionType.Quit,
            0,
            new Vector3(0f, -0.9f, -0.06f),
            new Vector3(2.6f, 0.24f, 0.08f)
        );
    }

    public void ShowLevelMenu()
    {
        ShowMainMenu();
    }

    public void ShowSchulteSetup(int gridSize)
    {
        selectedGridSize = Mathf.Clamp(gridSize, 3, 6);
        ResetToMenu();

        CreateStatusLabel(
            selectedGridSize + " x " + selectedGridSize + " SETUP",
            new Vector3(0f, 0.98f, -0.08f)
        );
        CreateActionButton(
            "SchulteGhostOption",
            "GHOST: " + (schulteGhostEnabled ? "ON" : "OFF"),
            BoardActionButton.ActionType.ToggleSchulteGhost,
            0,
            new Vector3(0f, 0.5f, -0.06f),
            new Vector3(3.4f, 0.24f, 0.08f)
        );
        CreateActionButton(
            "SchulteColorOption",
            "ODD / EVEN COLORS: " + (schulteOddEvenColorsEnabled ? "ON" : "OFF"),
            BoardActionButton.ActionType.ToggleSchulteColors,
            0,
            new Vector3(0f, 0.1f, -0.06f),
            new Vector3(3.4f, 0.24f, 0.08f)
        );
        CreateActionButton(
            "StartConfiguredSchulte",
            "START " + selectedGridSize + " x " + selectedGridSize,
            BoardActionButton.ActionType.StartSchulteLevel,
            selectedGridSize,
            new Vector3(0f, -0.34f, -0.06f),
            new Vector3(3.4f, 0.24f, 0.08f)
        );
        CreateActionButton(
            "SchulteSetupBack",
            "BACK",
            BoardActionButton.ActionType.ShowSchulteLevels,
            0,
            new Vector3(0f, -0.82f, -0.06f),
            new Vector3(2.4f, 0.24f, 0.08f)
        );
    }

    public void ToggleSchulteGhost()
    {
        schulteGhostEnabled = !schulteGhostEnabled;
        ShowSchulteSetup(selectedGridSize);
    }

    public void ToggleSchulteColors()
    {
        schulteOddEvenColorsEnabled = !schulteOddEvenColorsEnabled;
        ShowSchulteSetup(selectedGridSize);
    }

    public void StartLevel(int gridSize)
    {
        BeginTask(TaskMode.Schulte);
        selectedGridSize = Mathf.Clamp(gridSize, 3, 6);
        totalNumbers = selectedGridSize * selectedGridSize;
        GenerateSchulteButtons();
        CreateInGameReturnButton();

        if (schulteGhostEnabled || schulteOddEvenColorsEnabled)
        {
            CreateTurnIndicator();
            UpdateTurnIndicator();
        }

        Debug.Log(
            "Started Schulte " + selectedGridSize + "x" + selectedGridSize
            + ". Select 1 to " + totalNumbers + " in order."
        );
    }

    public void StartMatchingLevel(int difficulty)
    {
        matchingDifficulty = Mathf.Clamp(difficulty, 1, 2);
        BeginTask(TaskMode.MatchingNumbers);
        totalNumbers = 10;
        GenerateMatchingButtons();
        CreateTurnIndicator();
        UpdateTurnIndicator();

        Debug.Log(
            "Started Matching Numbers Level " + matchingDifficulty
            + ". Player selects odd numbers; ghost selects even numbers."
        );
    }

    public void RestartLevel()
    {
        if (currentMode == TaskMode.MatchingNumbers)
        {
            StartMatchingLevel(matchingDifficulty);
            return;
        }

        StartLevel(selectedGridSize);
    }

    public void QuitApplication()
    {
        Debug.Log("Quit requested.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void PreviousResultPage()
    {
        if (resultPage <= 0)
        {
            return;
        }

        resultPage--;
        ShowResultPage();
    }

    public void NextResultPage()
    {
        if (resultPage >= GetResultPageCount() - 1)
        {
            return;
        }

        resultPage++;
        ShowResultPage();
    }

    public bool SelectNumber(int selectedNumber)
    {
        if (taskEnded)
        {
            return false;
        }

        if (currentMode == TaskMode.MatchingNumbers)
        {
            return SelectMatchingNumber(selectedNumber);
        }

        return SelectSchulteNumber(selectedNumber);
    }

    private bool SelectSchulteNumber(int selectedNumber)
    {
        float reactionTime = Time.time - lastCorrectTime;
        bool correct = !ghostTurn && selectedNumber == currentTarget;
        if (!correct)
        {
            wrongClicks++;
            if (UsesTurnLog())
            {
                matchingRecords.Add(new MatchingTurnRecord
                {
                    number = selectedNumber,
                    actor = "Player",
                    correct = false,
                    duration = reactionTime
                });
            }
            Debug.Log("Wrong. Expected " + currentTarget + ", but selected " + selectedNumber);
            return false;
        }

        correctClicks++;
        reactionTimes.Add(reactionTime);
        totalCorrectReactionTime += reactionTime;
        if (UsesTurnLog())
        {
            matchingRecords.Add(new MatchingTurnRecord
            {
                number = selectedNumber,
                actor = "Player",
                correct = true,
                duration = reactionTime
            });
        }
        currentTarget++;
        lastCorrectTime = Time.time;

        if (currentTarget > totalNumbers)
        {
            EndTask();
        }
        else if (IsGhostMode())
        {
            ghostTurnCoroutine = StartCoroutine(GhostTurnRoutine());
        }
        else
        {
            UpdateTurnIndicator();
        }

        return true;
    }

    private bool SelectMatchingNumber(int selectedNumber)
    {
        float reactionTime = Time.time - lastCorrectTime;
        bool correct = !ghostTurn && selectedNumber == currentTarget;

        if (!correct)
        {
            wrongClicks++;
            matchingRecords.Add(new MatchingTurnRecord
            {
                number = selectedNumber,
                actor = "Player",
                correct = false,
                duration = reactionTime
            });
            Debug.Log("Matching Numbers: incorrect or early selection.");
            return false;
        }

        correctClicks++;
        reactionTimes.Add(reactionTime);
        totalCorrectReactionTime += reactionTime;
        matchingRecords.Add(new MatchingTurnRecord
        {
            number = selectedNumber,
            actor = "Player",
            correct = true,
            duration = reactionTime
        });

        currentTarget++;
        lastCorrectTime = Time.time;

        if (currentTarget > totalNumbers)
        {
            EndTask();
            return true;
        }

        ghostTurnCoroutine = StartCoroutine(GhostTurnRoutine());
        return true;
    }

    private IEnumerator GhostTurnRoutine()
    {
        ghostTurn = true;
        UpdateTurnIndicator();
        yield return new WaitForSeconds(ghostTurnDelay);

        if (taskEnded || !IsGhostMode())
        {
            yield break;
        }

        int ghostNumber = currentTarget;
        float ghostDuration = Time.time - lastCorrectTime;
        NumberButton ghostButton;
        if (matchingButtons.TryGetValue(ghostNumber, out ghostButton)
            && ghostButton != null)
        {
            ghostButton.CompleteAutomatically(matchingGhostColor);
        }

        matchingRecords.Add(new MatchingTurnRecord
        {
            number = ghostNumber,
            actor = "Ghost",
            correct = true,
            duration = ghostDuration
        });

        currentTarget++;
        lastCorrectTime = Time.time;
        ghostTurn = false;
        ghostTurnCoroutine = null;

        if (currentTarget > totalNumbers)
        {
            EndTask();
            yield break;
        }

        UpdateTurnIndicator();
    }

    private void BeginTask(TaskMode mode)
    {
        StopGhostTurn();
        DestroyTurnIndicator();
        ClearNumberButtons();
        ClearActionButtons();

        currentMode = mode;
        taskEnded = false;
        ghostTurn = false;
        resultPage = 0;
        currentTarget = 1;
        correctClicks = 0;
        wrongClicks = 0;
        totalCorrectReactionTime = 0f;
        reactionTimes.Clear();
        matchingRecords.Clear();

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        startTime = Time.time;
        lastCorrectTime = Time.time;
    }

    private void ResetToMenu()
    {
        StopGhostTurn();
        DestroyTurnIndicator();
        ClearNumberButtons();
        ClearActionButtons();
        currentMode = TaskMode.None;
        taskEnded = false;
        ghostTurn = false;

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    private void GenerateSchulteButtons()
    {
        List<int> numbers = new List<int>();
        for (int number = 1; number <= totalNumbers; number++)
        {
            numbers.Add(number);
        }

        Shuffle(numbers);

        // Scale the whole number grid (positions, buttons and inherited text)
        // to two thirds while retaining room for the in-game Return button.
        const float numberGridScale = 2f / 3f;
        const float gridWidth = 6.8f * numberGridScale;
        const float gridHeight = 4f * numberGridScale;
        float spacingX = gridWidth / selectedGridSize;
        float spacingY = gridHeight / selectedGridSize;
        float buttonSize = Mathf.Min(spacingX, spacingY) * 0.78f;

        for (int index = 0; index < numbers.Count; index++)
        {
            int row = index / selectedGridSize;
            int column = index % selectedGridSize;
            float x = (column - (selectedGridSize - 1) * 0.5f) * spacingX;
            float y = ((selectedGridSize - 1) * 0.5f - row) * spacingY;
            Color color = schulteOddEvenColorsEnabled
                ? numbers[index] % 2 == 1 ? matchingPlayerColor : matchingGhostColor
                : matchingNeutralColor;
            SpawnNumberButton(numbers[index], x, y, buttonSize, color);
        }
    }

    private void GenerateMatchingButtons()
    {
        List<int> numbers = new List<int>();
        for (int number = 1; number <= totalNumbers; number++)
        {
            numbers.Add(number);
        }

        Shuffle(numbers);

        const int columns = 5;
        const float spacingX = 0.8f;
        const float spacingY = 0.75f;
        const float buttonSize = 0.5f;

        for (int index = 0; index < numbers.Count; index++)
        {
            int row = index / columns;
            int column = index % columns;
            float x = (column - 2) * spacingX;
            float y = (0.5f - row) * spacingY;
            Color color = matchingDifficulty == 2
                ? numbers[index] % 2 == 1 ? matchingPlayerColor : matchingGhostColor
                : matchingNeutralColor;
            SpawnNumberButton(numbers[index], x, y, buttonSize, color);
        }
    }

    private void SpawnNumberButton(
        int number,
        float x,
        float y,
        float size,
        Color color)
    {
        if (numberButtonPrefab == null || taskRoot == null)
        {
            Debug.LogError("Number Board references are missing.");
            return;
        }

        GameObject buttonObject = Instantiate(numberButtonPrefab, taskRoot);
        buttonObject.name = "Number_" + number;
        buttonObject.transform.localPosition = new Vector3(x, y, -0.05f);
        buttonObject.transform.localRotation = Quaternion.identity;
        buttonObject.transform.localScale = new Vector3(size, size, 0.08f);
        spawnedButtons.Add(buttonObject);

        NumberButton button = buttonObject.GetComponent<NumberButton>();
        if (button == null)
        {
            Debug.LogError("NumberButton component is missing.");
            return;
        }

        button.Setup(number, this);
        button.SetNormalColor(color);
        matchingButtons[number] = button;
    }

    private void CreateTurnIndicator()
    {
        turnIndicator = new GameObject("TurnIndicator");
        turnIndicator.transform.SetParent(taskRoot, false);
        turnIndicator.transform.localPosition = new Vector3(0f, 2.05f, -0.08f);
        turnIndicatorText = turnIndicator.AddComponent<TextMesh>();
        turnIndicatorText.anchor = TextAnchor.MiddleCenter;
        turnIndicatorText.alignment = TextAlignment.Center;
        turnIndicatorText.characterSize = 0.05f;
        turnIndicatorText.fontSize = 48;
        turnIndicatorText.fontStyle = FontStyle.Bold;
        turnIndicatorText.color = Color.white;
    }

    private void UpdateTurnIndicator()
    {
        if (turnIndicatorText == null)
        {
            return;
        }

        string colorRule = GetColorRuleText();
        string turnText = IsGhostMode()
            ? ghostTurn
                ? "GHOST TURN: " + currentTarget
                : "YOUR TURN: " + currentTarget
            : "SELECT IN ORDER: " + currentTarget;
        turnIndicatorText.text = colorRule + turnText;
        turnIndicatorText.color = ghostTurn ? matchingGhostColor : Color.white;
    }

    private bool IsGhostMode()
    {
        return currentMode == TaskMode.MatchingNumbers
            || currentMode == TaskMode.Schulte && schulteGhostEnabled;
    }

    private bool HasOddEvenColors()
    {
        return currentMode == TaskMode.MatchingNumbers
            ? matchingDifficulty == 2
            : currentMode == TaskMode.Schulte && schulteOddEvenColorsEnabled;
    }

    private bool UsesTurnLog()
    {
        return IsGhostMode();
    }

    private string GetColorRuleText()
    {
        if (!HasOddEvenColors())
        {
            return string.Empty;
        }

        return IsGhostMode()
            ? "BLUE ODD = YOU | ORANGE EVEN = GHOST\n"
            : "ODD = BLUE | EVEN = ORANGE\n";
    }

    private void EndTask()
    {
        taskEnded = true;
        ghostTurn = false;
        DestroyTurnIndicator();

        foreach (GameObject button in spawnedButtons)
        {
            if (button != null)
            {
                button.SetActive(false);
            }
        }

        resultPage = 0;
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        ShowResultPage();
    }

    private void ShowResultPage()
    {
        if (resultText == null)
        {
            Debug.LogError("Result Text has not been assigned.");
            return;
        }

        float totalTime = Time.time - startTime;
        int totalAttempts = correctClicks + wrongClicks;
        float accuracy = totalAttempts > 0
            ? correctClicks / (float)totalAttempts * 100f
            : 0f;
        float averageReactionTime = correctClicks > 0
            ? totalCorrectReactionTime / correctClicks
            : 0f;
        int recordCount = GetRecordCount();
        int pageCount = GetResultPageCount();
        int firstIndex = resultPage * resultsPerPage;
        int lastIndex = Mathf.Min(firstIndex + resultsPerPage, recordCount);

        StringBuilder results = new StringBuilder();
        results.AppendLine("<b>TASK COMPLETE</b>");
        results.AppendLine(GetModeTitle());
        results.AppendLine("Total Time: " + totalTime.ToString("F2") + "s");
        results.AppendLine("Correct: " + correctClicks + "    Wrong: " + wrongClicks);
        results.AppendLine(
            "Accuracy: " + accuracy.ToString("F1")
            + "%    Average: " + averageReactionTime.ToString("F2") + "s"
        );
        results.AppendLine();
        results.AppendLine(
            "<b>Log " + (firstIndex + 1) + "-" + lastIndex + " of " + recordCount
            + "   (Page " + (resultPage + 1) + "/" + pageCount + ")</b>"
        );

        if (UsesTurnLog())
        {
            AppendMatchingRecords(results, firstIndex, lastIndex);
        }
        else
        {
            AppendSchulteRecords(results, firstIndex, lastIndex);
        }

        resultText.text = results.ToString();
        CreateResultControls(pageCount);
    }

    private string GetModeTitle()
    {
        if (currentMode == TaskMode.MatchingNumbers)
        {
            return "Matching Numbers - Level " + matchingDifficulty;
        }

        string options = string.Empty;
        if (schulteGhostEnabled)
        {
            options += " + Ghost";
        }

        if (schulteOddEvenColorsEnabled)
        {
            options += " + Odd/Even Colors";
        }

        return "Schulte Grid - " + selectedGridSize + " x " + selectedGridSize + options;
    }

    private void AppendSchulteRecords(StringBuilder results, int firstIndex, int lastIndex)
    {
        for (int index = firstIndex; index < lastIndex; index++)
        {
            results.Append("No. " + (index + 1) + ": " + reactionTimes[index].ToString("F2") + "s");
            AppendRecordSeparator(results, index, firstIndex, lastIndex);
        }
    }

    private void AppendMatchingRecords(StringBuilder results, int firstIndex, int lastIndex)
    {
        for (int index = firstIndex; index < lastIndex; index++)
        {
            MatchingTurnRecord record = matchingRecords[index];
            string actor = record.actor == "Player" ? "P" : "G";
            string status = record.correct ? "OK" : "MISS";
            results.Append(
                actor + " " + record.number + " " + status
                + " " + record.duration.ToString("F2") + "s"
            );
            AppendRecordSeparator(results, index, firstIndex, lastIndex);
        }
    }

    private static void AppendRecordSeparator(
        StringBuilder results,
        int index,
        int firstIndex,
        int lastIndex)
    {
        if ((index - firstIndex + 1) % 2 == 0 || index == lastIndex - 1)
        {
            results.AppendLine();
        }
        else
        {
            results.Append("    ");
        }
    }

    private int GetRecordCount()
    {
        return UsesTurnLog()
            ? matchingRecords.Count
            : reactionTimes.Count;
    }

    private int GetResultPageCount()
    {
        return Mathf.Max(1, Mathf.CeilToInt(GetRecordCount() / (float)resultsPerPage));
    }

    private void CreateResultControls(int pageCount)
    {
        ClearActionButtons();

        bool hasPreviousPage = resultPage > 0;
        bool hasNextPage = resultPage < pageCount - 1;

        if (hasPreviousPage)
        {
            CreateActionButton(
                "PreviousPage",
                "PREV",
                BoardActionButton.ActionType.PreviousPage,
                0,
                new Vector3(hasNextPage ? -0.55f : 0f, -0.74f, -2.08f),
                new Vector3(0.72f, 0.14f, 0.08f),
                0.026f
            );
        }

        if (hasNextPage)
        {
            CreateActionButton(
                "NextPage",
                "NEXT",
                BoardActionButton.ActionType.NextPage,
                0,
                new Vector3(hasPreviousPage ? 0.55f : 0f, -0.74f, -2.08f),
                new Vector3(0.72f, 0.14f, 0.08f),
                0.026f
            );
        }

        CreateActionButton(
            "RestartButton",
            "RESTART",
            BoardActionButton.ActionType.Restart,
            0,
            new Vector3(0f, -1.01f, -2.08f),
            new Vector3(1.5f, 0.14f, 0.08f),
            0.032f
        );
        CreateActionButton(
            "ReturnToMenuButton",
            "RETURN TO MENU",
            BoardActionButton.ActionType.ShowMainMenu,
            0,
            new Vector3(0f, -1.28f, -2.08f),
            new Vector3(2.5f, 0.14f, 0.08f),
            0.022f
        );
    }

    private void CreateActionButton(
        string objectName,
        string label,
        BoardActionButton.ActionType action,
        int value,
        Vector3 localPosition,
        Vector3 localScale,
        float labelCharacterSize = 0.041f)
    {
        if (taskRoot == null)
        {
            Debug.LogError("Task Root has not been assigned.");
            return;
        }

        GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cube);
        button.name = objectName;
        button.transform.SetParent(taskRoot, false);
        button.transform.localPosition = localPosition;
        button.transform.localRotation = Quaternion.identity;
        button.transform.localScale = localScale;

        BoardActionButton actionButton = button.AddComponent<BoardActionButton>();
        actionButton.Setup(this, action, label, value, labelCharacterSize);
        actionButtons.Add(button);
    }

    private void CreateInGameReturnButton()
    {
        CreateActionButton(
            "InGameReturnButton",
            "RETURN",
            BoardActionButton.ActionType.ShowMainMenu,
            0,
            new Vector3(3.45f, -2.15f, -0.06f),
            new Vector3(1.8f, 0.18f, 0.08f)
        );
    }

    private void CreateStatusLabel(string label, Vector3 localPosition)
    {
        if (taskRoot == null)
        {
            return;
        }

        GameObject labelObject = new GameObject("MenuStatusLabel");
        labelObject.transform.SetParent(taskRoot, false);
        labelObject.transform.localPosition = localPosition;

        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = label;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.041f;
        textMesh.fontSize = 64;
        textMesh.fontStyle = FontStyle.Bold;
        textMesh.color = Color.white;
        actionButtons.Add(labelObject);
    }

    private void ClearNumberButtons()
    {
        foreach (GameObject button in spawnedButtons)
        {
            if (button != null)
            {
                Destroy(button);
            }
        }

        spawnedButtons.Clear();
        matchingButtons.Clear();
    }

    private void ClearActionButtons()
    {
        foreach (GameObject button in actionButtons)
        {
            if (button != null)
            {
                Destroy(button);
            }
        }

        actionButtons.Clear();
    }

    private void StopGhostTurn()
    {
        if (ghostTurnCoroutine != null)
        {
            StopCoroutine(ghostTurnCoroutine);
            ghostTurnCoroutine = null;
        }

        ghostTurn = false;
    }

    private void DestroyTurnIndicator()
    {
        if (turnIndicator != null)
        {
            Destroy(turnIndicator);
        }

        turnIndicator = null;
        turnIndicatorText = null;
    }

    private void EnsureResultTextFont()
    {
        if (resultText != null && resultText.font == null)
        {
            resultText.font = TMP_Settings.defaultFontAsset;
        }
    }

    private void PositionResultTextAboveControls()
    {
        if (resultText == null)
        {
            return;
        }

        RectTransform textTransform = resultText.rectTransform;
        textTransform.anchoredPosition = new Vector2(
            textTransform.anchoredPosition.x,
            100f
        );
    }

    private static void Shuffle(List<int> list)
    {
        for (int index = 0; index < list.Count; index++)
        {
            int randomIndex = Random.Range(index, list.Count);
            int value = list[index];
            list[index] = list[randomIndex];
            list[randomIndex] = value;
        }
    }
}
