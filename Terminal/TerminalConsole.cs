using UnityEngine;
using System.Collections.Generic;
using TerminalSystem;
using System;

public class TerminalConsole {

    private const float START_DELAY = 7f;

    private bool _activeInput = true;
    private bool _canInputCommands = false;

    public TextTyper Typer;
    public ConsoleWriter Writter;
    private DisplaySystem _screen;
    private FileController _fileController;
    private NotePad _note;
    private CommandBuffer _buffer;
    private CommandRunner _runner;
    private DynamicCode _dynamicCode;
    private Terminal _terminal;

    private IUpdateble _app;
    private bool _hasApp = false;

    private Timer _clipTimer;
    private bool _clipped = false;
    private float _startClipDelay = 1.3f;
    private float _tickDelay = 0.06f;
    private float _tick;

    private int _acIndex = -1;
    private string _startCommand;
    private string _command = "";

    private Timer _runEventsTimer;
    private float _outDeltaTime = 0.2f;
    private bool _isWriting = false;
    private int _posted = 0;
    private List<string> _linesToOut = new List<string>();
    private List<string> _stateLog = new List<string>();
    private List<Action> _actions = new List<Action>();
    private float _actionDelay = 0.3f;

    public ActionInvoker Invoker;
    private bool _autoApply = true;
    public float Delta;
    public string[] DownedKeysList;

    private float _startingTime = START_DELAY;
    private float _startingShow = START_DELAY;
    public bool IsStarting = false;

    private bool _isInputString = false;
    private string _inputString;

    private AudioClip beep;
    private AudioSource dynamic;

    public TerminalConsole(DisplaySystem screen, Terminal terminal) {
        _terminal = terminal;
        _screen = screen;
        Typer = new TextTyper(screen, this);
        _runEventsTimer = new Timer(_outDeltaTime, RunEvents);
        _buffer = new CommandBuffer(10);
        _dynamicCode = new DynamicCode(this, screen);
        _clipTimer = new Timer(_startClipDelay, StartClip);
        _tick = _tickDelay;
        _fileController = new FileController(this);
        _fileController.UpdateDirsList();
        _fileController.UpdateFileList();
        _runner = new CommandRunner(Typer, _fileController, this);

        Writter = new ConsoleWriter(Typer);
        Invoker = new ActionInvoker();

        _screen.BlackScreen();
        _screen.Apply();
    }

    public void SetSound(AudioClip beep, AudioSource dynamic) {
        this.beep = beep;
        this.dynamic = dynamic;
    }

    public void Beep(float volume, float pitch) {
        if (dynamic.isPlaying) return;
        if (volume > 1)
            volume = 1f;
        dynamic.clip = beep;
        dynamic.volume = volume * 0.2f;
        dynamic.pitch = pitch;
        dynamic.Play();
    }

    public void OnPower() {
        IsStarting = true;
        TextAsset code = Resources.Load("start") as TextAsset;

        // draw logo
        Texture2D logo = Resources.Load("logo") as Texture2D;
        int xcenter = (DisplaySystem.WIDTH - logo.width) / 2;
        int ycenter = (DisplaySystem.HEIGHT + logo.height) / 2;
        _screen.SetFontColor(Color.white);
        _screen.SetBackColor(Color.black);
        _screen.Clear();
        _screen.DrawImage(xcenter, ycenter, logo.width, logo.height, logo.GetPixels());

        _runner.RunCode(code.text);
    }

    public void ShutDown() {
        _activeInput = false;
        _screen.SetBackColor(Color.black);
        Typer.Cursor.SetPosition(Typer.borderWidth, Typer.borderHeight);
        _screen.Clear();
        _terminal.SetPowerOn(false);
    }

