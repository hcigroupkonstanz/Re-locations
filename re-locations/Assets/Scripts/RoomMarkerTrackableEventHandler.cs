using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class RoomMarkerTrackableEventHandler : MonoBehaviour, ITrackableEventHandler
{
    private TrackableBehaviour trackableBehaviour;
    private RoomMarkerDetection roomMarkerDetection;
    private bool isTrackingActive = false;
    private bool isLastFrameTrackingActive = true;
    private List<Vector3> positionSamples;
    private List<Quaternion> rotationSamples;
    private int maxSamples = 100;
    private int skipSamples = 10;
    private bool isCalibrationFinished;
    private GameObject progressCanvas;
    private UnityEngine.UI.Image progressImage;
    private UIManager uiManager;

    void OnEnable()
    {
        trackableBehaviour = GetComponent<TrackableBehaviour>();
        trackableBehaviour.RegisterTrackableEventHandler(this);
        roomMarkerDetection = transform.parent.GetComponent<RoomMarkerDetection>();
        positionSamples = new List<Vector3>();
        rotationSamples = new List<Quaternion>();
        skipSamples = 10;
        isLastFrameTrackingActive = true;
        isCalibrationFinished = false;
        progressCanvas = transform.Find("ProgressCanvas").gameObject;
        progressImage = transform.Find("ProgressCanvas/ProgressImage").GetComponent<UnityEngine.UI.Image>();
        progressImage.fillAmount = 0;
        uiManager = Main.Instance.UIManager;
        uiManager.HideDashboard();
    }

    void OnDisable()
    {
        trackableBehaviour.UnregisterTrackableEventHandler(this);
    }

    public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
    {
        isTrackingActive = (newStatus == TrackableBehaviour.Status.DETECTED || newStatus == TrackableBehaviour.Status.TRACKED);
        //if (newStatus == TrackableBehaviour.Status.DETECTED || newStatus == TrackableBehaviour.Status.TRACKED)
        //{
        //    RoomMarkerDetection?.RoomMarkerDetected(trackableBehaviour.TrackableName, transform.position);
        //}
    }

    void Update()
    {
        if (!isCalibrationFinished)
        {
            if (isTrackingActive)
            {
                if (positionSamples.Count >= maxSamples)
                {
                    isCalibrationFinished = true;

                    Vector3 actualPosition = MathUtility.Average(positionSamples);
                    Quaternion actualRotation = MathUtility.Average(rotationSamples);

                    progressCanvas.SetActive(false);

                    roomMarkerDetection?.RoomMarkerDetected(trackableBehaviour.TrackableName, actualPosition, actualRotation);
                }
                else
                {
                    if (skipSamples <= 0)
                    {
                        positionSamples.Add(transform.position);
                        rotationSamples.Add(transform.rotation);
                    }
                    else
                    {
                        skipSamples--;
                    }


                    // int percent = (int)(PositionSamples.Count * (100.0 / (double)MaxSamples));
                    float percent = (float)positionSamples.Count / (float)maxSamples;
                    // RoomMarkerDetection.UIManager.InstructionTextMeshPro.SetText("Please hold still. Calibrating: " + percent + "%");
                    uiManager.HideDashboard();
                    progressCanvas.SetActive(true);
                    progressImage.fillAmount = percent;
                }
            }
            else if (isLastFrameTrackingActive)
            {
                positionSamples = new List<Vector3>();
                rotationSamples = new List<Quaternion>();
                skipSamples = 10;

                progressCanvas.SetActive(false);
                uiManager.ShowInstructionElements();
                uiManager.InstructionTextMeshPro.SetText("Please look at the Room Marker");
                uiManager.InstructionSprite.sprite = uiManager.Sprites[3];
                if (Main.Instance.UIManager.InMainMenu)
                {
                    uiManager.NewRoomButton.SetActive(true);
                }
            }
            isLastFrameTrackingActive = isTrackingActive;
        }
    }
}
