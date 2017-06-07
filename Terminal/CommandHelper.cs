using System.Collections.Generic;

public static class CommandHelper  {

    public static bool init = false;

    struct Command {
        public string n;
        public List<string> desc;
        public Command(string n) {
            this.n = n;
            desc = new List<string>();
        }
    }
    static private List<Command> commands = new List<Command>();

    public static void Init() {
        Command cmd = new Command("help");
        cmd.desc.Add("help [command]");
        cmd.desc.Add("Show list of commands");
        cmd.desc.Add("Show help for command");
        commands.Add(cmd);
        cmd = new Command("clear");
        cmd.desc.Add("Clear the screen");
        commands.Add(cmd);
        cmd = new Command("edit");
        cmd.desc.Add("edit <file>");
        cmd.desc.Add("Edit the file");
        cmd.desc.Add("Ctrl + Q to exit");
        cmd.desc.Add("Ctrl + S to save file");
        commands.Add(cmd);
        cmd = new Command("echo");
        cmd.desc.Add("echo <text>");
        cmd.desc.Add("Show the message");
        commands.Add(cmd);
        cmd = new Command("run");
        cmd.desc.Add("run <file>");
        cmd.desc.Add("Run Python code");
        commands.Add(cmd);
        cmd = new Command("ren");
        cmd.desc.Add("ren <file> <name>");
        cmd.desc.Add("Rename file");
        commands.Add(cmd);
        cmd = new Command("ls");
        cmd.desc.Add("ls [keys]");
        cmd.desc.Add("Show files & dirs in current folder");
        cmd.desc.Add("Use key 'f' to show files only");
        cmd.desc.Add("Use key 'd' to show folders only");
        commands.Add(cmd);
        cmd = new Command("im");
        cmd.desc.Add("im[name]");
        cmd.desc.Add("Show or set username");
        commands.Add(cmd);
        cmd = new Command("resetcolors");
        cmd.desc.Add("Reset colors of display");
        commands.Add(cmd);
        cmd = new Command("cd");
        cmd.desc.Add("cd <dir>");
        cmd.desc.Add("Change directory");
        cmd.desc.Add("Use '..' for move back");
        cmd.desc.Add("Use '/' for move to root");
        commands.Add(cmd);
        cmd = new Command("where");
        cmd.desc.Add("Show path where you are");
        commands.Add(cmd);
        cmd = new Command("mkdir");
        cmd.desc.Add("Make new directory");
        commands.Add(cmd);
        cmd = new Command("cpdir");
        cmd.desc.Add("cpdir <dir> <to>");
        cmd.desc.Add("Copy the directory");
        commands.Add(cmd);
        cmd = new Command("rmdir");
        cmd.desc.Add("rmdir <dir>");
        cmd.desc.Add("Remove the directory");
        commands.Add(cmd);
        cmd = new Command("cf");
        cmd.desc.Add("cf <file>");
        cmd.desc.Add("Create new file");
        commands.Add(cmd);
        cmd = new Command("rm");
        cmd.desc.Add("rm <file>");
        cmd.desc.Add("Remove the file");
        commands.Add(cmd);
        cmd = new Command("cp");
        cmd.desc.Add("cp <file> <to>");
        cmd.desc.Add("Copy the file");
        commands.Add(cmd);
        cmd = new Command("color");
        cmd.desc.Add("color <r> <g> <b>");
        cmd.desc.Add("Set font color");
        cmd.desc.Add("Values [0..255]");
        commands.Add(cmd);
        cmd = new Command("colorbg");
        cmd.desc.Add("colorbg <r> <g> <b>");
        cmd.desc.Add("Set background color");
        cmd.desc.Add("Values [0..255]");
        commands.Add(cmd);
        cmd = new Command("colorborder");
        cmd.desc.Add("colorborder <r> <g> <b>");
        cmd.desc.Add("Set border color");
        cmd.desc.Add("Values [0..255]");
        commands.Add(cmd);
        cmd = new Command("colorerror");
        cmd.desc.Add("colorerror <r> <g> <b>");
        cmd.desc.Add("Set error message color");
        cmd.desc.Add("Values [0..255]");
        commands.Add(cmd);
        cmd = new Command("defcolors");
        cmd.desc.Add("Set default values of colors");
        cmd.desc.Add("from sys.cols");
        commands.Add(cmd);
        cmd = new Command("shutdown");
        cmd.desc.Add("Shut system");
        commands.Add(cmd);

        init = true;
    }

    public static string Autocomplete(string start, int index) {
        foreach (Command word in commands) {
            if (word.n.StartsWith(start)) {
                if (index != 0) {
                    index--;
                    continue;
                }
                return word.n;
            }
        }
        return start;
    }

    public static string[] GetHelp(string page) {
        bool exist = false;
        List<string> des = new List<string>();
        List<string> res = new List<string>();
        foreach (Command com in commands) {
            if (com.n == page) {
                exist = true;
                des = com.desc;
                break;
            }
        }
        if (exist) {
            res.AddRange(des);
        }
        else {
            string[] outList = new string[commands.Count / 2 + commands.Count % 2];
            bool even = commands.Count % 2 == 0;
            int sep = (DisplaySystem.COLS - 6) / 2;
            int j = 0;
            for (int i = 0; i < outList.Length - 1; i++) {
                outList[i] = commands[j].n.PadRight(sep) + commands[j + 1].n.PadRight(sep);
                j += 2;
            }
            if (even) {
                outList[outList.Length - 1] = commands[j].n.PadRight(sep) + commands[j + 1].n.PadRight(sep);
            }
            else {
                outList[outList.Length - 1] = commands[j].n.PadRight(sep) + " ".PadRight(sep);
            }
            res.Add("Type \"help <com>\" to more info");
            res.AddRange(outList);
        }
        return res.ToArray();
    }

}
