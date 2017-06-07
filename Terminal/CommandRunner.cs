using System.Text;
using UnityEngine;

namespace TerminalSystem {

    public class CommandRunner  {

        private string _username = "User";

        public FileController FileController;
        private TextTyper _typer;
        private TerminalConsole _console;

        public CommandRunner(TextTyper typer, FileController fileController, TerminalConsole console) {
            _console = console;
            _typer = typer;
            FileController = fileController;
        }

        public Result StartRunCommand(string command, string[] args) {
            Result res = new Result();
            bool haveArgs = args.Length > 0;

            switch (command) {
                case "help":
                    if (haveArgs) res.outLines.AddRange(CommandHelper.GetHelp(args[0]));
                    else res.outLines.AddRange(CommandHelper.GetHelp(""));
                    return res;
                case "ls":
                    return FileController.List(args);
                case "echo":
                    return new Result(new string[] { string.Join(" ", args) }, new string[0]);
                case "im":
                    if (haveArgs)
                        _username = args[0];
                    res.outLines.Add("You are " + _username);
                    return res;
                case "resetcolors":
                    _typer.Screen.ResetColors();
                    _typer.ResetScreen(true);
                    return res;
                case "edit":
                    return OpenEditor(args);
                case "cd":
                    return ChangeDirectory(args);
                case "where":
                    res.outLines.Add(FileController.invite);
                    return res;
                case "mkdir":
                    return MakeDirectory(args);
                case "cf":
                    return CreateFile(args);
                case "ren":
                    return RenameFiles(args);
                case "run":
                    return Run(args);
                case "cpdir":
                    return CopyDirectory(args);
                case "rmdir":
                    return RenameDir(args);
                case "rm":
                    return RemoveFiles(args);
                case "cp":
                    return CopyFiles(args);
                case "color":
                    return ColorCommand(args);
                case "colorbg":
                    return ColorBG(args);
                case "colorborder":
                    return ColorBorder(args);
                case "colorerror":
                    return ColorError(args);
                case "defcolors":
                    return DefineColors();
                case "shutdown":
                    return ShutDown();
                default:
                    res.errors.Add("Undefined " + command + " Try 'help'");
                    return res;
            }
        }

        private Result ColorError(string[] args) {
            Result res = new Result();
            if (args.Length >= 3) {
                try {
                    Color c = Utils.ParseColor(args[0], args[1], args[2]);
                    _typer.SetErrorColor(c);
                    _typer.ResetScreen(true);
                    return res;
                }
                catch {
                    res.errors.Add("invalid values");
                }
            }
            else {
                res.outLines.Add("colorerror <r> <g> <b>");
            }
            return res;
        }

        private Result ColorBorder(string[] args) {
            Result res = new Result();
            if (args.Length >= 3) {
                try {
                    Color c = Utils.ParseColor(args[0], args[1], args[2]);
                    _typer.SetBorderColor(c);
                    _typer.ResetScreen(true);
                    return res;
                }
                catch {
                    res.errors.Add("invalid values");
                }
            }
            else {
                res.outLines.Add("colorborder <r> <g> <b>");
            }
            return res;
        }

        private Result ColorBG(string[] args) {
            Result res = new Result();
            if (args.Length >= 3) {
                try {
                    Color c = Utils.ParseColor(args[0], args[1], args[2]);
                    _typer.Screen.SetBackColor(c);
                    _typer.ResetScreen(true);
                    return res;
                }
                catch {
                    res.errors.Add("invalid values");
                }
            }
            else {
                res.outLines.Add("colorbg <r> <g> <b>");
            }
            return res;
        }

        private Result ColorCommand(string[] args) {
            Result res = new Result();
            if (args.Length >= 3) {
                try {
                    Color c = Utils.ParseColor(args[0], args[1], args[2]);
                    _typer.Screen.SetFontColor(c);
                    _typer.ResetScreen(true);
                    return res;
                }
                catch {
                    res.errors.Add("invalid values");
                }
            }
            else {
                res.outLines.Add("color <r> <g> <b>");
            }
            return res;
        }

        private Result CopyFiles(string[] args) {
            Result res = new Result();
            if (args.Length >= 2) {
                return FileController.CopyFile(args[0], args[1]);
            }
            else {
                res.outLines.Add("cp <from> <to>");
            }
            return res;
        }

