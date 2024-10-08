﻿using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class GameManager : MonoBehaviour
{
    public GameObject keyboard;
    public TMP_InputField inputFieldTranscribed;
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
    private string currentTextEntryInterface = "unknown";
    private string currentUserCode = "unknown";
    public TextMeshProUGUI textCurrentUserCode;
    public TextMeshProUGUI textCurrentTextEntryInterface;
    private string fullTranscribedInputStream;
    private bool started;
    private float timeLeft;
    public RectTransform panelTime;
    private static float SESSION_TIMING;
    private float previousKeyPressTime;
    private string logPressBulk;
    private bool finish;

    async void Start()
    {

#if WINDOWS_UWP
        GameObject.Find("SceneContent").GetComponent<RadialView>().MinDistance = 2f;
        GameObject.Find("SceneContent").GetComponent<RadialView>().MaxDistance = 2f;
#endif

        keyboard.GetComponent<Keyboard>().OnKeyPress += Keyboard_OnKeyPress;
        keyboard.SetActive(false);

        inputFieldTranscribed.Select();

        string s = await FileUtils.ReadTextFile("Assets/Experiment/Resources/phrases2.txt");
        phrases = new List<string>(s.Split('\n'));

        NetworkUtils network = new NetworkUtils();
        network.OnMessageReceived += NetworkUtils_OnMessageReceived;
        network.StartServer("55555");
        
        SetPhrase();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }

        if (started)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0) 
            {
                FinishSession();
            }
            panelTime.sizeDelta = new Vector2(Mathf.Lerp(400, 0, timeLeft/SESSION_TIMING), panelTime.sizeDelta.y);
        }

        if (currentTextEntryInterface.Equals("tk"))
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Keyboard_OnKeyPress(">");
            }

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                Keyboard_OnKeyPress("<");
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Keyboard_OnKeyPress(" ");
            }

            for (int i = (int)KeyCode.A; i <= (int)KeyCode.Z; i++)
            {
                if (Input.GetKeyDown((KeyCode)i))
                {
                    Keyboard_OnKeyPress(((KeyCode)i).ToString().ToLower());
                }
            }
        }
    }

    void Keyboard_OnKeyPress(string s)
    {
        if (s.Length > 1)
        {
            LogPress(s, false);
        }
        else
        {
            if (started)
            {
                LogPress(s, true);
            }            
            fullTranscribedInputStream += s;
            keyStrokeCount += 1;

            if (typingActive == false) // start typing
            {
                typingActive = true;
                textPresented.color = colorTypingActive;

                startTypingTime = Time.time;
            }

            string currentTranscribedText = inputFieldTranscribed.text.Substring(0, inputFieldTranscribed.text.Length-1);

            if (s.Equals(">")) // enter (i.e. end of the phrase, stop typing)
            {
                float timing = lastPressTime - startTypingTime;
                float wpm = TypingUtils.WordsPerMinute(currentTranscribedText, timing);
                float er = TypingUtils.ErrorRate(textPresented.text, currentTranscribedText);
                textWPM.text = Mathf.Round(wpm) + " wpm";
                textER.text = Mathf.Round(er) + " %";
                if (started)
                {
                    LogEssential(textPresented.text, currentTranscribedText, wpm, er);
                    BulkLogPress();
                    if (finish)
                    {
                        finish = false;
                        started = false;
                    }
                }                
                SetPhrase();
                return;
            }

            lastPressTime = Time.time;
            
            if (s.Equals("<")) // backspace
            {
                if (inputFieldTranscribed.text.Length > 1)
                    inputFieldTranscribed.text = inputFieldTranscribed.text.Substring(0, inputFieldTranscribed.text.Length-2) + "_";
                return;
            }

            if (s.Equals("_")) // space
                s = " ";
            inputFieldTranscribed.text = currentTranscribedText + s + "_";
        }
    }

    private void SetPhrase()
    {
        inputFieldTranscribed.text = "_";
        fullTranscribedInputStream = "";
        keyStrokeCount = 0;
        previousKeyPressTime = -1f;
        string phrase = "abcdefghijklmnopqrstuvwxyz";
        if (started)
        {
            int index = Random.Range(0, phrases.Count);
            phrase = phrases[index];
            phrases.RemoveAt(index);
        }
        textPresented.text = phrase;
        textPresented.color = colorTypingInactive;
        typingActive = false;
    }

    private void LogEssential(string presentedText, string transcribedText, float WPM, float ER)
    {
        float KSPC = TypingUtils.KSPC(keyStrokeCount-1, transcribedText); // minus 1 to remove Enter keystroke
        string s = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + "\t" +
            currentUserCode + "\t" +
            currentTextEntryInterface + "\t" +
            presentedText + "\t" +
            transcribedText + "\t" +
            WPM + "\t" +
            ER + "\t" +
            KSPC + "\t" + 
            (keyStrokeCount-1) + "\t" + // minus 1 to remove Enter keystroke
            fullTranscribedInputStream + "\n";
            
        Debug.Log(s);
#if WINDOWS_UWP
        FileUtils.AppendTextToFile(currentUserCode + "_essential.txt", s);
#else
        FileUtils.AppendTextToFile(Application.dataPath + "/" + currentUserCode + "_essential.txt", s);
#endif
    }

    private void LogPress(string s, bool allowed)
    {
        float interKeyPressTime;
        if (previousKeyPressTime == -1f) // first letter of the phrase
        {
            interKeyPressTime = -1f;
        }
        else
        {
            interKeyPressTime = Time.time - previousKeyPressTime;
        }
        string log = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + "\t" +
            currentUserCode + "\t" +
            currentTextEntryInterface + "\t" +
            allowed + "\t" +
            s + "\t" + 
            interKeyPressTime + "\n";
        previousKeyPressTime = Time.time;

        Debug.Log(log);
        logPressBulk += log;
    }

    // to avoid too many writing
    private void BulkLogPress()
    {
#if WINDOWS_UWP
        FileUtils.AppendTextToFile(currentUserCode + "_press.txt", logPressBulk);
#else
        FileUtils.AppendTextToFile(Application.dataPath + "/" + currentUserCode + "_press.txt", logPressBulk);
#endif
        logPressBulk = "";
    }

    private void NetworkUtils_OnMessageReceived(string message)
    {

        string[] data = message.TrimEnd().Split(':');
        switch(data[0])
        {
            case "u": // user
                currentUserCode = data[1];
                textCurrentUserCode.SetText(currentUserCode);
            break;
            case "i": // interface
                currentTextEntryInterface = data[1];
                switch(currentTextEntryInterface)
                {
                    case "hggk":
                        SESSION_TIMING = 1200f; // 20 min
                        textCurrentTextEntryInterface.text = "HoloLens Gaze Gesture Keyboard";
                        keyboard.SetActive(true);
                    break;
                    case "ssk":
                        SESSION_TIMING = 1200f; // 20 min
                        textCurrentTextEntryInterface.text = "Smartphone Soft Keyboard";
                        keyboard.SetActive(false);
                    break;
                    case "kc":
                        SESSION_TIMING = 1200f; // 20 min
                        textCurrentTextEntryInterface.text = "Keycube";
                        keyboard.SetActive(false);
                    break;
                    case "tk":
                        SESSION_TIMING = 300f; // 5 min
                        textCurrentTextEntryInterface.text = "Traditional Keyboard";
                        keyboard.SetActive(false);
                    break;
                }
            break;
            case "k": // key press
                Keyboard_OnKeyPress(data[1]);
            break;
            case "s":
                StartSession();
            break;
        }
    }

    private void StartSession()
    {
        timeLeft = SESSION_TIMING;
        started = true;
        SetPhrase();
    }

    private void FinishSession()
    {
        finish = true;
    }
}