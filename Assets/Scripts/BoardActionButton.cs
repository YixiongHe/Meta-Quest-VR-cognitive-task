using UnityEngine;

public class BoardActionButton : MonoBehaviour
{
    public enum ActionType
    {
        StartSchulteLevel,
        StartMatchingLevel,
        ConfigureSchulteLevel,
        ToggleSchulteGhost,
        ToggleSchulteColors,
        Restart,
        ShowMainMenu,
        ShowSchulteLevels,
        PreviousPage,
        NextPage,
        Quit
    }

    public ActionType actionType;

    public int gridSize;

    private NumberBoardTask taskManager;

    public void Setup(
        NumberBoardTask manager,
        ActionType action,
        string label,
        int size = 0,
        float labelCharacterSize = 0.041f)
    {
        taskManager = manager;
        actionType = action;
        gridSize = size;

        Renderer buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer != null)
        {
            buttonRenderer.material.color = new Color(
                0.08f,
                0.32f,
                0.55f,
                1f
            );
        }

        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(transform, false);

        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = label;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        // Menu labels use the smaller, consistent board-world size requested
        // for the level and setup screens.
        textMesh.characterSize = labelCharacterSize;
        textMesh.fontSize = 64;
        textMesh.fontStyle = FontStyle.Bold;
        textMesh.color = Color.white;
        labelObject.transform.localPosition = new Vector3(0f, 0f, -0.55f);
        labelObject.transform.localScale = new Vector3(
            1f / Mathf.Max(transform.localScale.x, 0.01f),
            1f / Mathf.Max(transform.localScale.y, 0.01f),
            1f / Mathf.Max(transform.localScale.z, 0.01f)
        );
    }

    public void Select()
    {
        if (taskManager == null)
        {
            Debug.LogError("Board action has no task manager.");
            return;
        }

        switch (actionType)
        {
            case ActionType.StartSchulteLevel:
                taskManager.StartLevel(gridSize);
                break;
            case ActionType.StartMatchingLevel:
                taskManager.StartMatchingLevel(gridSize);
                break;
            case ActionType.ConfigureSchulteLevel:
                taskManager.ShowSchulteSetup(gridSize);
                break;
            case ActionType.ToggleSchulteGhost:
                taskManager.ToggleSchulteGhost();
                break;
            case ActionType.ToggleSchulteColors:
                taskManager.ToggleSchulteColors();
                break;
            case ActionType.Restart:
                taskManager.RestartLevel();
                break;
            case ActionType.ShowMainMenu:
                taskManager.ShowMainMenu();
                break;
            case ActionType.ShowSchulteLevels:
                taskManager.ShowLevelMenu();
                break;
            case ActionType.PreviousPage:
                taskManager.PreviousResultPage();
                break;
            case ActionType.NextPage:
                taskManager.NextResultPage();
                break;
            case ActionType.Quit:
                taskManager.QuitApplication();
                break;
        }
    }
}
