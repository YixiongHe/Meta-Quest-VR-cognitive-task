using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class QuestControllerSelector : MonoBehaviour
{
    [Header("Ray Origin")]
    public Transform rayOrigin;

    [Header("Ray Settings")]
    public float rayDistance = 20f;
    public float rayThickness = 0.01f;

    [Header("Ray Colors")]
    public Color normalRayColor = Color.green;
    public Color targetRayColor = Color.yellow;

    private InputDevice rightController;

    private LineRenderer lineRenderer;
    private Material rayMaterial;

    private bool triggerWasPressed;
    private float nextControllerSearchTime;
    private Component lastTarget;

    private void Start()
    {
        CreateVisibleRay();
        FindRightController();
    }

    private void Update()
    {
        if (rayOrigin == null)
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }

            return;
        }

        // 如果还没有找到右手柄，每1秒重新找一次
        if (!rightController.isValid
            && Time.unscaledTime >= nextControllerSearchTime)
        {
            FindRightController();

            nextControllerSearchTime =
                Time.unscaledTime + 1f;
        }

        Vector3 rayStart =
            rayOrigin.position;

        Vector3 rayDirection =
            rayOrigin.forward;

        Ray ray =
            new Ray(
                rayStart,
                rayDirection
            );

        // 检查射线碰到的全部物体
        RaycastHit[] hits =
            Physics.RaycastAll(
                ray,
                rayDistance,
                ~0,
                QueryTriggerInteraction.Collide
            );

        Array.Sort(
            hits,
            delegate (RaycastHit a, RaycastHit b)
            {
                return
                    a.distance.CompareTo(b.distance);
            }
        );

        NumberButton targetButton = null;
        BoardActionButton targetActionButton = null;

        float visibleLength =
            rayDistance;

        foreach (RaycastHit hit in hits)
        {
            NumberButton possibleButton =
                hit.collider
                    .GetComponentInParent<NumberButton>();

            if (possibleButton != null)
            {
                targetButton =
                    possibleButton;

                visibleLength =
                    hit.distance;

                break;
            }

            BoardActionButton possibleActionButton =
                hit.collider.GetComponentInParent<BoardActionButton>();

            if (possibleActionButton != null)
            {
                targetActionButton = possibleActionButton;
                visibleLength = hit.distance;
                break;
            }
        }

        // 没碰到数字时，让射线停在最近物体上
        if (targetButton == null
            && targetActionButton == null
            && hits.Length > 0)
        {
            visibleLength =
                hits[0].distance;
        }

        Component currentTarget = targetButton != null
            ? targetButton
            : targetActionButton;

        if (currentTarget != lastTarget)
        {
            Debug.Log(
                currentTarget != null
                    ? "Target selectable found: "
                        + currentTarget.gameObject.name
                    : "No number button targeted."
            );

            lastTarget = currentTarget;
        }

        UpdateVisibleRay(
            rayStart,
            rayDirection,
            visibleLength,
            currentTarget != null
        );

        bool triggerPressed =
            ReadRightTrigger();

        // 只在扳机刚按下的一刻执行一次
        if (triggerPressed
            && !triggerWasPressed)
        {
            Debug.Log(
                "Right trigger pressed."
            );

            if (targetButton != null)
            {
                Debug.Log(
                    "Selected number object: "
                    + targetButton.gameObject.name
                );

                targetButton.Select();
            }
            else if (targetActionButton != null)
            {
                Debug.Log(
                    "Selected action object: "
                    + targetActionButton.gameObject.name
                );

                targetActionButton.Select();
            }
            else
            {
                Debug.Log(
                    "Trigger pressed, but no number button was targeted."
                );
            }
        }

        triggerWasPressed =
            triggerPressed;
    }

    private void FindRightController()
    {
        // 方法1：通过 XRNode 找右手柄
        rightController =
            InputDevices.GetDeviceAtXRNode(
                XRNode.RightHand
            );

        if (rightController.isValid)
        {
            Debug.Log(
                "Right controller detected: "
                + rightController.name
            );

            return;
        }

        // 方法2：通过设备特征找右手柄
        List<InputDevice> devices =
            new List<InputDevice>();

        InputDeviceCharacteristics characteristics =
            InputDeviceCharacteristics.Controller
            | InputDeviceCharacteristics.HeldInHand
            | InputDeviceCharacteristics.Right;

        InputDevices.GetDevicesWithCharacteristics(
            characteristics,
            devices
        );

        if (devices.Count > 0)
        {
            rightController =
                devices[0];

            Debug.Log(
                "Right controller found: "
                + rightController.name
            );
        }
        else
        {
            Debug.LogWarning(
                "Right controller has not been detected."
            );
        }
    }

    private bool ReadRightTrigger()
    {
        // 方法1：Unity XR triggerButton
        if (rightController.isValid)
        {
            bool triggerButton;

            if (rightController.TryGetFeatureValue(
                    CommonUsages.triggerButton,
                    out triggerButton)
                && triggerButton)
            {
                return true;
            }

            // 方法2：Unity XR 模拟扳机值
            float triggerValue;

            if (rightController.TryGetFeatureValue(
                    CommonUsages.trigger,
                    out triggerValue)
                && triggerValue > 0.55f)
            {
                return true;
            }
        }

        // 方法3：Meta SDK
        if (OVRInput.Get(
                OVRInput.Button.PrimaryIndexTrigger,
                OVRInput.Controller.RTouch))
        {
            return true;
        }

        // 方法4：Meta Raw Input
        if (OVRInput.Get(
                OVRInput.RawButton.RIndexTrigger))
        {
            return true;
        }

        return false;
    }

    private void CreateVisibleRay()
    {
        lineRenderer =
            GetComponent<LineRenderer>();

        if (lineRenderer == null)
        {
            lineRenderer =
                gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.positionCount = 2;

        lineRenderer.useWorldSpace =
            true;

        lineRenderer.startWidth =
            rayThickness;

        lineRenderer.endWidth =
            rayThickness * 0.5f;

        lineRenderer.numCapVertices = 4;

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Unlit"
            );

        if (shader == null)
        {
            shader =
                Shader.Find(
                    "Sprites/Default"
                );
        }

        if (shader == null)
        {
            Debug.LogError(
                "Ray shader could not be found."
            );

            return;
        }

        rayMaterial =
            new Material(shader);

        lineRenderer.material =
            rayMaterial;

        SetRayColor(
            normalRayColor
        );
    }

    private void UpdateVisibleRay(
        Vector3 start,
        Vector3 direction,
        float length,
        bool targetingButton)
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.enabled =
            true;

        lineRenderer.startWidth =
            rayThickness;

        lineRenderer.endWidth =
            rayThickness * 0.5f;

        lineRenderer.SetPosition(
            0,
            start
        );

        lineRenderer.SetPosition(
            1,
            start + direction * length
        );

        if (targetingButton)
        {
            SetRayColor(
                targetRayColor
            );
        }
        else
        {
            SetRayColor(
                normalRayColor
            );
        }
    }

    private void SetRayColor(
        Color color)
    {
        if (lineRenderer != null)
        {
            lineRenderer.startColor =
                color;

            lineRenderer.endColor =
                color;
        }

        if (rayMaterial == null)
        {
            return;
        }

        if (rayMaterial.HasProperty(
                "_BaseColor"))
        {
            rayMaterial.SetColor(
                "_BaseColor",
                color
            );
        }

        if (rayMaterial.HasProperty(
                "_Color"))
        {
            rayMaterial.SetColor(
                "_Color",
                color
            );
        }
    }
}
