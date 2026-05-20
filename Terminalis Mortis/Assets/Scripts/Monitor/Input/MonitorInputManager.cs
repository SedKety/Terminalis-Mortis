using UnityEngine;
using TMPro;
public class MonitorInputManager : MonoBehaviour
{
    public TMP_Text uiText;

    void Update()
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b') // backspace
            {
                if (uiText.text.Length > 0)
                {
                    uiText.text = uiText.text.Substring(0, uiText.text.Length - 1);
                }
            }
            else if (c == '\n' || c == '\r')
            {
                uiText.text += "\n"; // enter key
            }
            else
            {
                uiText.text += c;
            }
        }
    }
}
