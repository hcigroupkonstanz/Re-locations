using UnityEngine;
using Vuforia;
using HCIKonstanz.Colibri.Communication;
using HCIKonstanz.Colibri.Store;

public class Main : MonoBehaviour
{
    public static readonly string MOVEMENT_CHANNEL = "ReLMove";
    public static readonly string VOICE_CHANNEL = "ReLVoice";
    public static readonly string APP_NAME = "relocations";

    public static Main Instance = null;


    [Header("Manager")]
    public UIManager UIManager;
    public SpaceManager SpaceManager;
    public RemoteUserManager RemoteUserManager;
    public MovementBroadcast MovementBroadcast;
    public VoiceBroadcast VoiceBroadcast;
    public RoomCalibrationManager RoomCalibrationManager;
    public SpatialAwarenessManager SpatialAwarenessManager;

    [Header("Scene Objects")]
    public GameObject SceneContent;


    private RoomMarkerDetection roomMarkerDetection;

    void Awake()
    {
        // Create instance
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        GameObject mainCamera = Camera.main.gameObject;
        // Debug.Log("[Main] Position: " + mainCamera.transform.position + ", Rotation: " + mainCamera.transform.rotation.eulerAngles);

        UIManager.InMainMenu = true;

        // All ImageTargets must be enabled at start
        SpatialAwarenessManager.StartRoomMarkerDetection();
        RoomCalibrationManager.StartReLocationCalibration();
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
    }

    public void OnVuforiaStarted()
    {
        VuforiaARController.Instance.UnregisterVuforiaStartedCallback(OnVuforiaStarted);

        // Deactivate all ImageTargets
        SpatialAwarenessManager.StopRoomMarkerDetection();
        RoomCalibrationManager.StopReLocationCalibration();

        // Start room marker detection
        roomMarkerDetection = SpatialAwarenessManager.RoomMarkerDetection;
        StartRoomMarkerSearch();
    }

    public void StartRoomMarkerSearch()
    {
        // Stop broadcasting and visualizing remote users and unload room
        MovementBroadcast.StopBroadcast();
        VoiceBroadcast.StopBroadcast();
        RemoteUserManager.StopRemoteUserVisualization();
        SpaceManager.UnloadRoom();

        UIManager.InMainMenu = true;

        // Start room marker detection
        SpatialAwarenessManager.StartRoomMarkerDetection();
        RoomMarkerDetection.OnRoomMarkerDetected += OnRoomMarkerDetected;
    }

    public void StopRoomMarkerSearch()
    {
        // Stop room marker detection
        RoomMarkerDetection.OnRoomMarkerDetected -= OnRoomMarkerDetected;
        SpatialAwarenessManager.StopRoomMarkerDetection();
    }

    public void StartRoomCalibration()
    {
        UIManager.InMainMenu = false;
        StopRoomMarkerSearch();
        RoomCalibrationManager.CalibrateNewRoom();
    }

    private async void OnRoomMarkerDetected(string id, Vector3 position, Quaternion rotation)
    {
        // Debug.Log("[RoomMarker] Position: " + position + ", Rotation: " + rotation.eulerAngles);

        RoomMarkerDetection.OnRoomMarkerDetected -= OnRoomMarkerDetected;

        // UI
        UIManager.HideDashboard();
        UIManager.ShowInstructionElements();
        UIManager.InstructionTextMeshPro.SetText("Loading room. Please wait.");
        UIManager.InstructionSprite.sprite = UIManager.Sprites[2]; // 2 is waiting

        // Get room from server using REST Api
        Room room = await RestApi.Instance.Get<Room>(APP_NAME, id);
        if (room != null)
        {
            // Hide UI
            UIManager.InMainMenu = false;
            UIManager.HideDashboard();
            // Create downloaded room and Re-Locations
            SpaceManager.LoadRoom(room, position, rotation);
            // Broadcast movements and voice
            MovementBroadcast.StartBroadcast();
            VoiceBroadcast.StartBroadcast(MovementBroadcast.LocalUserInfo.Id);
            // Start visualizing remote users
            RemoteUserManager.StartRemoteUserVisualization();
        }
        else
        {
            // Adjust User Interface
            UIManager.HideDashboard();
            UIManager.ShowInstructionElements();
            UIManager.InstructionTextMeshPro.SetText("Room can not be loaded");
            UIManager.InstructionSprite.sprite = UIManager.Sprites[1];
            // Start new room marker search
            Invoke("StartRoomMarkerSearch", 2);
        }

    }
}