    public void StartingUpdate() {
        _startingTime -= Time.deltaTime;
        _startingShow -= Time.deltaTime;

        if (_startingShow < 1) {
            _dynamicCode.CloseThreads();
            _screen.Clear();
            _screen.Apply();
        }

        if (_startingTime < 0) {
            _screen.Clear();
            _fileController = new FileController(this);
            _fileController.UpdateDirsList();
            _fileController.UpdateFileList();
            _runner.FileController = _fileController;
            StartScreen();
            NextCommad();
            IsStarting = false;
            _terminal.SetPowerOn(true);
            _startingShow = START_DELAY;
            _startingTime = START_DELAY;
        }

        Delta = Time.deltaTime;
        if (Delta <= 0.01f)
            Delta = 0.01f;
        Invoker.Update();
    }

    public void SetAutoApply(bool a) {
        _autoApply = a;
    }

    public void SetActiveInput(bool active) {
        _activeInput = active;
    }

    public void AddToActions(Action act) {
        _actions.Add(act);
    }

    public void ResetOutput() { // if process aborted
        NextCommad();
        _activeInput = true;
        _autoApply = true;
        Invoker.Reset();
    }

    public void ResetOutputStopped() {
        _isWriting = false;
        _activeInput = true;
        _autoApply = true;
        NextCommad();
    }

    public void GetInput() {
        Invoker.Pause();
        _isInputString = true;
        _inputString = "";
    }

    public void UpdateApp(KeyCode key) {
        switch (key) {
            case KeyCode.LeftArrow:
            case KeyCode.RightArrow:
            case KeyCode.UpArrow:
            case KeyCode.DownArrow:
            case KeyCode.Return:
            case KeyCode.Tab:
            case KeyCode.Delete:
            case KeyCode.Backspace:
            case KeyCode.Home:
            case KeyCode.End:
                _app.Action(key.ToString());
                return;
            case KeyCode.Q:
                if (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) {
                    _app.Action("exit");
                }
                else {
                    _app.Action(Input.inputString);
                }
                return;
            case KeyCode.A:
            case KeyCode.S:
                if (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) {
                    _app.Action("save");
                }
                else {
                    _app.Action(Input.inputString);
                }
                return;
            default:
                _app.Action(Input.inputString);
                return;
        }
    }

    public void OnKey(KeyCode key) {
        switch (key) {
            case KeyCode.Backspace:
            case KeyCode.Delete:
            case KeyCode.LeftArrow:
            case KeyCode.RightArrow:
            case KeyCode.UpArrow:
            case KeyCode.DownArrow:
                if (_hasApp) {
                    _clipTimer.Update();
                    if (_clipped) {
                        _tick -= Time.deltaTime;
                        if (_tick < 0) {
                            _app.Action(key.ToString());
                            _tick = _tickDelay;
                        }
                    }
                }
                return;
            case KeyCode.LeftControl:
            case KeyCode.RightControl:
                if (Input.GetKeyDown(KeyCode.Q) && !_hasApp && _isWriting) {
                    _dynamicCode.CloseThreads();
                    ResetOutput();
                    _isWriting = false;
                }
                return;
        }
    }

    public void OnKeyUp(KeyCode key) {
        switch (key) {
            case KeyCode.Backspace:
            case KeyCode.Delete:
            case KeyCode.LeftArrow:
            case KeyCode.RightArrow:
            case KeyCode.UpArrow:
            case KeyCode.DownArrow:
                _clipped = false;
                _clipTimer.Reset();
                return;
        }
    }

    public void OnKeyDown(KeyCode key) {
        if (_isInputString && key == KeyCode.Return) {
            _isInputString = false;
            _dynamicCode.GetInputResult(_inputString);
            Typer.Cursor.NextLine();
            Invoker.CanAdd();
        }
        if (_hasApp) UpdateApp(key);
        if (_canInputCommands) {
            switch (key) {
                case KeyCode.LeftArrow:
                    Typer.MoveCursor(true);
                    return;
                case KeyCode.RightArrow:
                    Typer.MoveCursor(false);
                    return;
                case KeyCode.UpArrow:
                    GetBufferLine(true);
                    return;
                case KeyCode.DownArrow:
                    GetBufferLine(false);
                    return;
                case KeyCode.Tab:
                    try {
                        AutoComplete();
                    }
                    catch {

                    }
                    return;
                case KeyCode.Return:
                    OnCommandEnter(_command, true);
                    return;
            }
        }
    }

