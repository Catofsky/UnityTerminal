using UnityEngine;
using TerminalSystem;
using System.Linq;

public class TextTyper {

    public DisplaySystem Screen;
    public TerminalConsole Console;
    public CursorController Cursor;
    private KeysController keys;

    public int borderWidth = 2;
    public int borderHeight = 1;
    private Color borderColor = new Color(0.2f, 0.2f, 0.7f);
    public Color errorColor = new Color(0.9f, 0.1f, 0.1f);

    public TextTyper(DisplaySystem screen, TerminalConsole console) {
        Screen = screen;
        Console = console;

        Cursor = new CursorController(screen, borderWidth, borderHeight);
        keys = new KeysController(this);
        
        DrawBorder();
    }

    public int PositionX {
        get {
            return Cursor.x - borderWidth;
        }
    }

    public void Update() {
        keys.Update();
    }

    public void SetConsoleActive(bool active) {
        Console.SetActiveInput(active);
    }

    public void SetBorderColor(Color c) {
        borderColor = c;
    }

    public Color GetBorderColor() {
        return borderColor;
    }

    public void SetErrorColor(Color c) {
        errorColor = c;
    }

    public void DrawBorder() {
        Color tmp = Screen.GetFontColor();
        Screen.SetFontColor(borderColor);
        for (int i = 0; i < DisplaySystem.COLS; i++) {
            for (int j = 0; j < borderHeight; j++) {
                Screen.TypeChar(1, i, j);
                Screen.TypeChar(1, i, DisplaySystem.LINES - 1 - j);
            }
        }
        for (int i = 0; i < DisplaySystem.LINES; i++) {
            for (int j = 0; j < borderWidth; j++) {
                Screen.TypeChar(1, j, i);
                Screen.TypeChar(1, DisplaySystem.COLS - 1 - j, i);
            }
        }
        Screen.SetFontColor(tmp);
    }

    public void Print(string text) {
        if (text.Length > DisplaySystem.COLS - borderWidth * 2) {
            string[] lines = Utils.Split(text, DisplaySystem.COLS - borderWidth * 2);
            foreach (string line in lines) {
                Print(line);
            }
        }
        else {
            foreach (char i in text) {
                if (i == '\n') {
                    Cursor.NextLine();
                    continue;
                }
                Screen.TypeChar(Utils.GetCharNumber(i), Cursor.x, Cursor.y);
                Cursor.Next();
            }
        }
    }

    public void PrintLines(string[] text) {
        int count = text.Length;

        int linesCount = DisplaySystem.LINES - borderHeight * 2 - 1;
        if (count > linesCount) {
            string[] outLines = text.Skip(count - linesCount).ToArray();
            ResetScreen(false);
            Print(string.Join("\n", outLines));
            return;
        }

        if (Cursor.y + count > DisplaySystem.LINES - borderHeight) {
            Cursor.MoveUp(count);
            Screen.ScrollUp(borderWidth, borderHeight, count);
        }

        Print(string.Join("\n", text));
    }

    public void PrintLines(string text) {
        int count = text.Count(f => f == '\n');

        int linesCount = DisplaySystem.LINES - borderHeight * 2 - 1;
        if (count > linesCount) {
            string[] outLines = text.Split('\n').Skip(count - linesCount).ToArray();
            ResetScreen(false);
            Print(string.Join("\n", outLines));
            return;
        }

        if (Cursor.y + count > DisplaySystem.LINES - borderHeight) {
            Cursor.MoveUp(count);
            Screen.ScrollUp(borderWidth, borderHeight, count);
        }

        Print(text);
    }

    void Scrolling(int count) {
        if (Cursor.y + count > DisplaySystem.LINES - borderHeight) {
            Cursor.MoveUp(count);
            Screen.ScrollUp(borderWidth, borderHeight, count);
        }
    }

    public void MoveCursor(bool left) {
        if (left) {
            if (Cursor.x > 1 + borderWidth) {
                Cursor.SetPosition(Cursor.x - 1, Cursor.y);
                Screen.Apply();
            }
        }
        else {
            if (Screen.GetLineLength(Cursor.y) > Cursor.x) {
                Cursor.SetPosition(Cursor.x + 1, Cursor.y);
                Screen.Apply();
            }
        }
    }

    public void OnKeyDown(char key) {
        Console.KeyDownProcessor(key);
    }

    public void Apply() {
        Screen.Apply();
    }

    public void ResetScreen(bool apply) {
        Screen.Clear();
        Cursor.SetPosition(borderWidth, borderHeight);
        DrawBorder();
        Screen.Apply();
    }

}
