using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementBroadcast : MonoBehaviour
{
    [Header("Current Local User Status")]
    public List<GameObject> InsideReLocationGameObjects;
    public UserInfo LocalUserInfo;


    private GameObject mainCamera;
    private SyncCommands syncCommands;
    private bool broadcast = false;

    void Start()
    {
        // Get main camera object
        mainCamera = Camera.main.gameObject;

        // If local id not set manually create random id
        if (LocalUserInfo.Id == 0)
        {
            LocalUserInfo.Id = (short)UnityEngine.Random.Range(1, 32000);
        }

        // Create list for Re-Locations the user currently standing inside
        InsideReLocationGameObjects = new List<GameObject>();

        syncCommands = SyncCommands.Instance;
    }

    void Update()
    {
        if (broadcast)
        {
            // Get the positon at the work area the user is looking at using raycast
            RaycastHit raycastHit = new RaycastHit();
            bool hitWorkArea = false;
            bool hitRemoteUser = false;
            if (InsideReLocationGameObjects.Count > 0)
            {
                if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out raycastHit, 10f, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (raycastHit.transform.name == "WorkArea")
                    {
                        hitWorkArea = true;
                    }
                    else if (raycastHit.transform.name.Contains("RemoteUser"))
                    {
                        hitRemoteUser = true;
                    }
                }
            }

            // When standing outside Re-Locations
            if (InsideReLocationGameObjects.Count == 0)
            {
                LocalUserInfo.ReLocationType = "";
                LocalUserInfo.RelativeToReLocationPosition = Vector3.zero;
                LocalUserInfo.LookAtObject = false;
                LocalUserInfo.LookAtRemoteUserId = -1;
                LocalUserInfo.RelativeToRemoteUserLookAtPosition = Vector3.zero;
                LocalUserInfo.LookAtReLocationType = "";
                LocalUserInfo.RelativeToWorkAreaLookAtPosition = Vector3.zero;
            }
            // When standing inside exactly one Re-Location
            else if (InsideReLocationGameObjects.Count == 1)
            {
                GameObject insideReLocationGameObject = InsideReLocationGameObjects[0];
                ReLocation insideReLocation = insideReLocationGameObject.GetComponent<ReLocationManager>().LoadedReLocation;

                // ---------- Position ----------
                LocalUserInfo.ReLocationType = insideReLocation.Type;
                // Calculate position of camera relative to Re-Location using InverseTransformPoint (converting global position to local position)
                LocalUserInfo.RelativeToReLocationPosition = insideReLocationGameObject.transform.InverseTransformPoint(mainCamera.transform.position);

                // ---------- Viewing direction ----------
                // User is looking at a work area
                if (hitWorkArea)
                {
                    // ATTENTION: Without Re-Location and gaze scaling, only 1:1
                    // GameObject lookAtReLocationGameObject = raycastHit.transform.parent.gameObject;
                    // ReLocation lookAtReLocation = lookAtReLocationGameObject.GetComponent<ReLocationManager>().LoadedReLocation;
                    // LocalUserInfo.LookAtObject = true;
                    // LocalUserInfo.LookAtReLocationType = lookAtReLocation.Type;
                    // LocalUserInfo.RelativeToWorkAreaLookAtPosition = lookAtReLocationGameObject.transform.InverseTransformPoint(raycastHit.point);
                    // LocalUserInfo.LookAtRemoteUserId = -1;
                    // LocalUserInfo.RelativeToRemoteUserLookAtPosition = Vector3.zero;

                    // ATTENTION: Without Re-Location scaling, but gaze scaling relative to work area
                    GameObject lookAtReLocationGameObject = raycastHit.transform.parent.gameObject;
                    ReLocation lookAtReLocation = lookAtReLocationGameObject.GetComponent<ReLocationManager>().LoadedReLocation;
                    GameObject lookAtWorkAreaGameObject = raycastHit.transform.gameObject;
                    LocalUserInfo.LookAtObject = true;
                    LocalUserInfo.LookAtReLocationType = lookAtReLocation.Type;
                    LocalUserInfo.RelativeToWorkAreaLookAtPosition = lookAtWorkAreaGameObject.transform.InverseTransformPoint(raycastHit.point);
                    LocalUserInfo.LookAtRemoteUserId = -1;
                    LocalUserInfo.RelativeToRemoteUserLookAtPosition = Vector3.zero;
                }
                // User is looking at a remote user
                else if (hitRemoteUser)
                {
                    GameObject lookAtRemoteUserGameObject = raycastHit.transform.gameObject;
                    RemoteUserInfo remoteUserInfo = lookAtRemoteUserGameObject.GetComponent<RemoteUserInfo>();
                    LocalUserInfo.LookAtObject = true;
                    LocalUserInfo.LookAtRemoteUserId = remoteUserInfo.UserInfo.Id;
                    LocalUserInfo.RelativeToRemoteUserLookAtPosition = lookAtRemoteUserGameObject.transform.InverseTransformPoint(raycastHit.point);
                    LocalUserInfo.LookAtReLocationType = "";
                    LocalUserInfo.RelativeToWorkAreaLookAtPosition = Vector3.zero;
                }
                // User is not looking at a work area or a remote user
                else
                {
                    LocalUserInfo.LookAtObject = false;
                    LocalUserInfo.LookAtReLocationType = "";
                    LocalUserInfo.RelativeToWorkAreaLookAtPosition = insideReLocationGameObject.transform.InverseTransformPoint(mainCamera.transform.position + (mainCamera.transform.forward * 5f));
                    LocalUserInfo.LookAtRemoteUserId = -1;
                    LocalUserInfo.RelativeToRemoteUserLookAtPosition = Vector3.zero;
                }
            }
            // When standing inside two or more Re-Locations at the same time
            else if (InsideReLocationGameObjects.Count >= 2)
            {
                GameObject insideReLocationGameObject = null;
                ReLocation insideReLocation = null;

                // User is looking at a work area
                if (hitWorkArea)
                {
                    // ---------- Viewing direction ----------
                    // ATTENTION: Without Re-Location and gaze scaling, only 1:1
                    // GameObject lookAtReLocationGameObject = raycastHit.transform.parent.gameObject;
                    // ReLocation lookAtReLocation = lookAtReLocationGameObject.GetComponent<ReLocationManager>().LoadedReLocation;
                    // LocalUserInfo.LookAtObject = true;
                    // LocalUserInfo.LookAtReLocationType = lookAtReLocation.Type;
                    // LocalUserInfo.RelativeToWorkAreaLookAtPosition = lookAtReLocationGameObject.transform.InverseTransformPoint(raycastHit.point);
                    // LocalUserInfo.LookAtRemoteUserId = -1;
                    // LocalUserInfo.RelativeToRemoteUserLookAtPosition = Vector3.zero;

                    // ATTENTION: Without Re-Location scaling, but gaze scaling relative to work area
                    GameObject lookAtReLocationGameObject = raycastHit.transform.parent.gameObject;
                    ReLocation lookAtReLocation = lookAtReLocationGameObject.GetComponent<ReLocationManager>().LoadedReLocation;
                    GameObject lookAtWorkAreaGameObject = raycastHit.transform.gameObject;
                    LocalUserInfo.LookAtObject = true;
                    LocalUserInfo.LookAtReLocationType = lookAtReLocation.Type;
                    LocalUserInfo.RelativeToWorkAreaLookAtPosition = lookAtWorkAreaGameObject.transform.InverseTransformPoint(raycastHit.point);
                    LocalUserInfo.LookAtRemoteUserId = -1;
                    LocalUserInfo.RelativeToRemoteUserLookAtPosition = Vector3.zero;

                    // ---------- Position ----------
                    // When "Look at Re-Location" is one of the "Inside Re-Locations" use this Re-Location
                    foreach (GameObject reLocationGameObject in InsideReLocationGameObjects)
                    {
                        ReLocation reLocation = reLocationGameObject.GetComponent<ReLocationManager>().LoadedReLocation;
                        if (lookAtReLocation.Equals(reLocation))
                        {
                            insideReLocationGameObject = reLocationGameObject;
                            insideReLocation = lookAtReLocation;
                            break;
                        }
                    }
                }
                // User is looking at a remote user
                else if (hitRemoteUser)
                {
                    GameObject lookAtRemoteUserGameObject = raycastHit.transform.gameObject;
                    RemoteUserInfo remoteUserInfo = lookAtRemoteUserGameObject.GetComponent<RemoteUserInfo>();
                    LocalUserInfo.LookAtObject = true;
                    LocalUserInfo.LookAtRemoteUserId = remoteUserInfo.UserInfo.Id;
                    LocalUserInfo.RelativeToRemoteUserLookAtPosition = lookAtRemoteUserGameObject.transform.InverseTransformPoint(raycastHit.point);
                    LocalUserInfo.LookAtReLocationType = "";
                    LocalUserInfo.RelativeToWorkAreaLookAtPosition = Vector3.zero;
                }

                // The "Look at Re-Location" is not one of the "Inside Re-Locations" or the user is not looking at a Re-Location at all
                if (insideReLocation == null)
                {
                    // ---------- Position ----------
                    float smallestAngle = 181f;
                    GameObject smallestAngleReLocationGameObject = null;
                    ReLocation smallestAngleReLocation = null;
                    foreach (GameObject reLocationGameObject in InsideReLocationGameObjects)
                    {
                        ReLocationManager reLocationManager = reLocationGameObject.GetComponent<ReLocationManager>();
                        ReLocation reLocation = reLocationManager.LoadedReLocation;
                        GameObject workArea = reLocationManager.WorkArea;

                        // Calculate two bounds of the Re-Location work area (theoretically all 4 edges of the work area should be calculated to support tabletops, ...)
                        Vector3 leftBound = workArea.transform.position + workArea.transform.right * (reLocation.SizeWorkArea.x * 0.5f);
                        Vector3 rightBound = workArea.transform.position + (workArea.transform.right * -1f) * (reLocation.SizeWorkArea.x * 0.5f);

                        // Calculate angles to bounds
                        float angleLeftBound = Mathf.Abs(Vector3.SignedAngle(leftBound - mainCamera.transform.position, mainCamera.transform.forward, Vector3.up));
                        if (angleLeftBound < smallestAngle)
                        {
                            smallestAngle = angleLeftBound;
                            smallestAngleReLocationGameObject = reLocationGameObject;
                            smallestAngleReLocation = reLocation;
                        }
                        float angleRightBound = Mathf.Abs(Vector3.SignedAngle(rightBound - mainCamera.transform.position, mainCamera.transform.forward, Vector3.up));
                        if (angleRightBound < smallestAngle)
                        {
                            smallestAngle = angleRightBound;
                            smallestAngleReLocationGameObject = reLocationGameObject;
                            smallestAngleReLocation = reLocation;
                        }
                    }
                    insideReLocationGameObject = smallestAngleReLocationGameObject;
                    insideReLocation = smallestAngleReLocation;

                    // ---------- Viewing direction ----------
                    // User is not looking at a work area or a remote user
                    if (!hitWorkArea && !hitRemoteUser)
                    {
                        LocalUserInfo.LookAtObject = false;
                        LocalUserInfo.LookAtReLocationType = "";
                        LocalUserInfo.RelativeToWorkAreaLookAtPosition = insideReLocationGameObject.transform.InverseTransformPoint(mainCamera.transform.position + (mainCamera.transform.forward * 5f));
                        LocalUserInfo.LookAtRemoteUserId = -1;
                        LocalUserInfo.RelativeToRemoteUserLookAtPosition = Vector3.zero;
                    }
                }

                // ---------- Position ----------
                LocalUserInfo.ReLocationType = insideReLocation.Type;
                LocalUserInfo.RelativeToReLocationPosition = insideReLocationGameObject.transform.InverseTransformPoint(mainCamera.transform.position);

            }

            SendLocalUserMovement();
        }
    }

    public void StartBroadcast()
    {
        // Set room id to loaded room
        LocalUserInfo.RoomId = Main.Instance.SpaceManager.LoadedRoomManager.LoadedRoom.Id;
        broadcast = true;
    }

    public void StopBroadcast()
    {
        broadcast = false;
    }

    public void OnEnteredReLocation(GameObject reLocationGameObject)
    {
        InsideReLocationGameObjects.Add(reLocationGameObject);
    }

    public void OnExitedReLocation(GameObject reLocationGameObject)
    {
        InsideReLocationGameObjects.Remove(reLocationGameObject);
    }

    private void SendLocalUserMovement()
    {
        syncCommands.SendData(Main.MOVEMENT_CHANNEL, JsonUtility.ToJson(LocalUserInfo));
    }
}
