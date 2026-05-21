using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MonitorInputManager : MonoBehaviour
{
    public TMP_Text uiText;
    [SerializeField] private Transform movementTarget;
    [SerializeField] private float moveStepDistance = 1f;
    [SerializeField] private int maxLines = 12;

    private bool _activeFlicker = false;
    private string _currentInput = string.Empty;
    private MonitorCommandParser _commandParser;
    private MonitorCommandExecutor _commandExecutor;

    private void Start()
    {
        if (uiText == null)
        {
            Debug.LogError("MonitorInputManager: uiText is not assigned.");
            enabled = false;
            return;
        }

        uiText.enableWordWrapping = true;
        uiText.overflowMode = TextOverflowModes.Truncate;

        _commandParser = new MonitorCommandParser();

        if (movementTarget == null)
        {
            movementTarget = transform;
        }

        MovementManager movementManager = new MovementManager(movementTarget, moveStepDistance);
        InteractionManager interactionManager = new InteractionManager();
        _commandExecutor = new MonitorCommandExecutor(movementManager, interactionManager);

        StartCoroutine(FlickeringDot());
    }                           

    private void Update()
    {
        if (uiText == null) return;
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            uiText.text = string.Empty;
        }
        foreach (char c in Input.inputString)
        {
            if (c == '\b') // backspace
            {
                HandleBackspace();
            }
            else if (c == '\n' || c == '\r')
            {
                SubmitInput();
            }
            else
            {
                WriteToText(c);
            }
        }
    }

    private void HandleBackspace()
    {
        // Remove flicker dot first if currently shown.
        if (_activeFlicker && uiText.text.Length > 0 && uiText.text.EndsWith("."))
        {
            uiText.text = uiText.text.Substring(0, uiText.text.Length - 1);
            _activeFlicker = false;
        }

        // Then remove actual last character.
        if (uiText.text.Length > 0)
        {
            uiText.text = uiText.text.Substring(0, uiText.text.Length - 1);
        }

        if (_currentInput.Length > 0)
        {
            _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
        }
    }

    private void WriteToText(char c)
    {
        // Remove flicker dot safely before writing.
        if (_activeFlicker && uiText.text.Length > 0 && uiText.text.EndsWith("."))
        {
            uiText.text = uiText.text.Substring(0, uiText.text.Length - 1);
            _activeFlicker = false;
        }

        uiText.text += c;
        _currentInput += c;
        EnforceMaxLines();
    }

    private void SubmitInput()
    {
        if (_activeFlicker && uiText.text.Length > 0 && uiText.text.EndsWith("."))
        {
            uiText.text = uiText.text.Substring(0, uiText.text.Length - 1);
            _activeFlicker = false;
        }

        string commandText = _currentInput.Trim();
        MonitorCommandType commandType = _commandParser.ParseWithParameters(commandText, out string parameters);
        MonitorCommandResult result = _commandExecutor.Execute(commandType, parameters);

        if (result.ClearTerminal)
        {
            uiText.text = string.Empty;
        }

        if (!string.IsNullOrEmpty(result.Output))
        {
            AppendLine(result.Output);
        }

        _currentInput = string.Empty;
        EnforceMaxLines();
    }

    private void AppendLine(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (!string.IsNullOrEmpty(uiText.text) && !uiText.text.EndsWith("\n"))
        {
            uiText.text += "\n";
        }

        uiText.text += message + "\n";
    }

    private void EnforceMaxLines()
    {
        if (maxLines <= 0 || string.IsNullOrEmpty(uiText.text))
        {
            EnforceRectBounds();
            return;
        }

        List<string> lines = new List<string>(uiText.text.Split('\n'));

        if (lines.Count <= maxLines)
        {
            EnforceRectBounds();
            return;
        }

        int removeCount = lines.Count - maxLines;
        lines.RemoveRange(0, removeCount);
        uiText.text = string.Join("\n", lines).TrimStart('\n');

        EnforceRectBounds();
    }

    private void EnforceRectBounds()
    {
        if (uiText == null || string.IsNullOrEmpty(uiText.text))
        {
            return;
        }

        uiText.ForceMeshUpdate();

        float rectHeight = uiText.rectTransform.rect.height;
        if (rectHeight <= 0f)
        {
            return;
        }

        int safety = 0;
        while (uiText.preferredHeight > rectHeight && safety < 200)
        {
            int newlineIndex = uiText.text.IndexOf('\n');
            if (newlineIndex < 0)
            {
                if (uiText.text.Length <= 1)
                {
                    uiText.text = string.Empty;
                    break;
                }

                uiText.text = uiText.text.Substring(1);
            }
            else
            {
                uiText.text = uiText.text.Substring(newlineIndex + 1);
            }

            uiText.ForceMeshUpdate();
            safety++;
        }
    }

    private IEnumerator FlickeringDot()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            if (uiText == null) continue;

            if (_activeFlicker)
            {
                // Only remove the dot we added for flicker.
                if (uiText.text.Length > 0 && uiText.text.EndsWith("."))
                {
                    uiText.text = uiText.text.Substring(0, uiText.text.Length - 1);
                }

                _activeFlicker = false;
            }
            else
            {
                uiText.text += ".";
                _activeFlicker = true;
            }

            EnforceMaxLines();
        }
    }
}
