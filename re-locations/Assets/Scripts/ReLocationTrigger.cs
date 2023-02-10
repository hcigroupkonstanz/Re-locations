using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReLocationTrigger : MonoBehaviour
{

    private ReLocationManager reLocationManager;
    private string mainCameraName;

    void Start()
    {
        reLocationManager = transform.parent.parent.GetComponent<ReLocationManager>();
        mainCameraName = Camera.main.gameObject.transform.name;
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        if (otherCollider.name == mainCameraName)
        {
            Debug.Log("[Re-Location] Local user ENTERED Re-Location " + reLocationManager.LoadedReLocation.Type);
            reLocationManager.OnEnteredReLocation();
        }
    }

    private void OnTriggerExit(Collider otherCollider)
    {
        if (otherCollider.name == mainCameraName)
        {
            Debug.Log("[Re-Location] Local user has LEFT Re-Location " + reLocationManager.LoadedReLocation.Type);
            reLocationManager.OnExitedReLocation();
        }
    }
}
