using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class RoomMarkerDetection : MonoBehaviour
{
    public delegate void RoomMarkerDetectionAction(string id, Vector3 position, Quaternion rotation);
    public static event RoomMarkerDetectionAction OnRoomMarkerDetected;


    private string roomMarkerId;
    private Vector3 roomMarkerPosition;
    private Quaternion roomMarkerRotation;

    void OnEnable()
    {
        // Enable Vuforia
        VuforiaBehaviour.Instance.enabled = true;
    }

    void OnDisable()
    {
        // Disable Vuforia
        // VuforiaBehaviour.Instance.enabled = false;
    }

    public void RoomMarkerDetected(string id, Vector3 position, Quaternion rotation)
    {
        Debug.Log("[RoomMarkerDetection] Room Marker Detected: " + id + " " + position);

        OnRoomMarkerDetected?.Invoke(id, position, rotation);

        VuforiaBehaviour.Instance.enabled = false;

        gameObject.SetActive(false);
    }
}
