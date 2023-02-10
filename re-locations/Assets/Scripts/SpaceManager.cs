using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceManager : MonoBehaviour
{
    [Header("Loaded Room")]
    public GameObject LoadedRoomGameObject;
    public RoomManager LoadedRoomManager;

    [Header("Prefabs")]
    public GameObject RoomPrefab;


    private GameObject sceneContent;

    // Start is called before the first frame update
    void Start()
    {
        sceneContent = Main.Instance.SceneContent;
    }

    public void LoadRoom(Room room, Vector3 roomMarkerPosition, Quaternion roomMarkerRotation)
    {
        // Reset position and rotation of SceneContent
        sceneContent.transform.position = Vector3.zero;
        sceneContent.transform.rotation = Quaternion.identity;

        // Calculate rotation difference between saved room marker and current room marker and set it to the scene content
        Quaternion rotationDifference = roomMarkerRotation * Quaternion.Inverse(room.RoomMarkerRotation);
        sceneContent.transform.rotation = rotationDifference;

        // Calculate position difference between saved room marker and current room marker and set it to the scene content
        Vector3 savedRoomMarkerPosition = sceneContent.transform.TransformPoint(room.RoomMarkerPosition);
        Vector3 positionDifference = roomMarkerPosition - savedRoomMarkerPosition;

        sceneContent.transform.position = positionDifference;

        LoadedRoomGameObject = Instantiate(RoomPrefab, transform.parent);
        LoadedRoomGameObject.name = "[Room] " + room.Id;
        LoadedRoomManager = LoadedRoomGameObject.GetComponent<RoomManager>();
        LoadedRoomManager.LoadRoom(room);
    }

    public void UnloadRoom()
    {
        if (LoadedRoomGameObject != null)
        {
            Destroy(LoadedRoomGameObject);
            LoadedRoomManager = null;
        }
    }
}
