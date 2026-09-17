using System.Collections;
using TMPro;
using UnityEngine;

public class NumberButton : MonoBehaviour
{
    [Header("Button Data")]
    public int numberValue;

    public NumberBoardTask taskManager;

    [Header("Visual Feedback")]
    public Color correctColor =
        Color.green;

    public Color wrongColor =
        Color.red;

    public float wrongFlashDuration =
        0.35f;

    private Renderer buttonRenderer;

    private Collider buttonCollider;

    private Color originalColor;

    private bool alreadyCompleted;

    private void Awake()
    {
        buttonRenderer =
            GetComponent<Renderer>();

        if (buttonRenderer == null)
        {
            buttonRenderer =
                GetComponentInChildren<Renderer>();
        }

        buttonCollider =
            GetComponent<Collider>();

        if (buttonCollider == null)
        {
            buttonCollider =
                GetComponentInChildren<Collider>();
        }

        if (buttonRenderer != null)
        {
            originalColor =
                buttonRenderer.material.color;
        }
    }

    public void Setup(
        int value,
        NumberBoardTask manager)
    {
        numberValue =
            value;

        taskManager =
            manager;

        alreadyCompleted =
            false;

        // 支持你原来的 TextMesh
        TextMesh legacyText =
            GetComponentInChildren<TextMesh>(
                true
            );

        if (legacyText != null)
        {
            legacyText.text =
                value.ToString();
        }

        // 同时支持 TextMeshPro
        TMP_Text tmpText =
            GetComponentInChildren<TMP_Text>(
                true
            );

        if (tmpText != null)
        {
            tmpText.text =
                value.ToString();
        }

        if (legacyText == null
            && tmpText == null)
        {
            Debug.LogWarning(
                "No number text was found on "
                + gameObject.name
            );
        }
    }

    public void Select()
    {
        if (alreadyCompleted)
        {
            return;
        }

        if (taskManager == null)
        {
            Debug.LogError(
                "Task Manager is missing on "
                + gameObject.name
            );

            return;
        }

        bool wasCorrect =
            taskManager.SelectNumber(
                numberValue
            );

        StopAllCoroutines();

        if (wasCorrect)
        {
            ShowCorrect();
        }
        else
        {
            StartCoroutine(
                ShowWrong()
            );
        }
    }

    public void SetNormalColor(Color color)
    {
        if (buttonRenderer == null)
        {
            return;
        }

        originalColor = color;
        buttonRenderer.material.color = color;
    }

    public void CompleteAutomatically(Color color)
    {
        StopAllCoroutines();
        alreadyCompleted = true;

        if (buttonRenderer != null)
        {
            buttonRenderer.material.color = color;
        }

        if (buttonCollider != null)
        {
            buttonCollider.enabled = false;
        }
    }

    private void ShowCorrect()
    {
        alreadyCompleted =
            true;

        if (buttonRenderer != null)
        {
            buttonRenderer.material.color =
                correctColor;
        }

        // 正确以后不允许再点
        if (buttonCollider != null)
        {
            buttonCollider.enabled =
                false;
        }

        Debug.Log(
            "Correct button selected: "
            + numberValue
        );
    }

    private IEnumerator ShowWrong()
    {
        if (buttonRenderer != null)
        {
            buttonRenderer.material.color =
                wrongColor;
        }

        yield return
            new WaitForSeconds(
                wrongFlashDuration
            );

        if (buttonRenderer != null
            && !alreadyCompleted)
        {
            buttonRenderer.material.color =
                originalColor;
        }
    }
}
