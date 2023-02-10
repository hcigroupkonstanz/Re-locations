using System.Collections.Generic;
using UnityEngine;
using HCIKonstanz.Colibri.Communication;

public class RemoteUserManager : MonoBehaviour
{
    public Dictionary<int, GameObject> RemoteUserGameObjectsDictionary;

    [Header("Prefabs")]
    public GameObject RemoteUserAvatar;
    public float RemoteUserAvatarBodyHeight = 1.6f;

    [Header("Connection")]
    public float DisconnectUserAfterSeconds = 2f;

    private static readonly string DEBUG_HEADER = "[RemoteUser] ";
    private SyncCommands syncCommands;
    private SpaceManager spaceManager;
    private MovementBroadcast movementBroadcast;
    private GameObject mainCamera;
    private bool visualizeRemoteUsers = false;
    private string localRoomId = "";

    void OnEnable()
    {
        syncCommands = SyncCommands.Instance;
    }

    void OnDisable()
    {
        StopRemoteUserVisualization();
    }

    void Start()
    {
        spaceManager = Main.Instance.SpaceManager;
        movementBroadcast = Main.Instance.MovementBroadcast;
        mainCamera = Camera.main.gameObject;
    }

    void Update()
    {
        // Remove avatar when remote user sends no movement update
        if (visualizeRemoteUsers)
        {
            // Extra remove list needed because you can not remove items from a list while iterating over it
            List<int> removeIds = new List<int>();
            float currentTime = Time.time;
            foreach (GameObject remoteUserAvatar in RemoteUserGameObjectsDictionary.Values)
            {
                RemoteUserInfo remoteUserInfo = remoteUserAvatar.GetComponent<RemoteUserInfo>();
                if (currentTime - remoteUserInfo.LastHeartbeat > DisconnectUserAfterSeconds)
                {
                    removeIds.Add(remoteUserInfo.UserInfo.Id);
                }
            }
            for (int i = 0; i < removeIds.Count; i++)
            {
                GameObject remoteUserAvatar = RemoteUserGameObjectsDictionary[removeIds[i]];
                RemoteUserGameObjectsDictionary.Remove(removeIds[i]);
                VoiceReceiver voiceReceiver = remoteUserAvatar.GetComponentInChildren(typeof(VoiceReceiver)) as VoiceReceiver;
                voiceReceiver.StopPlayback();
                Destroy(remoteUserAvatar);
                Debug.Log(DEBUG_HEADER + "Remote user with ID " + removeIds[i] + " has LEFT the session");
            }
        }
    }

    public void AddUser(UserInfo userInfo)
    {
        if (!RemoteUserGameObjectsDictionary.ContainsKey(userInfo.Id))
        {
            // Instantiate remote user avatar
            GameObject remoteUserAvatar = Instantiate(RemoteUserAvatar, transform.parent);
            RemoteUserInfo remoteUserInfo = remoteUserAvatar.GetComponent<RemoteUserInfo>();
            remoteUserAvatar.name = "[RemoteUser] " + userInfo.Id;

            // Add remote user avatar GameObject to dictionary
            RemoteUserGameObjectsDictionary.Add(userInfo.Id, remoteUserAvatar);

            // Set remote user info
            remoteUserInfo.UserInfo = userInfo;
            remoteUserInfo.LastHeartbeat = Time.time;

            // Start voice playback
            VoiceReceiver voiceReceiver = remoteUserAvatar.GetComponentInChildren(typeof(VoiceReceiver)) as VoiceReceiver;
            voiceReceiver.StartPlayback(userInfo.Id);

            // Calculate avatar position and rotation
            UpdateUser(userInfo);

            Debug.Log(DEBUG_HEADER + "Remote user with ID " + userInfo.Id + " JOINED the session");
        }
        else
        {
            Debug.LogError(DEBUG_HEADER + "AddUser: GameObject with the id " + userInfo.Id + " already available");
        }
    }

