using UnityEngine;
using TMPro;
using System.Collections;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using Codice.Client.Common.GameUI;
using XRMultiplayer;
using UnityEngine.Assertions.Must;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Services.Lobbies.Models;
using System.Text;
using UnityEngine.UI;


public class GitHubAPIManager : MonoBehaviour
{
    public GameObject GitHubPanel;
    public GitHubAPIHelper gitHubAPIHelper;  // Reference to the helper
    public TMP_InputField ownerNameInput;    // Input field for Owner Name
    public TMP_InputField repoNameInput;     // Input field for Repository Name
    public TextMeshProUGUI fileText;         // Display file or directory info
    public Transform contentDirectory;       // Where file/folder items will be displayed
    public GameObject clickableLabelPrefab;  // Prefab for file/folder item UI
    public TextMeshProUGUI pathName;
    public Transform contentFile;
    private string currentDir;
    string endpoint;
    private string fullPath;
    public GitHubTokenManager tokenManager;
    private string token;
    private GameObject fileTextObj;
    public TMP_InputField inputFieldComponent;
    private TextMeshProUGUI textComponent;
    public Toggle myEditButton;
    public Button CommitButton;
    private string currentFilePath;
    private string currentFileSHA;  // Store the file SHA for committing changes
    private string originalFileContent;
    private string repoMetadataUrl;
    public TextMeshProUGUI commitResult;

    private void Start()
    {        
        tokenManager.OnTokenLoaded += OnTokenLoaded;
        myEditButton.gameObject.SetActive(false);
        CommitButton.gameObject.SetActive(false);
      
    }

private void OnTokenLoaded()
{
    token = tokenManager.GetToken();
    string url = "https://api.github.com/user/repos?visibility=all";
    UnityWebRequest request = UnityWebRequest.Get(url);
    Debug.Log("GitHub API Response FULL URL: " + url + " TOKEN = " + token);

    // Set the authorization header
    request.SetRequestHeader("Authorization", "token " + token);
    request.SetRequestHeader("User-Agent", "UnityVRApp");

    // Start the web request as a coroutine
    StartCoroutine(SendGitHubRequest(request));
}

private IEnumerator SendGitHubRequest(UnityWebRequest request)
{
    // Wait for the request to complete
    yield return request.SendWebRequest();

    // Check for errors
    if (request.result == UnityWebRequest.Result.Success)
    {
        // Parse the response if successful
        RepositoryInfo[] repos = JsonHelper.GetArray<RepositoryInfo>(request.downloadHandler.text);
        DisplayRepositories(repos);
       
        string nome = XRINetworkPlayer.LocalPlayer.playerName;
        //Debug.Log("Nome Player " + nome);
        fullPath = $"/repos/{nome}/"; // A quanto pare non serve, ma lo lascio lo stesso, non si sa mai
        
    }
    else
    {
        Debug.Log("GitHub API request failed: " + request.error);
    }
}

    // Called when the search button is pressed
    public void OnSearchButtonPressed()
    {
        string ownerName = ownerNameInput.text.Trim();
        string repoName = repoNameInput.text.Trim();

        if (string.IsNullOrEmpty(ownerName))
        {
            Debug.Log("Owner name is required.");
            return;
        }

        
        if (string.IsNullOrWhiteSpace(repoName))
        {
            // If no repo name, list all repositories of the owner
            endpoint = $"/users/{ownerName}/repos";
            fullPath = $"/repos/{ownerName}/";
        }
        else
        {
            // If both owner and repo name are provided, fetch contents of that repository
            endpoint = $"/repos/{ownerName}/{repoName}/contents/";
        }

        // Start the GitHub API request
        StartCoroutine(gitHubAPIHelper.MakeGitHubRequest(endpoint,
            onSuccess: (response) =>
            {
                //Debug.Log("GitHub API Response: " + response);
                //Debug.Log("ENDPOINT: " + endpoint);
                if (string.IsNullOrEmpty(repoName))
                {
                    // Parse and display the list of repositories
                    RepositoryInfo[] repos = JsonHelper.GetArray<RepositoryInfo>(response);
                    DisplayRepositories(repos);
                    
                }
                else
                {
                    // Parse and display the repository content
                    FileFolderInfo[] items = JsonHelper.GetArray<FileFolderInfo>(response);
                    fullPath=endpoint;
                    currentFilePath = fullPath;
                    DisplayItems(items);
                    
                }
            },
            onError: (error) =>
            {
                Debug.Log("GitHub API request failed: " + error);
            }
        ));
    }

