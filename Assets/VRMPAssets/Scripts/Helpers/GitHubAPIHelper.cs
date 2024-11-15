using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using XRMultiplayer;

public class GitHubAPIHelper : MonoBehaviour
{
    private string token;  
    public GitHubTokenManager tokenManager;
    private string baseUrl = "https://api.github.com";

    private void Start()
        {
            tokenManager = GetComponent<GitHubTokenManager>();
            tokenManager.OnTokenLoaded += OnTokenLoaded;
            
        }

    private void OnTokenLoaded()
    {
        token = tokenManager.GetToken();
    }

    // General function to make GitHub API requests
    public IEnumerator MakeGitHubRequest(string endpoint, System.Action<string> onSuccess, System.Action<string> onError)
    {
        string url = baseUrl + endpoint;
        UnityWebRequest request = UnityWebRequest.Get(url);
        Debug.Log("GitHub API Response FULL URL: " + url + " TOKEN = " + token);
        //Set the authorization header
        request.SetRequestHeader("Authorization", "token " + token);
        request.SetRequestHeader("User-Agent", "UnityVRApp");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }
}
