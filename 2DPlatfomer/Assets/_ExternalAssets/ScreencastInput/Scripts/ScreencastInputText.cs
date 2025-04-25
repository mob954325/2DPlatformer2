using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

internal sealed class ScreencastInputText : MonoBehaviour
{
    private const string INPUT_DELIMITER = " + ";
    private const string LOG_PATH = @"Assets/ScreencastInput/Logs";
    private const string LOG_FILENAME_PATTERN = "yyyy-dd-MM--HH-mm-ss";
    private const string LOG_FILE_EXTENSION = ".txt";
    private const string LOG_ENTRY_PREFIX = "[Screencast Input]";

    [Header("References")]
    [SerializeField] private TMP_Text _inputText;

    [Header("Settings")]
    [SerializeField] private ScreencastInputTextSettingsSO _screencastInputSettings;

    private float _clearTimer;
    private float _displayTimer;
    private int _inputCount;
    private StreamWriter _logStream;

    private readonly List<string> _displayList = new List<string>();
    private readonly List<string> _removeList = new List<string>();
    private string _lastInput = string.Empty;
    private string _logFilePath;

    private KeyCode[] _keyCodes;

    private void Awake()
    {
        _keyCodes = Enum.GetValues(typeof(KeyCode)) as KeyCode[];
    }

    private void Start()
    {
        _inputText.text = string.Empty;

#if UNITY_EDITOR
        if (_screencastInputSettings.LogEnabled)
        {
            _logFilePath = $"{LOG_PATH}/{DateTime.Now:LOG_FILENAME_PATTERN}{LOG_FILE_EXTENSION}";
            Debug.LogWarning($"{LOG_ENTRY_PREFIX} Starting Input Logger. Log Path: {_logFilePath}");

            if (!Directory.Exists(LOG_PATH))
                Directory.CreateDirectory(LOG_PATH);

            _logStream = File.Exists(_logFilePath)
                ? new StreamWriter(_logFilePath, true)
                : File.CreateText(_logFilePath);
        }
#endif
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        _logStream?.Close();

        if (_logStream != null)
        {
            Debug.Log($"{LOG_ENTRY_PREFIX} Closing Input Logger.");
            AssetDatabase.ImportAsset(_logFilePath);
        }
#endif
    }

    private void Update()
    {
#if UNITY_EDITOR
/*        foreach (var view in SceneView.sceneViews)
            if (view is EditorWindow { hasFocus: true })
                return;*/

        PollInput();
        UpdateDisplayTime();
        UpdateDisplay();
        UpdateLog();
#endif
    }

    private void PollInput()
    {
        foreach (KeyCode code in _keyCodes)
        {
            if (code == KeyCode.LeftWindows || code == KeyCode.RightWindows || code.ToString().Contains("Command") || code.ToString().Contains("Apple"))
                continue;

            string name = code.ToString();
            if (name == KeyCode.Mouse0.ToString()) name = "Left Button";
            else if (name == KeyCode.Mouse1.ToString()) name = "Right Button";
            else if (name == KeyCode.Mouse2.ToString()) name = "Middle Button";

            if (Input.GetKeyDown(code))
            {
                _inputCount = (_lastInput == name) ? _inputCount + 1 : 1;
                _displayList.Add(name);
                _lastInput = name;
            }
            else if (Input.GetKeyUp(code))
            {
                _clearTimer = 0.1f;
                _removeList.Add(name);
            }
        }
    }

    private void UpdateDisplayTime()
    {
        if (Input.anyKey)
            _displayTimer = _screencastInputSettings.DisplayTime;

        _clearTimer = Mathf.Clamp(_clearTimer - Time.deltaTime, 0f, _screencastInputSettings.DisplayTime);
        if (_clearTimer <= 0f)
        {
            _displayList.RemoveAll(input => _removeList.Contains(input));
            _removeList.Clear();
        }
    }

    private void UpdateDisplay()
    {
        _displayTimer = Mathf.Clamp(_displayTimer - Time.deltaTime, 0f, _screencastInputSettings.DisplayTime);

        if (_displayTimer <= 0f)
        {
            _displayList.Clear();
            _removeList.Clear();
            _inputText.text = string.Empty;
            return;
        }

        if (!_displayList.Any())
            return;

        if (_inputCount > 1 && _displayList.All(i => i == _displayList[0]))
            _inputText.text = $"{_displayList[0]} x{_inputCount}";
        else
            _inputText.text = string.Join(INPUT_DELIMITER, _displayList.Distinct());
    }

    private void UpdateLog()
    {
        if (_logStream == null)
            return;

        if (!string.IsNullOrEmpty(_inputText.text))
            _logStream.WriteLine($"[{DateTime.Now:HH:mm:ss}] {_inputText.text}");

        _logStream.Flush();
    }
}