    // To list the contents of a directory
    public IEnumerator ListDirectoryContents(string path)
    {
        if(path.Equals("")) yield break;

        yield return gitHubAPIHelper.MakeGitHubRequest(path,
            onSuccess: (response) =>
            {
                fullPath = path;
                currentDir = path;
                Debug.Log("GitHub API Response: " + response);
                pathName.text = path;
                FileFolderInfo[] items = JsonHelper.GetArray<FileFolderInfo>(response);  // Parse the array
                DisplayItems(items);  // Display the parsed files/folders
                
            },
            onError: (error) =>
            {
                Debug.Log("Failed to load directory contents: " + error);
            }
        );
    }

    private void DisplayRepositories(RepositoryInfo[] repos)
    {
        // Clear the current items
        foreach (Transform child in contentDirectory)
        {
            Destroy(child.gameObject);
        }

        // Display each repository as a clickable item
        foreach (var repo in repos)
        {
            GameObject newLabel = Instantiate(clickableLabelPrefab, contentDirectory);
            TextMeshProUGUI labelText = newLabel.GetComponentInChildren<TextMeshProUGUI>();
            labelText.text = repo.name;

            ClickableLabel clickable = newLabel.AddComponent<ClickableLabel>();
            clickable.OnClick += () =>
            {
                // When clicking a repository, list its contents
                StartCoroutine(ListDirectoryContents(fullPath + repo.name + "/contents/"));
            };
        }
    }

    public void GoBack()
        {
            if (!string.IsNullOrEmpty(currentDir))
            {
                // Find the parent directory by removing the last directory from the current path
                
                int countC = Regex.Matches(currentDir, @"\bcontents\b", RegexOptions.IgnoreCase).Count;
                int countR = Regex.Matches(currentDir, @"\brepos\b", RegexOptions.IgnoreCase).Count;
                if(currentDir.EndsWith("/contents", StringComparison.OrdinalIgnoreCase) && countC == 1 ) return;
                if(currentDir.EndsWith("/repos", StringComparison.OrdinalIgnoreCase)&& countR == 1 ) return;

                int lastSlashIndex = currentDir.LastIndexOf('/');
                if (lastSlashIndex >= 0)
                {
                    string parentDir = currentDir.Substring(0, lastSlashIndex);
                    StartCoroutine(ListDirectoryContents(parentDir));  // Navigate to the parent directory
                }
                else
                {
                    StartCoroutine(ListDirectoryContents(""));  // Go back to root if there's no parent directory
                }
            }
        }

