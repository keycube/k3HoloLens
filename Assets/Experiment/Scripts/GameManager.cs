using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public Keyboard keyboard;
    public TMP_InputField inputField;
    public TextMeshProUGUI textPresented;

    private List<string> phrases;

    async void Start()
    {
        keyboard.OnKeyPress += Keyboard_OnKeyPress;
        inputField.Select();

        string s = await FileUtils.ReadTextFile("Assets/Experiment/Resources/phrases2.txt"); 
        phrases = new List<string>(s.Split('\n'));

        SetPhrase();
    }

    void Keyboard_OnKeyPress(string s)
    {
        if (s.Equals("<")) // backspace
        {
            if (inputField.text.Length > 0)
                inputField.text = inputField.text.Substring(0, inputField.text.Length-1);
            return;
        }

        if (s.Equals(">")) // enter
        {
            SetPhrase();
            return;
        }

        if (s.Equals("_")) // space
            s = " ";

        inputField.text += s;
    }

    private void SetPhrase()
    {
        int index = Random.Range(0, phrases.Count);
        string phrase = phrases[index];
        phrases.RemoveAt(index);
        textPresented.text = phrase;
    }
}