using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject Dashboard;
    public TextMeshPro InstructionTextMeshPro;
    public SpriteRenderer InstructionSprite;
    public GameObject NewRoomButton;
    public GameObject DefineNewReLocationButton;
    public GameObject FinishRoomCalibrationButton;
    public GameObject ReLocationTypeButtonBar;

    [Header("UI Sprites")]
    public List<Sprite> Sprites;

    [Header("UI State")]
    public bool InMainMenu = true;

    public void HideDashboard()
    {
        // Hide all elements
        //InstructionCanvas.SetActive(false);
        InstructionTextMeshPro.gameObject.SetActive(false);
        InstructionSprite.gameObject.SetActive(false);
        NewRoomButton.SetActive(false);
        DefineNewReLocationButton.SetActive(false);
        FinishRoomCalibrationButton.SetActive(false);
        ReLocationTypeButtonBar.SetActive(false);
    }

    public void ShowInstructionElements()
    {
        InstructionTextMeshPro.gameObject.SetActive(true);
        InstructionSprite.gameObject.SetActive(true);
    }

}
