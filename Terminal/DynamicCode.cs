using UnityEngine;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Threading;
using System.IO;
using System.Security.Cryptography;

public class DynamicCode {

    private static ScriptEngine engine;
    private ScriptScope scope;
    private Thread stream;
    private bool aborted;
    private TerminalConsole _console;
    private DisplaySystem _display;

    private RNGCryptoServiceProvider _rand = new RNGCryptoServiceProvider();

    private bool hasResult;
    private string result;

    public DynamicCode(TerminalConsole console, DisplaySystem display) {
        _console = console;
        _display = display;
    }

    public void CreateThread(string code) {
        CodeWraper wraper = new CodeWraper(code, _console);
        stream = new Thread(ScriptRun);
        stream.IsBackground = true;
        stream.Start(wraper);
    }

    public void ScriptRun(object obj) {
        engine = Python.CreateEngine();
        CodeWraper wraper = (CodeWraper) obj;
        string code = wraper.GetCode();

        scope = engine.CreateScope();
        var streamOut = new MemoryStream();

        scope.SetVariable("WIDTH", DisplaySystem.WIDTH);
        scope.SetVariable("HEIGHT", DisplaySystem.HEIGHT);
        scope.SetVariable("__tick__", new Action(Tick));
        scope.SetVariable("set_color", new Action<int, int, int>(SetColor));
        scope.SetVariable("set_back", new Action<int, int, int>(SetBackColor));
        scope.SetVariable("set_bord", new Action<int, int, int>(SetBorderColor));
        scope.SetVariable("set_error", new Action<int, int, int>(SetErrorColor));
        scope.SetVariable("randint", new Func<int, int, int>(RandomInteger));
        scope.SetVariable("clear_screen", new Action(ClearScreen));
        scope.SetVariable("draw_circle", new Action<int, int, int>(DrawCircle));
        scope.SetVariable("draw_line", new Action<int, int, int, int>(DrawLine));
        scope.SetVariable("draw_rect", new Action<int, int, int, int>(DrawRect));
        scope.SetVariable("draw_pixel", new Action<int, int>(DrawPixel));
        scope.SetVariable("draw_pixels", new Action<int, int, int, int>(DrawPixels));
        scope.SetVariable("auto_apply", new Action<bool>(SetAutoapply));
        scope.SetVariable("apply", new Action(Apply));
        scope.SetVariable("delta_time", new Func<float>(GetDeltaTime));
        scope.SetVariable("get_keys", new Func<string[]>(GetDownedKeys));
        scope.SetVariable("char", new Func<string, int>(GetCharByNum));
        scope.SetVariable("type", new Action<int, int, int>(TyperChar));
        scope.SetVariable("input", new Func<string>(GetInput));
        scope.SetVariable("beep", new Action<float, float>(Beep));

        aborted = false;

        EventStreamWriter outputWr = new EventStreamWriter(streamOut);
        outputWr.StringWritten += new EventHandler<EventWithArgs<string>>(StringWritten);

        engine.Runtime.IO.SetOutput(streamOut, outputWr);

        try {
            engine.Execute(code, scope);
        }
        catch (Exception e) {
            _console.AddToLog(e.Message);
        }

        if (aborted) {
            _console.Writter.Reset();
            _console.AddToActions(_console.ResetOutput);
        }
        else
            _console.AddToActions(_console.ResetOutputStopped);

        stream.Abort();
    }

    public void StringWritten(object sender, EventWithArgs<string> e) {
        _console.Writter.AddToOutput(e.Value);
    }

    public void Tick() {
        if (aborted)
            throw new Exception("Aborted");
    }

    public void CloseThreads() {
        aborted = true;
        Thread.Sleep(10);
        if (stream != null)
            stream.Abort();
    }

    string[] GetDownedKeys() {
        return _console.DownedKeysList;
    }

    void Beep(float volume, float pitch) {
        _console.Invoker.Add(() => _console.Beep(volume, pitch));
    }

    float GetDeltaTime() {
        Debug.Log(_console.Delta);
        return _console.Delta;
    }

    string GetInput(string text) {
        _console.Invoker.Add(() => _console.Typer.Print(text));
        return GetInput();
    }

    string GetInput() {
        hasResult = false;
        _console.Invoker.Add(_console.GetInput);
        while (!hasResult) {
            Thread.Sleep(10);
        }
        return result;
    }

    public void GetInputResult(string res) {
        result = res;
        hasResult = true;
    }

    void TyperChar(int x, int y, int c) {
        _console.Invoker.Add(() => _display.TypeChar(c, x, y));
    }

    int GetCharByNum(string ch) {
        if (ch.Length != 0)
            return Utils.GetCharNumber(ch[0]);
        else
            return 0;
    }

    void SetAutoapply(bool turn) {
        _console.SetAutoApply(turn);
    }

    void Apply() {
        _console.Invoker.Add(_display.Apply);
    }

    void DrawPixel(int x, int y) {
        _console.Invoker.Add(() => _display.DrawPixel(x, y, _display.GetFontColor()));
    }

    void DrawPixels(int x, int y, int width, int height) {
        _console.Invoker.Add(() => _display.DrawPixels(x, y, width, height, _display.GetFontColor()));
    }

    void DrawRect(int x0, int y0, int width, int height) {
        _console.Invoker.Add(() => _display.DrawRectangle(x0, y0, width, height, _display.GetFontColor()));
    }

    void DrawLine(int x0, int y0, int x1, int y1) {
        _console.Invoker.Add(() => _display.DrawLine(x0, y0, x1, y1, _display.GetFontColor()));
    }

    void DrawCircle(int x, int y, int r) {
        _console.Invoker.Add(() => _display.DrawCircle(x, y, r, _display.GetFontColor()));
    }

    void ClearScreen() {
        _console.Invoker.Add(_display.Clear);
    }

    void SetColor(int r, int g, int b) {
        Color c = Utils.ParseColor(r, g, b);
        _console.Invoker.Add(() => _display.SetFontColor(c));
    }

    void SetBackColor(int r, int g, int b) {
        Color c = Utils.ParseColor(r, g, b);
        _console.Invoker.Add(() => _display.SetBackColor(c));
    }

    void SetBorderColor(int r, int g, int b) {
        Color c = Utils.ParseColor(r, g, b);
        _console.Invoker.Add(() => _console.Typer.SetBorderColor(c));
    }

    void SetErrorColor(int r, int g, int b) {
        Color c = Utils.ParseColor(r, g, b);
        _console.Invoker.Add(() => _console.Typer.SetErrorColor(c));
    }

    int RandomInteger(int min, int max) {
        uint scale = uint.MaxValue;
        while (scale == uint.MaxValue) {
            byte[] four_bytes = new byte[4];
            _rand.GetBytes(four_bytes);
            scale = BitConverter.ToUInt32(four_bytes, 0);
        }
        return (int)(min + (max + 1 - min) * (scale / (double)uint.MaxValue));
    }

}