    public void OnKeyDown(string key) {
        if (_isInputString && key != "\b") {
            _inputString += key;
            Typer.Print(key);
            _screen.Apply();
        }
        if (_canInputCommands) {
            if (key.Length > 0) {
                KeyDownProcessor(key[0]);
                ResetAutocompleteIndex();
            }
        }
    }

    public void KeyDownProcessor(char key) {
        if (key == 13) return;
        CursorController cursor = Typer.Cursor;
        if (key == '\b') {
            if (_command.Length + 1 == Typer.PositionX) {
                Typer.MoveCursor(true);
                _screen.TypeChar(0, cursor.x, cursor.y);
                _screen.Apply();
                if (!string.IsNullOrEmpty(_command)) {
                    DeleteLastChar();
                }
            }
            else {
                Typer.MoveCursor(true);
                _command = _command.Substring(0, Typer.PositionX - 1) + _command.Substring(Typer.PositionX);
                int x = cursor.x;
                cursor.SetPosition(Typer.borderWidth, cursor.y);
                Typer.Print(">" + _command + " ");
                cursor.SetPosition(x, cursor.y);
                _screen.Apply();
            }
        }
        else if (_command.Length + 1 == Typer.PositionX) {
            if (cursor.x == DisplaySystem.COLS - 1 - Typer.borderWidth) return;
            _screen.TypeChar(Utils.GetCharNumber(key), cursor.x, cursor.y);
            cursor.Next();
            _command = _command + key;
            _screen.Apply();
        }
        else if (!string.IsNullOrEmpty(_command)) {
            if (_command.Length > DisplaySystem.COLS - 3 - Typer.borderWidth) return;
            InsertToString(key);
        }
    }

    public void Update() {

        Delta = Time.deltaTime;

        _runEventsTimer.Update();
        Invoker.Update();

        if (_autoApply) Typer.Cursor.Update();
        if (_canInputCommands) Typer.Update();

        if (_actions.Count != 0) {
            if (_actionDelay > 0) {
                _actionDelay -= Time.deltaTime;
                return;
            }
            foreach (Action act in _actions) {
                act();
            }
            _actions.Clear();
            _actionDelay = 0.3f;
        }
    }

    void StartClip() {
        _clipped = true;
    }

    public void AddToLog(string text) {
        _stateLog.Add(text);
    }

    public void SetWriting(bool writing) {
        _isWriting = writing;
    }

    public void AddToApp(object up) {
        _app = (IUpdateble)up;
        _hasApp = true;
    }

    public void RemoveApp() {
        _app = null;
        _hasApp = false;
    }

    public void RunEvents() {

        Writter.WriteOutputs();

        if (_stateLog.Count != 0) {
            Color tmp = _screen.GetFontColor();
            _screen.SetFontColor(Typer.errorColor);
            foreach (string error in _stateLog) {
                Typer.Print(error);
                Typer.Cursor.NextLine();
            }
            _screen.SetFontColor(tmp);
            _stateLog.Clear();
            _screen.Apply();
        }
    }

    internal void CreateThread(string code) {
        _dynamicCode.CreateThread(code);
    }

    public void StartScreen() {
        _isWriting = false;
        _activeInput = true;
        _autoApply = true;
        _actions.Clear();
        _stateLog.Clear();
        _runner.StartRunCommand("defcolors", new string[0]);
        Typer.Print("");
        Typer.Cursor.NextLine();
        Typer.Print(Utils.StringToCenter("- CRECAT CORP TERMINAL -", DisplaySystem.COLS - Typer.borderWidth * 2));
        Typer.Cursor.NextLine();
        Typer.Print("");
    }

    void Clear() {
        Typer.Cursor.SetPosition(Typer.borderWidth, Typer.borderHeight);
        _screen.Clear();
        Typer.DrawBorder();
        _command = "";
        NextCommad();
        _screen.Apply();
    }

    void TrimCommand(ref string com) {
        char[] charsToTrim = { ' ', '\0' };
        com = com.Trim(charsToTrim);
    }

