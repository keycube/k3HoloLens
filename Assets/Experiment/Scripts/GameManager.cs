using UnityEngine;
using TMPro;
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
    private float lastPressTime;
    public TextMeshProUGUI textWPM;
    public TextMeshProUGUI textER;
    private int keyStrokeCount;

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
        keyStrokeCount += 1;

        if (typingActive == false) // start typing
        {
            typingActive = true;
            textPresented.color = colorTypingActive;

            startTypingTime = Time.time;
        }

        string currentTranscribedText = inputField.text.Substring(0, inputField.text.Length-1);

        if (s.Equals(">")) // enter (i.e. end of the phrase, stop typing)
        {
            float timing = lastPressTime - startTypingTime;
            float wpm = TypingUtils.WordsPerMinute(currentTranscribedText, timing);
            float er = TypingUtils.ErrorRate(textPresented.text, currentTranscribedText);
            float kspc = TypingUtils.KSPC(keyStrokeCount-1, currentTranscribedText); // minus 1 to remove Enter keystroke
            textWPM.text = Mathf.Round(wpm) + " wpm";
            textER.text = Mathf.Round(er) + " %";
            LogEssential(textPresented.text, currentTranscribedText, wpm, er, kspc);
            SetPhrase();
            return;
        }

        lastPressTime = Time.time;
        
        if (s.Equals("<")) // backspace
        {
            if (inputField.text.Length > 1)
                inputField.text = inputField.text.Substring(0, inputField.text.Length-2) + "_";
            return;
        }

        if (s.Equals("_")) // space
            s = " ";
        inputField.text = currentTranscribedText + s + "_";
    }

    private void SetPhrase()
    {
        inputField.text = "_";
        keyStrokeCount = 0;
        int index = Random.Range(0, phrases.Count);
        string phrase = phrases[index];
        phrases.RemoveAt(index);
        textPresented.text = phrase;
        textPresented.color = colorTypingInactive;
        typingActive = false;
    }

    private void LogEssential(string presentedText, string transcribedText, float WPM, float ER, float KSPC)
    {
        string s = System.DateTime.Now.ToString("yyyy-mm-dd hh:mm:ss.fff") + "\t" +
            presentedText + "\t" +
            transcribedText + "\t" +
            WPM + "\t" +
            ER + "\t" +
            KSPC + "\n";
            
        Debug.Log(s);
        FileUtils.AppendTextToFile(Application.dataPath + "/logs.txt", s);
    }
}