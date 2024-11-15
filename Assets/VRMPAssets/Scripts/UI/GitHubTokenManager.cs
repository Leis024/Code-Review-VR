using UnityEngine;
using System.IO;
using System;

public class GitHubTokenManager : MonoBehaviour
{
    public string tokenFilePath = "token.txt";  // Path to the token file
    private string gitHubToken;
    public event Action OnTokenLoaded;

    private void Start()
    {
        // Load the GitHub token from the file
        LoadToken();
    }

    public void LoadToken()
    {
        // Load the token from the file if it exists
        try
        {
            string filePath;
             #if UNITY_EDITOR
                 filePath = Path.Combine(Application.dataPath, tokenFilePath);  // In editor, this points to Assets folder
             #else
                filePath = Path.Combine(Application.persistentDataPath, tokenFilePath);  // In build, use persistentDataPath
            #endif

            if (File.Exists(filePath))
            {
                gitHubToken = File.ReadAllText(filePath).Trim();  // Trim to remove any extra whitespace/newlines
                Debug.Log("GitHub token loaded MANAGER: " + gitHubToken);
                OnTokenLoaded?.Invoke();
            }
            else
            {
                Debug.Log("Token file not found at: " + filePath + " Token " + gitHubToken);
                OnTokenLoaded?.Invoke();
            }
        }
        catch (IOException e)
        {
            Debug.Log("Error reading the token file: " + e.Message);
        }
    }

    // Function to access the token for GitHub API calls
    public string GetToken()
    {Debug.Log("GitHub token loaded NEL METODO: " + gitHubToken);
        return gitHubToken;
    }
}
