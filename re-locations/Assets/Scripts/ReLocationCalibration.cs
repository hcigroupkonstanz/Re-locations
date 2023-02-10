using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ReLocationCalibration : MonoBehaviour
{
    [Header("Calibration Settings")]
    public GameObject ReLocationCalibrationMarker;
    public float MinReLocationDepth = 1.5f;
    public float MinReLocationWidth = 2f;

    // The new Re-Location that should be generated
    [HideInInspector]
    public ReLocation NewReLocation;
    // Currently calibrating corner
    [HideInInspector]
    public bool CalibratingBottomLeft;

    private UIManager uiManager;

    // Waiting for user selecting the Re-Location type
    private bool WaitingForReLocationType = false;

    // Marker positions and rotations
    private Vector3 PositionBottomLeft;
    private Quaternion RotationBottomLeft;
    private Vector3 PositionTopRight;
    private Quaternion RotationTopRight;

    // Event handler
    public delegate void ReLocationCalibrationAction(ReLocation reLocation);
    public static event ReLocationCalibrationAction OnReLocationCalibrationFinished;

    void OnEnable()
    {
        uiManager = Main.Instance.UIManager;
        uiManager.HideDashboard();
        uiManager.ShowInstructionElements();
        uiManager.InstructionTextMeshPro.SetText("What type of Re-Location should be defined?");
        uiManager.InstructionSprite.sprite = uiManager.Sprites[2];
        uiManager.ReLocationTypeButtonBar.SetActive(true);
        WaitingForReLocationType = true;
        CalibratingBottomLeft = true;
    }

    void OnDisable()
    {
        // Disable Vuforia
        // VuforiaBehaviour.Instance.enabled = false;
    }

    public void OnReLocationTypeSelected(string type)
    {
        if (WaitingForReLocationType)
        {
            WaitingForReLocationType = false;
            NewReLocation = new ReLocation();
            NewReLocation.Type = type;

            // Enable Vuforia
            VuforiaBehaviour.Instance.enabled = true;
            TurnOnReLocationCalibrationMarker();
        }
    }

    public void ReLocationCalibrationMarkerDetected(Vector3 position, Quaternion rotation)
    {
        if (CalibratingBottomLeft)
        {
            CalibratingBottomLeft = false;
            PositionBottomLeft = position;
            RotationBottomLeft = rotation;

            // Turn the calibration marker off and on again
            ReLocationCalibrationMarker.SetActive(false);
            uiManager.HideDashboard();
            uiManager.ShowInstructionElements();
            uiManager.InstructionTextMeshPro.SetText("Bottom left calibration successful");
            uiManager.InstructionSprite.sprite = uiManager.Sprites[0];
            Invoke("TurnOnReLocationCalibrationMarker", 2);
        }
        else
        {
            PositionTopRight = position;
            RotationTopRight = rotation;

            CalculateReLocation();

            ReLocationCalibrationMarker.SetActive(false);
            uiManager.HideDashboard();
            uiManager.ShowInstructionElements();
            uiManager.InstructionTextMeshPro.SetText(NewReLocation.Type + " Re-Location calibration finished");
            uiManager.InstructionSprite.sprite = uiManager.Sprites[0];
            Invoke("Finish", 2);

            VuforiaBehaviour.Instance.enabled = false;
        }
    }

    private void CalculateReLocation()
    {
        // Calculate absolute rotation of work area
        List<Quaternion> rotations = new List<Quaternion> { RotationBottomLeft, RotationTopRight };
        Quaternion averageRotation = MathUtility.Average(rotations);
        Vector3 eulerAverageRotation = averageRotation.eulerAngles;

        // Calculate work area size
        Vector3 diagonalVector = PositionTopRight - PositionBottomLeft;
        Vector3 forwardVector = averageRotation * Vector3.forward;
        Vector3 horizontalVector = Vector3.Project(diagonalVector, forwardVector);
        Vector3 verticalVector = diagonalVector - horizontalVector;
        float width = horizontalVector.magnitude + 0.1f; // Add the width of the calibration marker (10cm)
        float height = verticalVector.magnitude + 0.1f;
        NewReLocation.SizeWorkArea = new Vector2(width, height);

        if (NewReLocation.Type == "Whiteboard" || NewReLocation.Type == "PCScreen")
        {
            // Calculate position of Re-Location
            Vector3 midpoint = PositionBottomLeft + diagonalVector * 0.5f;
            Vector3 upVector = averageRotation * Vector3.up;
            Vector3 reLocationPosition3D = midpoint + upVector * (MinReLocationDepth * 0.5f);
            Vector2 reLocationPosition = new Vector2(reLocationPosition3D.x, reLocationPosition3D.z);
            NewReLocation.Position = reLocationPosition;

            // Calculate rotation of Re-Location
            // Rotate by 90 degrees because the calibration marker is rotated by 90 degrees
            float yRotation = eulerAverageRotation.y + 90;
            if (yRotation >= 360)
            {
                yRotation -= 360;
            }
            Quaternion reLocationRotation = Quaternion.Euler(0, yRotation, 0);
            NewReLocation.Rotation = reLocationRotation;

            // Calculate size of Re-Location
            float reLocationWidth = width;
            if (reLocationWidth < MinReLocationWidth)
            {
                reLocationWidth = MinReLocationWidth;
            }
            Vector2 reLocationSize = new Vector2(reLocationWidth, MinReLocationDepth);
            NewReLocation.Size = reLocationSize;

            // Calculate position of work area
            Vector3 reLocationPositionWorkArea = new Vector3(0, midpoint.y - transform.parent.parent.position.y, MinReLocationDepth * -0.5f);
            NewReLocation.PositionWorkArea = reLocationPositionWorkArea;

            // Calculate rotation of work area
            NewReLocation.RotationWorkArea = Quaternion.identity;

        }
        else if (NewReLocation.Type == "Tabletop")
        {
            //TODO
        }
        else if (NewReLocation.Type == "Custom")
        {
            // Freaky position and rotation calculation needed
        }
    }

    private void TurnOnReLocationCalibrationMarker()
    {
        string markerPosition = "bottom left";
        Sprite instructionSprite = uiManager.Sprites[5];

        if (!CalibratingBottomLeft)
        {
            markerPosition = "top right";
            instructionSprite = uiManager.Sprites[6];
        }

        uiManager.HideDashboard();
        uiManager.ShowInstructionElements();
        uiManager.InstructionTextMeshPro.SetText("Hold the Calibration Marker at the " + markerPosition + " corner of the " + NewReLocation.Type + " and look at it");
        uiManager.InstructionSprite.sprite = instructionSprite;
        ReLocationCalibrationMarker.SetActive(true);
    }

    private void Finish()
    {
        uiManager.HideDashboard();
        OnReLocationCalibrationFinished?.Invoke(NewReLocation);
        gameObject.SetActive(false);
    }
}