    public void UpdateUser(UserInfo userInfo)
    {
        if (RemoteUserGameObjectsDictionary.ContainsKey(userInfo.Id))
        {
            GameObject remoteUserAvatar = RemoteUserGameObjectsDictionary[userInfo.Id];
            RemoteUserInfo remoteUserInfo = remoteUserAvatar.GetComponent<RemoteUserInfo>();
            AvatarControl avatarControl = remoteUserAvatar.GetComponent<AvatarControl>();

            // Update remote user info
            remoteUserInfo.UserInfo = userInfo;
            remoteUserInfo.LastHeartbeat = Time.time;

            // Make avatar (in)visible when not standing inside Re-Location or Re-Location is not available in this room
            if (userInfo.ReLocationType == "" || !spaceManager.LoadedRoomManager.LoadedReLocationGameObjects.ContainsKey(userInfo.ReLocationType))
            {
                avatarControl.AvatarVisible = false;
                CapsuleCollider remoteUserCapsuleCollider = remoteUserAvatar.GetComponent<CapsuleCollider>();
                if (remoteUserCapsuleCollider.enabled) remoteUserCapsuleCollider.enabled = false;
                return;
            }
            else
            {
                avatarControl.AvatarVisible = true;
                CapsuleCollider remoteUserCapsuleCollider = remoteUserAvatar.GetComponent<CapsuleCollider>();
                if (!remoteUserCapsuleCollider.enabled) remoteUserCapsuleCollider.enabled = true;
            }

            // Get Re-Location and calculate relative position of the avatar
            GameObject ReLocationGameObject = spaceManager.LoadedRoomManager.LoadedReLocationGameObjects[userInfo.ReLocationType];
            Vector3 relativeToReLocationPositionBottom = new Vector3(userInfo.RelativeToReLocationPosition.x,
                                                                     userInfo.RelativeToReLocationPosition.y - (RemoteUserAvatarBodyHeight - 0.1f),
                                                                     userInfo.RelativeToReLocationPosition.z);
            Vector3 avatarWorldPosition = ReLocationGameObject.transform.TransformPoint(relativeToReLocationPositionBottom);

            Vector3 avatarWorldLookAtPosition = Vector3.zero;
            bool avatarLookAtLocalUser = false;
            // Remote user is looking at an other Re-Location than standing
            if (userInfo.LookAtReLocationType != "" && spaceManager.LoadedRoomManager.LoadedReLocationGameObjects.ContainsKey(userInfo.LookAtReLocationType))
            {
                // ATTENTION: Without Re-Location and gaze scaling, only 1:1
                // GameObject lookAtReLocationGameObject = spaceManager.LoadedRoomManager.LoadedReLocationGameObjects[userInfo.LookAtReLocationType];
                // avatarWorldLookAtPosition = lookAtReLocationGameObject.transform.TransformPoint(userInfo.RelativeToWorkAreaLookAtPosition);

                // ATTENTION: Without Re-Location scaling, but gaze scaling relative to work area
                GameObject lookAtReLocationGameObject = spaceManager.LoadedRoomManager.LoadedReLocationGameObjects[userInfo.LookAtReLocationType];
                GameObject lookAtReLocationWorkArea = lookAtReLocationGameObject.GetComponent<ReLocationManager>().WorkArea;
                avatarWorldLookAtPosition = lookAtReLocationWorkArea.transform.TransformPoint(userInfo.RelativeToWorkAreaLookAtPosition);
            }
            // Remote user is looking at an other remote user or at me
            else if (userInfo.LookAtRemoteUserId != -1 && (RemoteUserGameObjectsDictionary.ContainsKey(userInfo.LookAtRemoteUserId) || userInfo.LookAtRemoteUserId == movementBroadcast.LocalUserInfo.Id))
            {
                // Remote user is looking at me
                if (userInfo.LookAtRemoteUserId == movementBroadcast.LocalUserInfo.Id)
                {
                    Vector3 mainCameraCoordinate = mainCamera.transform.TransformPoint(userInfo.RelativeToRemoteUserLookAtPosition);
                    mainCameraCoordinate.y -= RemoteUserAvatarBodyHeight - 0.1f;
                    avatarWorldLookAtPosition = mainCameraCoordinate;
                    avatarLookAtLocalUser = true;
                }
                // Remote user is looking at an other remote user
                else if (RemoteUserGameObjectsDictionary.ContainsKey(userInfo.LookAtRemoteUserId))
                {
                    GameObject lookAtRemoteUserGameObject = RemoteUserGameObjectsDictionary[userInfo.LookAtRemoteUserId];
                    avatarWorldLookAtPosition = lookAtRemoteUserGameObject.transform.TransformPoint(userInfo.RelativeToRemoteUserLookAtPosition);
                }
            }
            else
            {
                // ATTENTION: This does not work when the remote Re-location (the user is looking at) is not available locally
                if (userInfo.LookAtReLocationType == "")
                {
                    avatarWorldLookAtPosition = ReLocationGameObject.transform.TransformPoint(userInfo.RelativeToWorkAreaLookAtPosition);
                }
                // WORKAROUND: Do not update "look at" position
                else
                {
                    avatarWorldLookAtPosition = avatarControl.LookAtPosition;
                }
            }

            // Set position, "look at" position, and gaze visibility
            avatarControl.Position = avatarWorldPosition;
            avatarControl.LookAtPosition = avatarWorldLookAtPosition;
            avatarControl.LookAtLocalUser = avatarLookAtLocalUser;
            avatarControl.GazeVisible = userInfo.LookAtObject;
        }
        else
        {
            Debug.LogError(DEBUG_HEADER + "UpdateUser: No GameObject with the id " + userInfo.Id + " available");
        }
    }

    public void StartRemoteUserVisualization()
    {
        if (!visualizeRemoteUsers)
        {
            localRoomId = Main.Instance.SpaceManager.LoadedRoomManager.LoadedRoom.Id;
            RemoteUserGameObjectsDictionary = new Dictionary<int, GameObject>();
            syncCommands.AddStringListener(Main.MOVEMENT_CHANNEL, OnStringMessage);
            visualizeRemoteUsers = true;
        }
    }

    public void StopRemoteUserVisualization()
    {
        if (visualizeRemoteUsers)
        {
            visualizeRemoteUsers = false;
            syncCommands.RemoveStringListener(Main.MOVEMENT_CHANNEL, OnStringMessage);
            foreach (GameObject remoteUserGameObject in RemoteUserGameObjectsDictionary.Values)
            {
                VoiceReceiver voiceReceiver = remoteUserGameObject.GetComponentInChildren(typeof(VoiceReceiver)) as VoiceReceiver;
                voiceReceiver.StopPlayback();
                Destroy(remoteUserGameObject);
            }
            RemoteUserGameObjectsDictionary = new Dictionary<int, GameObject>();
        }
    }

    public void OnRemoteUserMovementUpdate(UserInfo remoteUser)
    {
        // Only show avatar when user is remote (not in the same room)
        if (remoteUser.RoomId != localRoomId)
        {
            if (RemoteUserGameObjectsDictionary.ContainsKey(remoteUser.Id))
            {
                UpdateUser(remoteUser);
            }
            else
            {
                AddUser(remoteUser);
            }
        }
    }

    private void OnStringMessage(string val)
    {
        UserInfo remoteUser = JsonUtility.FromJson<UserInfo>(val);
        if (remoteUser != null)
        {
            OnRemoteUserMovementUpdate(remoteUser);
        }
    }

}