    private void DisplayItems(FileFolderInfo[] items)
    {
        // Clear the current items (folders and files)
        foreach (Transform child in contentDirectory)
        {
            Destroy(child.gameObject);
        }

        // Display each file/folder as a clickable item
        foreach (var item in items)
        {
            GameObject newLabel = Instantiate(clickableLabelPrefab, contentDirectory);
            TextMeshProUGUI labelText = newLabel.GetComponentInChildren<TextMeshProUGUI>();
            labelText.text = item.name;

            ClickableLabel clickable = newLabel.AddComponent<ClickableLabel>();
            if (item.type == "dir")
            {
                clickable.OnClick += () => StartCoroutine(ListDirectoryContents(endpoint + item.name ));
                endpoint=fullPath +"/";
            }
            else if (item.type == "file")
            {
                clickable.OnClick += () => StartCoroutine(GetFileContentAndDisplay(fullPath + "/" + item.name));
            }
        }
    }


// Function to get file content and dynamically display it
public IEnumerator GetFileContentAndDisplay(string path)
{

    // Clear all existing content before showing the file
    foreach (Transform child in contentFile)
    {
        Destroy(child.gameObject);
    }

    // Request file content using the full path
    yield return gitHubAPIHelper.MakeGitHubRequest(path,
        onSuccess: (response) =>
        {
            FileFolderInfo fileInfo = JsonUtility.FromJson<FileFolderInfo>(response);
            pathName.text = fileInfo.path;  // Update the displayed path

            currentFilePath += fileInfo.path;
           // da aggiungere per fare commit
            repoMetadataUrl = fileInfo.download_url;

            string fileName = fileInfo.name;
            string fileExtension = GetFileExtension(fileName);
            currentFileSHA = fileInfo.sha;
            //Debug.Log("SHA " + currentFileSHA );
            StartCoroutine(DownloadFileContentAndDisplay(fileInfo.download_url, fileExtension));  // Use the download URL for the file
            myEditButton.isOn = false;
        },
        onError: (error) =>
        {
            Debug.Log("Failed to load file content: " + error);
        }
    );
}

private string GetFileExtension(string fileName)
{
    int lastDotIndex = fileName.LastIndexOf('.');
    if (lastDotIndex >= 0 && lastDotIndex < fileName.Length - 1)
    {
        return fileName.Substring(lastDotIndex + 1).ToLower();
    }
    return string.Empty;
}

private IEnumerator DownloadFileContentAndDisplay(string downloadUrl, string fileExtension)
{
    UnityWebRequest request = UnityWebRequest.Get(downloadUrl);
    yield return request.SendWebRequest();

    if (request.result == UnityWebRequest.Result.Success)
    {
        string fileContent = request.downloadHandler.text;
        originalFileContent = request.downloadHandler.text;
  
        

        DisplaySyntaxHighlightedCode(fileContent, fileExtension);
        ToggleEditMode(false);
       
        Debug.Log("File content displayed successfully.");

        repoMetadataUrl = downloadUrl.Replace("https://raw.githubusercontent.com", "https://api.github.com/repos");
        
        int index = repoMetadataUrl.IndexOf("main");

        if (index >= 0)
        {
            // Replace only the first occurrence of "main" with "contents"
            repoMetadataUrl = repoMetadataUrl.Substring(0, index) + "contents" + repoMetadataUrl.Substring(index + "main".Length);
        }
        
        Debug.Log("URL Per fare commit : " + repoMetadataUrl);

    }
    else
    {
        Debug.Log("Failed to download file content: " + request.error);
    }
}

public void DestroyPanel()
{
    if(GitHubPanel != null)
    {
        StopAllCoroutines();
        Destroy(GitHubPanel);
    }
}

private void DisplaySyntaxHighlightedCode(string code, string fileExtension)
{
   
      // Clear any previous content in the contentFile
    foreach (Transform child in contentFile)
    {
        Destroy(child.gameObject);
    }

    // Create a new GameObject to hold the text components
    fileTextObj = new GameObject("FileText");
    fileTextObj.transform.SetParent(contentFile, false);
    fileTextObj.transform.localPosition = Vector3.zero;

    // Add TextMeshProUGUI component to display the file content
    textComponent = fileTextObj.AddComponent<TextMeshProUGUI>();
    textComponent.gameObject.SetActive(true);
    textComponent.fontSize = 12;
    textComponent.alignment = TextAlignmentOptions.TopLeft;
    textComponent.rectTransform.sizeDelta = new Vector2(600, 800);

    // Detect language from file extension and highlight syntax
    Language detectedLanguage = DetectLanguageFromExtension(fileExtension);
    textComponent.text = SyntaxHighlighter.Highlight(code, detectedLanguage);

    

    // Set up the button to toggle edit mode
    SetupEditButton();
}