    void PrintOutputs(Result results) {

        if (results.outLines.Count != 0) {
            Typer.PrintLines(results.outLines.ToArray());
            Typer.Cursor.NextLine();
        }

        if (results.errors.Count != 0) {
            Color tmp = _screen.GetFontColor();
            _screen.SetFontColor(Typer.errorColor);
            Typer.PrintLines(results.errors.ToArray());
            Typer.Cursor.NextLine();
            _screen.SetFontColor(tmp);
        }        
    }

    public void OnCommandEnter(string command, bool addBuffer) {
        _buffer.Reset();
        if (addBuffer) _buffer.Add(command);
        _canInputCommands = false;
        TrimCommand(ref command);
        if (string.IsNullOrEmpty(command)) {
            Typer.Cursor.NextLine();
            NextCommad();
            return;
        }
        else if (command == "clear") {
            Clear();
            return;
        }
        Typer.Cursor.NextLine();
        PrintOutputs(ProcessingCommand(command));
        NextCommad();
    }

    public void NextCommad() {
        _command = "";
        if (_activeInput) {
            Typer.Print(">");
            _canInputCommands = true;
        }
        _screen.Apply();
    }

    void AutoComplete() {
        _acIndex++;
        if (_acIndex == 0) _startCommand = _command;
        if (_command.StartsWith("cd ") || _command.StartsWith("rmdir ") || _command.StartsWith("cpdir ")) {
            _command = _fileController.Autocomplete(_startCommand, _acIndex, false);
        }
        else if (_command.StartsWith("rm ") || _command.StartsWith("edit ") || _command.StartsWith("ren ") || _command.StartsWith("cp ")
            || _command.StartsWith("run ")) {
            _command = _fileController.Autocomplete(_startCommand, _acIndex, true);
        }
        else {
            _command = CommandHelper.Autocomplete(_startCommand, _acIndex);
        }
        if (_command == _startCommand) _acIndex = -1;
        UpdateLine();
    }

    void UpdateLine() {
        _screen.ClearLine(Typer.Cursor.y, Typer.borderWidth, Typer.borderWidth);
        Typer.Cursor.SetPosition(Typer.borderWidth, Typer.Cursor.y);
        Typer.Print(">" + _command);
        _screen.Apply();
    }

    public void ResetAutocompleteIndex() {
        _acIndex = -1;
    }

    public void GetBufferLine(bool up) {
        if (up)
            _buffer.Up();
        else
            _buffer.Down();
        string buf = _buffer.GetLine();
        if (!string.IsNullOrEmpty(buf)) {
            _screen.ClearLine(Typer.Cursor.y, Typer.borderWidth, Typer.borderWidth);
            _screen.Apply();
            Typer.Cursor.SetPosition(Typer.borderWidth, Typer.Cursor.y);
            Typer.Print(">" + buf);
            _command = buf;
            _screen.Apply();
        }
    }

    public void InsertToString(char key) {
        if (_screen.GetLineLength(Typer.Cursor.y) != DisplaySystem.COLS - Typer.borderWidth - 1) {
            _command = _command.Insert(Typer.Cursor.x - 1 - Typer.borderWidth, key.ToString());
            int x = Typer.Cursor.x;
            Typer.Cursor.SetPosition(Typer.borderWidth, Typer.Cursor.y);
            Typer.Print(">" + _command);
            Typer.Cursor.x = x;
            Typer.Cursor.Next();
            _screen.Apply();
        }
    }

    public void DeleteLastChar() {
        _command = _command.Substring(0, _command.Length - 1);
    }

    public Result ProcessingCommand(string text) {
        string[] words = text.Split(' ');
        string command = words[0];
        string[] args;
        if (words.Length > 1) {
            args = new string[words.Length - 1];
            for (int i = 1; i < words.Length; i++) {
                args[i - 1] = words[i];
            }
        }
        else {
            args = new string[0];
        }

        return _runner.StartRunCommand(command, args);
    }

    public void Close() {
        _dynamicCode.CloseThreads();
    }

}
