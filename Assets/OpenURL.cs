using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenURL : MonoBehaviour
{
public void OpenWebsite(string url = "https://github.com/"){
        Application.OpenURL(url);
    }
}
