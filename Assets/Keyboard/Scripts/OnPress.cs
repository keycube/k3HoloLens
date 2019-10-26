using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class OnPress : MonoBehaviour, IPointerDownHandler
{
    public event Action<string> OnKeyPress = delegate { };

    public string key;

    public void OnPointerDown(PointerEventData data)
    {
        OnKeyPress(key);
    }
}