        private Result RemoveFiles(string[] args) {
            Result res = new Result();
            if (args.Length != 0) {
                return FileController.DeleteFile(args[0]);
            }
            else {
                res.outLines.Add("rm <file>");
            }
            return res;
        }

        private Result RenameDir(string[] args) {
            Result res = new Result();
            if (args.Length != 0) {
                return FileController.DeleteDirectory(args[0]);
            }
            else {
                res.outLines.Add("rmdir <dir>");
            }
            return res;
        }

        private Result CopyDirectory(string[] args) {
            Result res = new Result();
            if (args.Length >= 2) {
                string path = new StringBuilder().Append(FileController.dataPath).Append(FileController.invite).ToString();
                return FileController.CopyDirectory(path + args[0], path + args[1], true);
            }
            else {
                res.outLines.Add("cpdir <from> <to>");
            }
            return res;
        }

        private Result Run(string[] args) {
            Result res = new Result();
            if (args.Length != 0) {
                string path = FileController.dataPath + FileController.invite;
                try {
                    string[] code = FileController.OpenFile(path + args[0]).outLines.ToArray();
                    return RunCode(code);
                }
                catch {
                    res.errors.Add("Can't run file");
                }
                return res;
            }
            else {
                res.outLines.Add("run <file>");
            }
            return res;
        }

        private Result RenameFiles(string[] args) {
            Result res = new Result();
            if (args.Length >= 2) {
                return FileController.RenameFile(args[0], args[1]);
            }
            else {
                res.outLines.Add("ren <file> <name>");
            }
            return res;
        }

        private Result CreateFile(string[] args) {
            Result res = new Result();
            if (args.Length != 0) {
                return FileController.CreateFile("", args[0]);
            }
            else {
                res.outLines.Add("cf <file>");
            }
            return res;
        }

        private Result MakeDirectory(string[] args) {
            Result res = new Result();
            if (args.Length != 0) {
                return FileController.CreateDirectory(args[0]);
            }
            else {
                res.outLines.Add("mkdir <dir>");
            }
            return res;
        }

        private Result DefineColors() {
            Result res = new Result();
            string path = FileController.dataPath + "/sys/cols";
            try {
                Result file = FileController.OpenFile(path);
                if (file.errors.Count > 0) {
                    res.errors.AddRange(file.errors);
                }
                foreach (string line in file.outLines) {
                    if (line.StartsWith("cl:")) {
                        string[] cols = line.Split(' ');
                        Color c = Utils.ParseColor(cols[1], cols[2], cols[3]);
                        _typer.Screen.SetFontColor(c);
                    }
                    if (line.StartsWith("bg:")) {
                        string[] cols = line.Split(' ');
                        Color c = Utils.ParseColor(cols[1], cols[2], cols[3]);
                        _typer.Screen.SetBackColor(c);
                    }
                    if (line.StartsWith("br:")) {
                        string[] cols = line.Split(' ');
                        Color c = Utils.ParseColor(cols[1], cols[2], cols[3]);
                        _typer.SetBorderColor(c);
                    }
                    if (line.StartsWith("er:")) {
                        string[] cols = line.Split(' ');
                        Color c = Utils.ParseColor(cols[1], cols[2], cols[3]);
                        _typer.SetErrorColor(c);
                    }
                }
                _typer.ResetScreen(true);
                return res;
            }
            catch {
                res.errors.Add("Invalid data");
            }
            return res;
        }

        private Result OpenEditor(string[] args) {
            Result res = new Result();
            if (args.Length != 0) {
                NotePad note = new NotePad(_typer);
                string path = FileController.dataPath + FileController.invite + args[0];
                return note.OpenFile(path);
            }
            else {
                res.outLines.Add("edit <file>");
            }
            return res;
        }

        private Result ChangeDirectory(string[] args) {
            Result res = new Result();
            if (args.Length != 0) {
                return FileController.ChangeDirectory(args[0]);
            }
            else {
                res.outLines.Add("cd <dir>");
            }
            return res;
        }

        public Result RunCode(string[] code) {
            _console.Invoker.CanAdd();
            _console.SetWriting(true);
            _console.CreateThread(string.Join("\n", code));
            _typer.SetConsoleActive(false);
            return new Result();
        }

        private Result ShutDown() {
            _console.ShutDown();
            return new Result();
        }

        public void RunCode(string code) {
            _console.Invoker.CanAdd();
            _console.SetWriting(true);
            _console.CreateThread(code);
            _typer.SetConsoleActive(false);
        }

    }

}