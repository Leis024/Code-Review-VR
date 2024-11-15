using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class ClickableLabel : MonoBehaviour, IPointerClickHandler
{
    public delegate void ClickAction();
    public event ClickAction OnClick;

    public void Initialize(string type, string path, GitHubAPIManager manager)
    {
        // You can store or use 'type' and 'path' if needed for further logic
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke(); // Invoke the click event when clicked
    }
}
