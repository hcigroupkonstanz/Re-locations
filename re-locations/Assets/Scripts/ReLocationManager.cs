using UnityEngine;

public class ReLocationManager : MonoBehaviour
{
    [Header("Re-Location")]
    public ReLocation LoadedReLocation;

    [Header("Re-Location Elements")]
    public GameObject FloorBorder;
    public GameObject WorkArea;
    public GameObject Border;


    private MovementBroadcast movementBroadcast;

    void Start()
    {
        movementBroadcast = Main.Instance.MovementBroadcast;
    }

    public void LoadReLocation(ReLocation reLocation)
    {
        LoadedReLocation = reLocation;

        // Re-Location position, rotation and size
        transform.localPosition = new Vector3(LoadedReLocation.Position.x, 0, LoadedReLocation.Position.y);
        transform.localRotation = LoadedReLocation.Rotation;
        FloorBorder.transform.localScale = new Vector3(LoadedReLocation.Size.x, FloorBorder.transform.localScale.y, LoadedReLocation.Size.y);

        // Work area position, rotation and size
        WorkArea.transform.localRotation = LoadedReLocation.RotationWorkArea;
        WorkArea.transform.localPosition = LoadedReLocation.PositionWorkArea;
        WorkArea.transform.localScale = new Vector3(LoadedReLocation.SizeWorkArea.x, LoadedReLocation.SizeWorkArea.y, WorkArea.transform.localScale.z);

        // Border position, rotation and size
        Border.transform.localRotation = LoadedReLocation.RotationWorkArea;
        Border.transform.localPosition = LoadedReLocation.PositionWorkArea;
        Border.transform.localScale = new Vector3(LoadedReLocation.SizeWorkArea.x + 0.1f , LoadedReLocation.SizeWorkArea.y + 0.1f, Border.transform.localScale.z);

    }

    public void OnEnteredReLocation()
    {
        if (LoadedReLocation != null)
        {
            movementBroadcast.OnEnteredReLocation(gameObject);
        }
    }

    public void OnExitedReLocation()
    {
        if (LoadedReLocation != null)
        {
            movementBroadcast.OnExitedReLocation(gameObject);
        }
    }
}
