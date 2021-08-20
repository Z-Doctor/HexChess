using UnityEngine;

public class LinkObjectButton : MonoBehaviour, IObjectButton
{
    public string url;

    public void Click() => Application.OpenURL(url);
}