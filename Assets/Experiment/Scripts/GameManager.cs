using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public Keyboard keyboard;
    public TMP_InputField inputField;
    public TextMeshProUGUI textPresented;
    public Color colorTypingInactive;
    public Color colorTypingActive;
    private bool typingActive;
    private List<string> phrases;
    private float startTypingTime;
    public TextMeshProUGUI textWPM;
    public TextMeshProUGUI textER;

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
        if (typingActive == false) // start typing
        {
            typingActive = true;
            textPresented.color = colorTypingActive;

            startTypingTime = Time.time;
        }

        if (s.Equals("<")) // backspace
        {
            if (inputField.text.Length > 1)
                inputField.text = inputField.text.Substring(0, inputField.text.Length-2) + "_";
            return;
        }
        string currentTranscribedText = inputField.text.Substring(0, inputField.text.Length-1);

        if (s.Equals(">")) // enter (i.e. end of the phrase, stop typing)
        {
            SetPhrase();
            float timing = Time.time - startTypingTime;
            textWPM.text = Mathf.Round(TypingUtils.WordsPerMinute(currentTranscribedText, timing)) + " wpm";
            textER.text = Mathf.Round(TypingUtils.ErrorRate(textPresented.text, currentTranscribedText)) + " %";
            return;
        }

        if (s.Equals("_")) // space
            s = " ";

        inputField.text = currentTranscribedText + s + "_";
    }

    private void SetPhrase()
    {
        inputField.text = "_";
        int index = Random.Range(0, phrases.Count);
        string phrase = phrases[index];
        phrases.RemoveAt(index);
        textPresented.text = phrase;
        textPresented.color = colorTypingInactive;
        typingActive = false;
    }
}