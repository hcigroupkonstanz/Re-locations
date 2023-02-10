using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserInfo
{
    // User id
    public short Id;

    // Room id the user is located
    public string RoomId;

    // Standing in Re-Locations type
    public string ReLocationType;

    // Relative to Re-Location position
    public Vector3 RelativeToReLocationPosition;
    
    // Looks at something
    public bool LookAtObject;

    // Looks at remote user
    public short LookAtRemoteUserId;
    public Vector3 RelativeToRemoteUserLookAtPosition;

    // Looks at other Re-Location
    public string LookAtReLocationType;
    public Vector3 RelativeToWorkAreaLookAtPosition;

}