    private void SetupEditButton()
{
    // Make sure the edit button is visible and connected to ToggleEditMode
    myEditButton.gameObject.SetActive(true);
    myEditButton.GetComponent<Toggle>().isOn = false;
    myEditButton.onValueChanged.RemoveAllListeners();  // Clear previous listeners
    myEditButton.onValueChanged.AddListener(isOn => ToggleEditMode(isOn));
    inputFieldComponent.gameObject.SetActive(true);
    inputFieldComponent.text = originalFileContent;
    inputFieldComponent.gameObject.SetActive(false);
    Debug.Log("Edit button set up and listeners attached.");
}

private void ToggleEditMode(bool isInEditMode)
{
    if (isInEditMode)
    {
        Debug.Log("Switching to Edit Mode");

        // Hide syntax-highlighted view, show input field
        
        textComponent.gameObject.SetActive(false);
        inputFieldComponent.gameObject.SetActive(true);
        
        inputFieldComponent.interactable = true;
        inputFieldComponent.Select();
      
        inputFieldComponent.caretColor = Color.white;  

        CommitButton.gameObject.SetActive(true);
    }
    else
    {
        Debug.Log("Switching to View Mode");

        // Hide input field, show highlighted text component
        commitResult.text = "";
        inputFieldComponent.gameObject.SetActive(false);
        inputFieldComponent.interactable = false;
        CommitButton.gameObject.SetActive(false);

        // Reapply syntax highlighting and reset colors when switching back to view mode
        textComponent.text = SyntaxHighlighter.Highlight(textComponent.text, DetectLanguageFromExtension(GetFileExtension(currentFilePath)));
        textComponent.gameObject.SetActive(true);
    }
}


    public void OnSaveEdit()
    {
        string newContent = inputFieldComponent.text;
        string commitMessage = "Updated file from Unity app";
        StartCoroutine(CommitFileChange(newContent, commitMessage));
    }

  [System.Serializable]
public class CommitRequestData
{
    public string message;
    public string content;
    public string sha;
}

private IEnumerator CommitFileChange(string newContent, string commitMessage)
{
    //string url = "https://api.github.com/repos/Leis024/RepoTestVR/contents/Folder%201/GitHubAPIHelper.cs";
    string base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(newContent));

    // Create a CommitRequestData object
    CommitRequestData commitData = new CommitRequestData
    {
        message = commitMessage,
        content = base64Content,
        sha = currentFileSHA
    };

    // Convert the object to JSON
    string json = JsonUtility.ToJson(commitData);

    // Debugging the JSON
    //Debug.Log("JSON Payload: " + json);

    // Create the request
    UnityWebRequest request = new UnityWebRequest(repoMetadataUrl, "PUT");
    byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    request.downloadHandler = new DownloadHandlerBuffer();
    request.SetRequestHeader("Content-Type", "application/json");
    request.SetRequestHeader("Authorization", "token " + token);
    request.SetRequestHeader("User-Agent", "UnityVRApp");

    yield return request.SendWebRequest();


    
/*
    inputFieldComponent.gameObject.SetActive(false);
    inputFieldComponent.interactable = false;
    CommitButton.gameObject.SetActive(false);
    textComponent.gameObject.SetActive(true);
    
Debug.Log("Aggiorna");
    string aggiorna = repoMetadataUrl;
    aggiorna = aggiorna.Replace("https://api.github.com/repos", "");
        
        int index = aggiorna.IndexOf("main");

        if (index >= 0)
        {
            // Replace only the first occurrence of "main" with "contents"
            aggiorna = aggiorna.Substring(0, index) + "contents" + aggiorna.Substring(index + "main".Length);
        }
        
        Debug.Log("URL Per Aggiornare dopo commit : " + aggiorna);
    GetFileContentAndDisplay(aggiorna); */


    // Check for errors
    if (request.result == UnityWebRequest.Result.Success)
    {
        Debug.Log("File committed successfully!");
        //ToggleEditMode(false);
        commitResult.text = "<color=green>File committed successfully!</color>";
    }
    else
    {
        Debug.Log("Failed to commit file: " + request.error);
        Debug.Log("Response: " + request.downloadHandler.text);
        commitResult.color = Color.red;
        commitResult.text = "<color=red>File commit failed!</color>";
    }
}



private Language DetectLanguageFromExtension(string fileExtension)
{
    switch (fileExtension)
    {
        case "cs":
            return Language.CSharp;
        case "js":
            return Language.JavaScript;
        case "py":
            return Language.Python;
        case "html":
            return Language.HTML;
        case "css":
            return Language.CSS;
        // Add more file extensions and corresponding languages here
        default:
            return Language.PlainText;  // Fallback to plain text if language is unknown
    }
}


}
