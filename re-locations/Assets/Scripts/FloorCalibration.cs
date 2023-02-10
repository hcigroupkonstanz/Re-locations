using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.Utilities;
using TMPro;
using UnityEngine;

public class FloorCalibration : MonoBehaviour, IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessMeshObject>
{
    [Header("Calibration Settings")]
    [Range(0f, 90f)]
    public float MinHeadRotation = 10f;

    [Range(0f, 1.5f)]
    public float MinDistanceToFloor = 1f;

    [Range(1, 240)]
    public int CountSamples = 120;


    private Camera mainCamera;
    private GameObject sceneContent;
    private UIManager uiManager;
    private List<float> floorPositions;
    private bool floorCalibrationFinished;
    private bool scanningRoomFinished = false;
    private IMixedRealitySpatialAwarenessSystem spatialAwarenessSystem;
    private IMixedRealitySpatialAwarenessSystem SpatialAwarenessSystem
    {
        get
        {
            if (spatialAwarenessSystem == null)
            {
                MixedRealityServiceRegistry.TryGetService<IMixedRealitySpatialAwarenessSystem>(out spatialAwarenessSystem);
            }
            return spatialAwarenessSystem;
        }
    }

    public delegate void FloorCalibrationAction();
    public static event FloorCalibrationAction OnFloorCalibrationFinished;

    private async void OnEnable()
    {
        mainCamera = Camera.main;
        sceneContent = Main.Instance.SceneContent;
        uiManager = Main.Instance.UIManager;
        floorPositions = new List<float>();
        floorCalibrationFinished = false;

        // UI
        uiManager.HideDashboard();
        uiManager.ShowInstructionElements();
        uiManager.InstructionTextMeshPro.SetText("Scanning room. Please look around.");
        uiManager.InstructionSprite.sprite = uiManager.Sprites[7];
        
        // Start spatial awareness system
        await new WaitUntil(() => SpatialAwarenessSystem != null);
        SpatialAwarenessSystem.ResumeObservers();
        SpatialAwarenessSystem.RegisterHandler<IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessMeshObject>>(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (!floorCalibrationFinished && mainCamera && scanningRoomFinished)
        {
            if (floorPositions.Count < CountSamples)
            {
                // Check if camera is looking down
                float cameraXAngle = mainCamera.transform.rotation.eulerAngles.x;
                if (cameraXAngle >= MinHeadRotation && cameraXAngle <= 90f)
                {
                    // Detect floor using Raycast
                    RaycastHit hit;
                    if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 4f))
                    {
                        // Check if hit point has min distance
                        float distanceToFloor = Camera.main.transform.position.y - hit.point.y;
                        if (distanceToFloor >= MinDistanceToFloor && distanceToFloor < 2.5f)
                        {
                            // Check normal of hit point
                            if (Vector3.Distance(hit.normal, Vector3.up) < 0.1f)
                            {
                                floorPositions.Add(hit.point.y);
                                int percent = (int)(floorPositions.Count * (100.0 / (double)CountSamples));
                                uiManager.InstructionTextMeshPro.SetText("Please hold still. Calibrating...\n" + percent + "%");
                                goto Successful;
                            }
                        }
                    }
                }
                uiManager.InstructionTextMeshPro.SetText("Please look at the floor while standing");
                uiManager.InstructionSprite.sprite = uiManager.Sprites[4];
            Successful:;
            }
            else
            {
                floorCalibrationFinished = true;

                // Stop spatial awareness system
                SpatialAwarenessSystem.SuspendObservers();

                // Calculate average floor positon
                float averageFloorPosition = floorPositions.Average();

                // Set floor position
                sceneContent.transform.position = new Vector3(sceneContent.transform.position.x, averageFloorPosition, sceneContent.transform.position.z);
                
                // Update UI
                uiManager.Dashboard.transform.position = mainCamera.transform.position + mainCamera.transform.forward;
                uiManager.InstructionTextMeshPro.SetText("Floor Calibration successful");
                uiManager.InstructionSprite.sprite = uiManager.Sprites[0];

                // Delete room model
                GameObject spatialAwarenessSystem = GameObject.Find("Spatial Awareness System");
                if (spatialAwarenessSystem)
                {
                    Destroy(spatialAwarenessSystem);
                }

                // Finish floor calibration in 2 seconds (UI is 2 seconds visible)
                Invoke("Finish", 2);
            }
        }
    }

    public void OnObservationAdded(MixedRealitySpatialAwarenessEventData<SpatialAwarenessMeshObject> eventData)
    {
        scanningRoomFinished = true;
        SpatialAwarenessSystem.UnregisterHandler<IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessMeshObject>>(this);
    }

    public void OnObservationRemoved(MixedRealitySpatialAwarenessEventData<SpatialAwarenessMeshObject> eventData)
    {

    }

    public void OnObservationUpdated(MixedRealitySpatialAwarenessEventData<SpatialAwarenessMeshObject> eventData)
    {

    }

    private void Finish()
    {
        uiManager.HideDashboard();
        OnFloorCalibrationFinished?.Invoke();
        gameObject.SetActive(false);
    }
}
