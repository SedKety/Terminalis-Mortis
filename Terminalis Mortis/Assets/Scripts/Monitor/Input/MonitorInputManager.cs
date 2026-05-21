using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MonitorInputManager : MonoBehaviour
{
    public TMP_Text uiText;
    [SerializeField] private GridGenerator gridGenerator;
    [SerializeField] private Transform movementTarget;
    [SerializeField] private float moveStepDistance = 1f;
    [SerializeField] private int maxLines = 12;
    [SerializeField] private bool showIntroOnStart = true;
    [SerializeField] private bool autoPlayBootSequence = true;
    [SerializeField] private float bootSequenceStepDelay = 0.35f;
    [SerializeField, TextArea(2, 8)] private List<string> bootSequenceLines = new List<string>
    {
        "booting, wait,",
        "booting, wait,.",
        "booting, wait,.."
    };
    [SerializeField] private string introNavigationHint = "Click Space to continue or Backspace to go back";
    [SerializeField] private int introMaxCharactersPerPage = 220;
    [SerializeField, TextArea(2, 8)] private List<string> introPages = new List<string>
    {
        "Hello User, welcome to Terminalis Mortis",
        "[SYS] This is turn-based. Each move or attack spends one turn.",
        "[SYS] Use move <north|south|east|west> to move one tile in that direction.",
        "[SYS] Use attack <direction> to strike one adjacent tile.",
        "[SYS] Clear the room, then run locate to continue.",
        "[SYS] move | attack | locate | help"
    };

    private bool _activeFlicker = false;
    private string _currentInput = string.Empty;
    private MonitorCommandParser _commandParser;
    private MonitorCommandExecutor _commandExecutor;
    private InteractionManager _interactionManager;
    private bool _introActive;
    private bool _bootSequencePlaying;
    private int _introIndex;
    private readonly List<string> _activeIntroPages = new List<string>();
    private const string IconsIntroPage = "[ICON Meanings]\nP = you\n# = wall\nM = malware enemy\nV = virus enemy\nT = trojan enemy\nR = ransomware enemy";

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

        if (gridGenerator == null)
        {
            gridGenerator = FindFirstObjectByType<GridGenerator>();
        }

        MovementManager movementManager;

        if (gridGenerator != null)
        {
            movementManager = new MovementManager(gridGenerator);
        }
        else
        {
            if (movementTarget == null)
            {
                movementTarget = transform;
            }

            movementManager = new MovementManager(movementTarget, moveStepDistance);
        }

        _interactionManager = new InteractionManager(gridGenerator);
        _commandExecutor = new MonitorCommandExecutor(movementManager, _interactionManager);
        PrepareIntroPages();

        if (showIntroOnStart)
        {
            if (autoPlayBootSequence && bootSequenceLines != null && bootSequenceLines.Count > 0)
            {
                StartCoroutine(PlayBootThenIntro());
            }
            else
            {
                StartIntroSequence(introPages);
            }
        }
        else
        {
            AppendLine(_interactionManager.GetTutorial());
            AppendLine(_interactionManager.GetStatus());
        }

        StartCoroutine(FlickeringDot());
    }

    private void PrepareIntroPages()
    {
        if (introPages == null)
        {
            introPages = new List<string>();
        }

        List<string> sanitized = new List<string>();
        foreach (string page in introPages)
        {
            if (string.IsNullOrWhiteSpace(page))
            {
                continue;
            }

            string trimmed = page.Trim();
            string lower = trimmed.ToLowerInvariant();

            if (lower.Contains("path"))
            {
                continue;
            }

            if (lower.Contains("icon"))
            {
                continue;
            }

            if (trimmed.Contains("P =") || trimmed.Contains("P=")
                || trimmed.Contains("M =") || trimmed.Contains("M=")
                || trimmed.Contains("V =") || trimmed.Contains("V=")
                || trimmed.Contains("T =") || trimmed.Contains("T=")
                || trimmed.Contains("R =") || trimmed.Contains("R=")
                || trimmed.Contains(". =") || trimmed.Contains(".="))
            {
                continue;
            }

            sanitized.Add(trimmed);
        }

        if (sanitized.Count == 0)
        {
            sanitized.Add("Hello User, welcome to Terminalis Mortis");
            sanitized.Add("[SYS] This is turn-based. Each move or attack spends one turn.");
            sanitized.Add("[SYS] Use move <north|south|east|west> to move one tile in that direction.");
            sanitized.Add("[SYS] Use attack <direction> to strike one adjacent tile.");
            sanitized.Add("[SYS] Clear the room, then run locate to continue.");
            sanitized.Add("[SYS] move | attack | locate | help");
        }

        sanitized.Add(IconsIntroPage);
        introPages = sanitized;
    }

    private void Update()
    {
        if (uiText == null) return;

        if (_bootSequencePlaying)
        {
            return;
        }

        if (_introActive)
        {
            HandleIntroInput();
            return;
        }

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

    private IEnumerator PlayBootThenIntro()
    {
        _bootSequencePlaying = true;
        _introActive = false;
        _currentInput = string.Empty;

        foreach (string line in bootSequenceLines)
        {
            if (uiText == null)
            {
                break;
            }

            uiText.text = line;
            EnforceMaxLines();
            yield return new WaitForSeconds(bootSequenceStepDelay);
        }

        _bootSequencePlaying = false;
        StartIntroSequence(introPages);
    }

    public void StartIntroSequence(IEnumerable<string> pages)
    {
        _activeIntroPages.Clear();

        if (pages != null)
        {
            foreach (string page in pages)
            {
                string resolved = ResolveIntroPage(page);
                if (resolved == IconsIntroPage)
                {
                    _activeIntroPages.Add(IconsIntroPage);
                }
                else
                {
                    AddIntroChunks(resolved);
                }
            }
        }

        if (_activeIntroPages.Count == 0)
        {
            string tutorial = _interactionManager != null ? _interactionManager.GetTutorial() : "Welcome.";
            _activeIntroPages.Add(tutorial);
        }

        _introActive = true;
        _introIndex = 0;
        _currentInput = string.Empty;
        RenderIntroPage();
    }

    private void HandleIntroInput()
    {
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            if (_introIndex < _activeIntroPages.Count - 1)
            {
                _introIndex++;
                RenderIntroPage();
            }
            else
            {
                _introActive = false;
                uiText.text = string.Empty;
                AppendLine("[BOOT] Ready. Type help.");
            }
        }
        else if (Input.GetKeyUp(KeyCode.Backspace))
        {
            if (_introIndex > 0)
            {
                _introIndex--;
                RenderIntroPage();
            }
        }
    }

    private void RenderIntroPage()
    {
        if (_activeIntroPages.Count == 0)
        {
            _introActive = false;
            return;
        }

        string page = _activeIntroPages[_introIndex];
        if (_introIndex == 0)
        {
            uiText.text = page + "\n\n<color=#ffd966>" + introNavigationHint + "</color>";
            return;
        }

        uiText.text = page;
    }

    private string ResolveIntroPage(string page)
    {
        if (string.IsNullOrWhiteSpace(page))
        {
            return string.Empty;
        }

        string resolved = page;
        if (_interactionManager != null)
        {
            resolved = resolved.Replace("{tutorial}", _interactionManager.GetTutorial());
            resolved = resolved.Replace("{status}", _interactionManager.GetStatus());
        }

        return resolved;
    }

    private void AddIntroChunks(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        int limit = Mathf.Max(80, introMaxCharactersPerPage);
        string remaining = text.Trim();

        while (remaining.Length > limit)
        {
            int splitIndex = FindSplitIndex(remaining, limit);
            string chunk = remaining.Substring(0, splitIndex).Trim();
            if (!string.IsNullOrEmpty(chunk))
            {
                _activeIntroPages.Add(chunk);
            }

            remaining = remaining.Substring(splitIndex).TrimStart();
        }

        if (!string.IsNullOrEmpty(remaining))
        {
            _activeIntroPages.Add(remaining);
        }
    }

    private static int FindSplitIndex(string text, int limit)
    {
        int newline = text.LastIndexOf('\n', limit);
        if (newline > 40)
        {
            return newline + 1;
        }

        int space = text.LastIndexOf(' ', limit);
        if (space > 40)
        {
            return space + 1;
        }

        return limit;
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

        string coloredMessage = ColorizeMessage(message);

        if (!string.IsNullOrEmpty(uiText.text) && !uiText.text.EndsWith("\n"))
        {
            uiText.text += "\n";
        }

        uiText.text += coloredMessage + "\n";
    }

    private string ColorizeMessage(string message)
    {
        if (message.Contains("<color="))
        {
            return message;
        }

        string lower = message.ToLowerInvariant();

        if (lower.StartsWith("moved "))
        {
            return "<color=#8cff66>" + message + "</color>";
        }

        if (lower.Contains("blocked")
            || lower.Contains("unknown")
            || lower.Contains("no enemy")
            || lower.Contains("no dungeon")
            || lower.Contains("attacked")
            || lower.Contains("void"))
        {
            return "<color=#ff4d4d>" + message + "</color>";
        }

        if (lower.Contains("use:")
            || lower.Contains("commands")
            || lower.Contains("enemy"))
        {
            return "<color=#ffd966>" + message + "</color>";
        }

        return "<color=#8cff66>" + message + "</color>";
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

            if (_bootSequencePlaying || _introActive)
            {
                _activeFlicker = false;
                continue;
            }

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
