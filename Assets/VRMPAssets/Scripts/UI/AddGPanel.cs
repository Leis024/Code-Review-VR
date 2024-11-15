using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddGPanel : MonoBehaviour
{
     // Reference to the GitHub Panel prefab (Canvas)
    public GameObject gitHubPanelPrefab;

    // Reference to the VR player camera or headset (usually the main camera in VR)
   // public Transform vrPlayerCamera;
public Camera vrPlayerCamera;
    // Offset for distance in front of the player
    public float distanceInFrontOfPlayer = 2f;

    // Variable to hold the instantiated panel
    private GameObject currentGitHubPanel;

    // Method to be called when the button is clicked
    public void OnButtonPressed()
    { 
            // Instantiate the GitHubPanel prefab
            currentGitHubPanel = Instantiate(gitHubPanelPrefab);

            // Position the panel in front of the VR player
            PositionPanelInFrontOfPlayer();
        
    }

    // Position the panel directly in front of the VR player's camera
    private void PositionPanelInFrontOfPlayer()
    {
        // Get the forward direction of the VR player (camera's forward)
        Vector3 forwardDirection = vrPlayerCamera.transform.forward;

        // Calculate the new position in front of the player
        Vector3 newPosition = vrPlayerCamera.transform.position + forwardDirection * distanceInFrontOfPlayer;

        // Set the panel's position to this calculated position
        currentGitHubPanel.transform.position = newPosition;

      
    }

   
   
}

