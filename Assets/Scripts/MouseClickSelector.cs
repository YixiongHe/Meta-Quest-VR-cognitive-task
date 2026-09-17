using UnityEngine;
using UnityEngine.InputSystem;

public class MouseClickSelector : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogError("No camera with the MainCamera tag was found.");
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                NumberButton button =
                    hit.collider.GetComponentInParent<NumberButton>();

                if (button != null)
                {
                    button.Select();
                    return;
                }

                BoardActionButton actionButton =
                    hit.collider.GetComponentInParent<BoardActionButton>();

                if (actionButton != null)
                {
                    actionButton.Select();
                }
            }
        }
    }
}
