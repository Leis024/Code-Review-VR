[System.Serializable]

public class FileFolderInfo 
{
    public string name;  // The name of the file or folder
    public string path;  // The path to the file or folder
    public string type;  // Either "file" or "dir"
    public string download_url;  // URL to download the file (for files only)
    public string sha;
    // Add other fields from GitHub's response as needed
}
