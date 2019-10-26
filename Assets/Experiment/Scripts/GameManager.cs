using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public Keyboard keyboard;
    public TMP_InputField inputField;

    void Start()
    {
        keyboard.OnKeyPress += Keyboard_OnKeyPress;
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
            keyboard.Hide();
            return;
        }

        if (s.Equals("_")) // space
            s = " ";

        inputField.text += s;
    }
}
