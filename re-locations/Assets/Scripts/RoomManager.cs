using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{

    [Header("Prefabs")]
    public GameObject ReLocationPrefab;

    [Header("Room")]
    public Room LoadedRoom;
    public Dictionary<string, GameObject> LoadedReLocationGameObjects;

    public void LoadRoom(Room room)
    {
        if (LoadedReLocationGameObjects == null) {
            LoadedReLocationGameObjects = new Dictionary<string, GameObject>();
        }
        LoadedRoom = room;
        foreach (ReLocation reLocation in LoadedRoom.ReLocations)
        {
            if (!LoadedReLocationGameObjects.ContainsKey(reLocation.Type))
            {
                GameObject loadedReLocationGameObject = Instantiate(ReLocationPrefab, transform);
                loadedReLocationGameObject.name = "[Re-Location] " + reLocation.Type;
                LoadedReLocationGameObjects.Add(reLocation.Type, loadedReLocationGameObject);
                loadedReLocationGameObject.GetComponent<ReLocationManager>().LoadReLocation(reLocation);
            }
        }
    }

}
