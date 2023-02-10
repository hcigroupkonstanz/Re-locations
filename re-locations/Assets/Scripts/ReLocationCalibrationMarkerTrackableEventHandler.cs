using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ReLocationCalibrationMarkerTrackableEventHandler : MonoBehaviour, ITrackableEventHandler
{
    private UIManager uiManager;
    private TrackableBehaviour trackableBehaviour;
    private ReLocationCalibration reLocationCalibration;
    private bool isTrackingActive;
    private List<Vector3> positionSamples;
    private List<Quaternion> rotationSamples;
    private int maxSamples = 100;
    private bool isCalibrationFinished;
    private GameObject progressCanvas;
    private UnityEngine.UI.Image progressImage;

    void OnEnable()
    {
        uiManager = Main.Instance.UIManager;
        trackableBehaviour = GetComponent<TrackableBehaviour>();
        trackableBehaviour.RegisterTrackableEventHandler(this);
        reLocationCalibration = Main.Instance.RoomCalibrationManager.ReLocationCalibration;
        positionSamples = new List<Vector3>();
        rotationSamples = new List<Quaternion>();
        isCalibrationFinished = false;
        progressCanvas = transform.Find("ProgressCanvas").gameObject;
        progressImage = transform.Find("ProgressCanvas/ProgressImage").GetComponent<UnityEngine.UI.Image>();
        progressImage.fillAmount = 0;
    }

    void OnDisable()
    {
        trackableBehaviour.UnregisterTrackableEventHandler(this);
    }

    public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
    {
        isTrackingActive = (newStatus == TrackableBehaviour.Status.DETECTED || newStatus == TrackableBehaviour.Status.TRACKED);
    }

    void Update()
    {
        if (!isCalibrationFinished)
        {
            if (isTrackingActive)
            {
                // Check if user looks at the marker
                RaycastHit raycastHit;
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out raycastHit, 2f) && raycastHit.transform.name == "ReLocationCalibrationCube")
                {
                    if (positionSamples.Count >= maxSamples)
                    {
                        isCalibrationFinished = true;

                        Vector3 actualPosition = MathUtility.Average(positionSamples);
                        Quaternion actualRotation = MathUtility.Average(rotationSamples);

                        reLocationCalibration?.ReLocationCalibrationMarkerDetected(actualPosition, actualRotation);
                    }
                    else
                    {
                        positionSamples.Add(transform.position);
                        rotationSamples.Add(transform.rotation);

                        // int percent = (int)(PositionSamples.Count * (100.0 / (double)MaxSamples));
                        float percent = (float)positionSamples.Count / (float)maxSamples;
                        // RoomMarkerDetection.UIManager.InstructionTextMeshPro.SetText("Please hold still. Calibrating: " + percent + "%");
                        uiManager.HideDashboard();
                        progressCanvas.SetActive(true);
                        progressImage.fillAmount = percent;
                    }
                    return;
                }

                positionSamples = new List<Vector3>();
                rotationSamples = new List<Quaternion>();

                string markerPosition = "bottom left";
                Sprite instructionSprite = uiManager.Sprites[5];

                if (!reLocationCalibration.CalibratingBottomLeft)
                {
                    markerPosition = "top right";
                    instructionSprite = uiManager.Sprites[6];
                }

                progressCanvas.SetActive(false);
                uiManager.ShowInstructionElements();
                uiManager.InstructionTextMeshPro.SetText("Hold the Calibration Marker at the " + markerPosition + " corner of the " + reLocationCalibration.NewReLocation.Type + " and look at it");
                uiManager.InstructionSprite.sprite = instructionSprite;
            }
        }
    }
}
