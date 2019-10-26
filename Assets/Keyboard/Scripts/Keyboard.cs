using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class Keyboard : MonoBehaviour
{
    public event Action<string> OnKeyPress = delegate { };

    void Start()
    {
        GameObject[] keys = GameObject.FindGameObjectsWithTag("Key");;
        foreach (GameObject key in keys)
        {
            string keyCharacter = key.name[3].ToString(); // "e.g KeyA" <- character A
            key.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = keyCharacter; // set the key text
            OnPress onPress = key.GetComponent<OnPress>();
            onPress.key = keyCharacter.ToLower();
            onPress.OnKeyPress += Key_OnPress;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Key_OnPress(string s)
    {
        OnKeyPress(s);
    }
}
