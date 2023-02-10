using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Room
{
    public string Id;
    public Vector3 RoomMarkerPosition;
    public Quaternion RoomMarkerRotation;
    public List<ReLocation> ReLocations;
}
