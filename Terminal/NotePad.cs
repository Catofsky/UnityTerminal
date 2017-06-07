using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace TerminalSystem {

    public class NotePad : IUpdateble {

        private TextTyper typer;
        private DisplaySystem screen;
        private string openedFile;
        private string path;

        private bool saved = true;

        private struct Position {
            public int x;
            public int y;
            public void Set(int x, int y) {
                this.x = x;
                this.y = y;
            }
        }

        private Position position;
        private Position screenPosition;

        private string fileData;
        private List<string> data = new List<string>();

        public NotePad(TextTyper typer) {
            this.typer = typer;
            screen = typer.Screen;
            position = new Position();
            screenPosition = new Position();
        }

        public Result OpenFile(string path) {
            Result res = new Result();
            try {
                if (File.Exists(path)) {
                    this.path = path;
                    ReadFile(path);
                    ClearPage();
                    typer.SetConsoleActive(false);
                    typer.Console.AddToApp(this);
                    StartDisplayPosition();
                }
                else {
                    res.errors.Add("File is not exist");
                }
            }
            catch {
                res.errors.Add("Can't open file");
            }
            return res;
        }

        void ReadFile(string path) {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            string[] splitted = fs.Name.Split('/');
            openedFile = splitted[splitted.Length - 1];
            byte[] bts = new byte[fs.Length];
            int num = (int)fs.Length;
            int numb = 0;
            while (num > 0) {
                int i = fs.Read(bts, numb, num);
                if (i == 0)
                    break;
                num -= i;
                numb += i;
            }
            fileData = Encoding.UTF8.GetString(bts);
            fs.Close();
            data.AddRange(fileData.Split('\n'));
        }

        void StartDisplayPosition() {
            int numberLine = data.Count;
            int numberCol = data[data.Count - 1].Length;
            int linesCount = 0;
            int colsCount = 0;
            if (numberLine > DisplaySystem.LINES - typer.borderHeight * 2) {
                linesCount = numberLine - (DisplaySystem.LINES - typer.borderHeight * 2) + 2;
            }
            if (numberCol > DisplaySystem.COLS - typer.borderWidth * 2) {
                colsCount = numberCol - (DisplaySystem.COLS - typer.borderWidth * 2) + 2;
            }
            Display(colsCount, linesCount, true);
        }

        private void ClearPage() {
            screen.ResetInvertBlock(typer.Cursor.x, typer.Cursor.y);
            typer.Cursor.SetPosition(typer.borderWidth, typer.borderHeight);
            screen.Clear();
            typer.DrawBorder();
            int step = 4;
            foreach (char c in openedFile) {
                screen.TypeChar(Utils.GetCharNumber(c), step++, 0, screen.GetBackColor(), typer.GetBorderColor());
            }
            step = DisplaySystem.COLS / 2;
            foreach (char c in "ctrl+Q to exit") {
                screen.TypeChar(Utils.GetCharNumber(c), step++, DisplaySystem.LINES - 1, screen.GetBackColor(), typer.GetBorderColor());
            }
            step = DisplaySystem.COLS / 2;
            foreach (char c in saved ? "    saved" : "ctrl+S to save") {
                screen.TypeChar(Utils.GetCharNumber(c), step++, 0, screen.GetBackColor(), typer.GetBorderColor());
            }
        }

        private void UpdateStatusBar() {
            int step = 4;
            string text = "ch " + (position.x + 1) + "  ln " + (position.y + 1) + "  ";
            foreach (char c in text) {
                screen.TypeChar(Utils.GetCharNumber(c), step++, DisplaySystem.LINES - 1, screen.GetBackColor(), typer.GetBorderColor());
            }
        }

        private void Print(string text, int x, int y) {
            if (text.Length >= DisplaySystem.COLS - typer.borderWidth * 2) {
                text = text.Substring(0, DisplaySystem.COLS - typer.borderWidth * 2);
            }
            foreach (char c in text) {
                screen.TypeChar(Utils.GetCharNumber(c), x++, y);
            }
        }

        public void Action(string input) {
            if (input.Length == 1) {
                InsertIntoLine(position.y, position.x, input.ToString());
                saved = false;
                Display(screenPosition.x, screenPosition.y, false);
                PositionRight();
            }
            else switch (input) {
                    case "Return":
                        Return();
                        break;
                    case "LeftArrow":
                        PositionLeft();
                        break;
                    case "RightArrow":
                        PositionRight();
                        break;
                    case "UpArrow":
                        PositionUp();
                        break;
                    case "DownArrow":
                        PositionDown();
                        break;
                    case "Backspace":
                        Back();
                        break;
                    case "Delete":
                        Delete();
                        break;
                    case "End":
                        PositionEnd();
                        break;
                    case "Home":
                        PositionHome();
                        break;
                    case "Tab":
                        Action(" ");
                        Action(" ");
                        Action(" ");
                        Action(" ");
                        break;
                    case "save":
                        Save();
                        break;
                    case "exit":
                        Close();
                        return;
                    default:
                        return;
                }
            UpdateStatusBar();
            screen.Apply();
        }

        private void Save() {
            using (StreamWriter outputFile = new StreamWriter(path)) {
                if (data.Count > 0) {
                    for (int i = 0; i < data.Count - 1; i++) {
                        outputFile.Write(data[i] + '\n');
                    }
                    outputFile.Write(data[data.Count - 1]);
                }
                else {
                    outputFile.WriteLine("");
                }
            }
            saved = true;
            Display(screenPosition.x, screenPosition.y, false);
        }

        public void Close() {
            typer.Cursor.SetPosition(typer.borderWidth, typer.borderHeight);
            screen.Clear();
            typer.DrawBorder();
            typer.SetConsoleActive(true);
            typer.Console.OnCommandEnter("echo File closed", false);
            typer.Console.RemoveApp();
        }

        private void Display(int col, int line, bool setPosition) {
            screenPosition.Set(col, line);
            int x = typer.Cursor.x;
            int y = typer.Cursor.y;
            ClearPage();
            int i = 0;
            while (i < data.Count && i < DisplaySystem.LINES - typer.borderHeight * 2) {
                Print(GetLine(line + i, col), 0 + typer.borderWidth, i + typer.borderHeight);
                i++;
            }
            if (setPosition) {
                int numLines = data.Count == 0 ? 0 : data.Count - 1;
                position.Set(data[data.Count - 1].Length, numLines);
                typer.Cursor.SetPosition(position.x - col + typer.borderWidth, position.y - line + typer.borderHeight);
            }
            else {
                typer.Cursor.SetPosition(x, y);
            }
            UpdateStatusBar();
        }

        private string GetLine(int line, int position) {
            if (line < data.Count && position < data[line].Length) {
                string s = data[line].Substring(position);
                int max = DisplaySystem.COLS - typer.borderWidth * 2 - 1;
                if (s.Length < max) {
                    s = s.PadRight(max);
                }
                return s;
            }
            return "";
        }

        private void InsertIntoLine(int line, int position, string text) {
            data[line] = data[line].Insert(position, text);
        }

        private void AddLine(int line, string text) {
            data.Insert(line, text);
        }

        private void RemoveChar(int line, int position) {
            if (line < data.Count && position < data[line].Length) {
                data[line] = data[line].Remove(position, 1);
            }
        }

        private string RemoveToEnd(int line, int position) {
            string cutted = "";
            if (line < data.Count && position < data[line].Length) {
                cutted = data[line].Substring(position);
                data[line] = data[line].Remove(position);
            }
            return cutted;
        }

        private void DeleteLine(int line) {
            if (line < data.Count) {
                data.RemoveAt(line);
            }
        }

        private void Return() {
            if (position.x == data[position.y].Length) {
                AddLine(position.y + 1, "");
                PositionHome();
            }
            else {
                AddLine(position.y + 1, RemoveToEnd(position.y, position.x));
                PositionHome();
            }
            PositionDown();
            saved = false;
            Display(screenPosition.x, screenPosition.y, false);
        }

        private void PositionLeft() {
            if (typer.Cursor.x == typer.borderWidth && position.x > 0) {
                Display(screenPosition.x - 1, screenPosition.y, false);
                position.x--;
            }
            else {
                if (position.x == 0) {
                    if (position.y == 0) {
                        return;
                    }
                    else {
                        position.y--;
                        position.x = data[position.y].Length;
                        if (position.x >= DisplaySystem.COLS - typer.borderWidth * 2) {
                            Display(position.x - (DisplaySystem.COLS - typer.borderWidth * 2 - 1), screenPosition.y, false);
                        }
                    }
                }
                else {
                    position.x--;
                }
            }
            SetCursorPosition();
        }

        private void PositionRight() {
            if (typer.Cursor.x == DisplaySystem.COLS - typer.borderWidth - 1 && position.x < data[position.y].Length) {
                Display(screenPosition.x + 1, screenPosition.y, false);
                position.x++;
            }
            else {
                if (position.x == data[position.y].Length) {
                    if (position.y == data.Count - 1) {
                        return;
                    }
                    else {
                        position.y++;
                        position.x = 0;
                        Display(0, screenPosition.y, false);
                    }
                }
                else {
                    position.x++;
                }
            }
            SetCursorPosition();
        }

        private void PositionUp() {
            if (typer.Cursor.y == typer.borderHeight && position.y > 0) {
                Display(screenPosition.x, screenPosition.y - 1, false);
                position.y--;
            }
            else {
                if (position.y == 0) {
                    return;
                }
                else {
                    position.y--;
                    if (position.x > data[position.y].Length) {
                        position.x = data[position.y].Length;
                        if (position.x > DisplaySystem.COLS - typer.borderWidth * 2) {
                            Display(position.x - (DisplaySystem.COLS - typer.borderWidth * 2 - 1), screenPosition.y, false);
                        }
                        else {
                            Display(0, screenPosition.y, false);
                        }
                    }
                }
            }
            SetCursorPosition();
        }

        private void PositionDown() {
            if (typer.Cursor.y == DisplaySystem.LINES - typer.borderHeight - 1 && position.y < data.Count - 1) {
                Display(screenPosition.x, screenPosition.y + 1, false);
                position.y++;
            }
            else {
                if (position.y == data.Count - 1) {
                    return;
                }
                else {
                    position.y++;
                    if (position.x > data[position.y].Length) {
                        position.x = data[position.y].Length;
                        if (position.x > DisplaySystem.COLS - typer.borderWidth * 2) {
                            Display(position.x - (DisplaySystem.COLS - typer.borderWidth * 2 - 1), screenPosition.y, false);
                        }
                        else {
                            Display(0, screenPosition.y, false);
                        }
                    }
                }
            }
            SetCursorPosition();
        }

        private void PositionHome() {
            position.x = 0;
            Display(0, screenPosition.y, false);
            SetCursorPosition();
        }

        private void PositionEnd() {
            position.x = data[position.y].Length;
            if (position.x > DisplaySystem.COLS - typer.borderWidth * 2) {
                Display(position.x - (DisplaySystem.COLS - typer.borderWidth * 2 - 1), screenPosition.y, false);
            }
            SetCursorPosition();
        }

        private void SetCursorPosition() {
            typer.Cursor.SetPosition(position.x - screenPosition.x + typer.borderWidth, position.y - screenPosition.y + typer.borderHeight);
            typer.Cursor.Flick();
        }

        private void Delete() {
            if (position.x == data[position.y].Length && position.y < data.Count - 1) {
                ClayTwoLines();
                DeleteLine(position.y + 1);
                StateUpdate();
            }
            else {
                RemoveChar(position.y, position.x);
                StateUpdate();
            }
        }

        private void ClayTwoLines() {
            data[position.y] = data[position.y] + data[position.y + 1];
        }

        private void StateUpdate() {
            saved = false;
            Display(screenPosition.x, screenPosition.y, false);
        }

        private void Back() {
            if (position.x == data[position.y].Length && data[position.y].Length != 0) {
                PositionLeft();
                RemoveChar(position.y, position.x);
                StateUpdate();
            }
            else if (position.x == data[position.y].Length && position.y < data.Count - 1) {
                PositionLeft();
                ClayTwoLines();
                DeleteLine(position.y + 1);
                StateUpdate();
            }
            else {
                if (position.x == 0) {
                    if (position.y == 0) {
                        return;
                    }
                    PositionLeft();
                    ClayTwoLines();
                    DeleteLine(position.y + 1);
                }
                else {
                    PositionLeft();
                    RemoveChar(position.y, position.x);
                }
                StateUpdate();
            }
        }

    }

}