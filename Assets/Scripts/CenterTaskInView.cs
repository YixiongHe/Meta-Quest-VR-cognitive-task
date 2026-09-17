using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class CenterTaskInView : MonoBehaviour
{
    [Header("Placement")]
    public float distanceFromCamera = 4.5f;
    public float verticalOffset = 0f;

    [Header("Recenter")]
    public float placementDelay = 0.4f;

    private Camera sceneCamera;
    private Coroutine placementCoroutine;

    private readonly List<XRInputSubsystem> inputSubsystems =
        new List<XRInputSubsystem>();

    private IEnumerator Start()
    {
        // 等待 Quest 头部追踪初始化完成
        yield return new WaitForSecondsRealtime(0.75f);

        PlaceTaskInFront();

        SubscribeToTrackingOriginChanges();
    }

    private void SubscribeToTrackingOriginChanges()
    {
        inputSubsystems.Clear();

        SubsystemManager.GetSubsystems(inputSubsystems);

        foreach (XRInputSubsystem subsystem in inputSubsystems)
        {
            if (subsystem != null)
            {
                subsystem.trackingOriginUpdated +=
                    OnTrackingOriginUpdated;
            }
        }
    }

    private void OnTrackingOriginUpdated(
        XRInputSubsystem subsystem)
    {
        SchedulePlacement();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            SchedulePlacement();
        }
    }

    private void SchedulePlacement()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (placementCoroutine != null)
        {
            StopCoroutine(placementCoroutine);
        }

        placementCoroutine =
            StartCoroutine(PlaceAfterDelay());
    }

    private IEnumerator PlaceAfterDelay()
    {
        yield return new WaitForSecondsRealtime(
            placementDelay
        );

        PlaceTaskInFront();

        placementCoroutine = null;
    }

    public void PlaceTaskInFront()
    {
        sceneCamera = Camera.main;

        if (sceneCamera == null)
        {
            Debug.LogError(
                "No camera with the MainCamera tag was found."
            );

            return;
        }

        // 只取头显的水平方向
        Vector3 forward =
            Vector3.ProjectOnPlane(
                sceneCamera.transform.forward,
                Vector3.up
            );

        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }

        forward.Normalize();

        // 把整个 TaskRoot 放到头显正前方
        transform.position =
            sceneCamera.transform.position
            + forward * distanceFromCamera
            + Vector3.up * verticalOffset;

        transform.rotation =
            Quaternion.LookRotation(
                forward,
                Vector3.up
            );

        Debug.Log(
            "Task placed in front of the camera."
        );
    }

    private void OnDestroy()
    {
        foreach (XRInputSubsystem subsystem in inputSubsystems)
        {
            if (subsystem != null)
            {
                subsystem.trackingOriginUpdated -=
                    OnTrackingOriginUpdated;
            }
        }
    }
}