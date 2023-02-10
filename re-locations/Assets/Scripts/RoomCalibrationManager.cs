using System.Collections.Generic;
using UnityEngine;
using HCIKonstanz.Colibri.Communication;
using HCIKonstanz.Colibri.Store;

public class RoomCalibrationManager : MonoBehaviour
{
    public ReLocationCalibration ReLocationCalibration;


    private SpatialAwarenessManager spatialAwarenessManager;
    private UIManager uiManager;
    private MovementBroadcast movementBroadcast;
    private VoiceBroadcast voiceBroadcast;
    private SpaceManager spaceManager;
    private RemoteUserManager remoteUserManager;
    private FloorCalibration floorCalibration;
    private RoomMarkerDetection roomMarkerDetection;
    private Room newRoom;

    void Start()
    {
        spatialAwarenessManager = Main.Instance.SpatialAwarenessManager;
        uiManager = Main.Instance.UIManager;
        movementBroadcast = Main.Instance.MovementBroadcast;
        voiceBroadcast = Main.Instance.VoiceBroadcast;
        spaceManager = Main.Instance.SpaceManager;
        remoteUserManager = Main.Instance.RemoteUserManager;
        floorCalibration = spatialAwarenessManager.FloorCalibration;
        roomMarkerDetection = spatialAwarenessManager.RoomMarkerDetection;
    }

    public void StartReLocationCalibration()
    {
        ReLocationCalibration.gameObject.SetActive(true);
    }

    public void StopReLocationCalibration()
    {
        ReLocationCalibration.gameObject.SetActive(false);
    }

    public void CalibrateNewRoom()
    {
        Debug.Log("[RoomCalibrationManager] Start Room Calibration");

        // Create new room
        newRoom = new Room();

        // Start floor calibration
        spatialAwarenessManager.StartFloorCalibration();
        FloorCalibration.OnFloorCalibrationFinished += OnFloorCalibrationFinished;
    }

    public void OnFloorCalibrationFinished()
    {
        FloorCalibration.OnFloorCalibrationFinished -= OnFloorCalibrationFinished;

        // Start room marker detection
        spatialAwarenessManager.StartRoomMarkerDetection();
        RoomMarkerDetection.OnRoomMarkerDetected += OnRoomMarkerDetected;
    }

    private void OnRoomMarkerDetected(string name, Vector3 position, Quaternion rotation)
    {
        RoomMarkerDetection.OnRoomMarkerDetected -= OnRoomMarkerDetected;

        newRoom.Id = name;
        newRoom.RoomMarkerPosition = position - transform.parent.position;
        // Works only whene SceneContent was not rotated
        newRoom.RoomMarkerRotation = rotation;
        newRoom.ReLocations = new List<ReLocation>();

        // UI
        uiManager.HideDashboard();
        uiManager.ShowInstructionElements();
        uiManager.InstructionTextMeshPro.SetText("Room calibration successful");
        uiManager.InstructionSprite.sprite = uiManager.Sprites[0];

        Invoke("ShowCalibrateReLocationsMenu", 2);

    }

    public void DefineNewReLocation()
    {
        ReLocationCalibration.gameObject.SetActive(true);
        ReLocationCalibration.OnReLocationCalibrationFinished += OnReLocationCalibrationFinished;
    }

    private void OnReLocationCalibrationFinished(ReLocation reLocation)
    {
        ReLocationCalibration.OnReLocationCalibrationFinished -= OnReLocationCalibrationFinished;
        newRoom.ReLocations.Add(reLocation);

        ShowCalibrateReLocationsMenu();
    }

    public async void FinishRoomCalibration()
    {
        if (newRoom.ReLocations?.Count > 0)
        {
            uiManager.HideDashboard();
            uiManager.ShowInstructionElements();
            uiManager.InstructionTextMeshPro.SetText("Saving room. Please wait.");
            uiManager.InstructionSprite.sprite = uiManager.Sprites[2];

            // Save room on server
            bool successful = await RestApi.Instance.Put(Main.APP_NAME, newRoom.Id, newRoom);
            if (successful)
            {
                uiManager.HideDashboard();

                // Load room
                spaceManager.LoadRoom(newRoom, newRoom.RoomMarkerPosition + transform.parent.position, newRoom.RoomMarkerRotation);

                // Start broadcasting position and voice
                movementBroadcast.StartBroadcast();
                voiceBroadcast.StartBroadcast(movementBroadcast.LocalUserInfo.Id);

                // Start visualizing remote users
                remoteUserManager.StartRemoteUserVisualization();
            }
            else
            {
                // Adjust User Interface
                uiManager.HideDashboard();
                uiManager.ShowInstructionElements();
                uiManager.InstructionTextMeshPro.SetText("Saving room failed");
                uiManager.InstructionSprite.sprite = uiManager.Sprites[1];
                // Show Re-Locations calibration menu
                Invoke("ShowCalibrateReLocationsMenu", 2);
            }
        }
    }

    private void ShowCalibrateReLocationsMenu()
    {
        uiManager.HideDashboard();
        uiManager.ShowInstructionElements();
        uiManager.InstructionTextMeshPro.SetText("You have " + newRoom.ReLocations.Count + " Re-Locations in this room defined");
        uiManager.InstructionSprite.sprite = uiManager.Sprites[0];
        uiManager.DefineNewReLocationButton.SetActive(true);
        uiManager.FinishRoomCalibrationButton.SetActive(true);
    }
}
