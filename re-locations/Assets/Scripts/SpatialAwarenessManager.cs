using UnityEngine;

public class SpatialAwarenessManager : MonoBehaviour
{
    [Header("Spatial Awareness Systems")]
    public FloorCalibration FloorCalibration;
    public RoomMarkerDetection RoomMarkerDetection;

    public void StartFloorCalibration()
    {
        FloorCalibration.gameObject.SetActive(true);
    }

    public void StopFloorCalibration()
    {
        FloorCalibration.gameObject.SetActive(false);
    }

    public void StartRoomMarkerDetection()
    {
        RoomMarkerDetection.gameObject.SetActive(true);
    }

    public void StopRoomMarkerDetection()
    {
        RoomMarkerDetection.gameObject.SetActive(false);
    }
}
