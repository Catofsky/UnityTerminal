using System.Collections.Generic;
using UnityEngine;

public class CodeWraper {

    private string code;
    private TerminalConsole console;

    public CodeWraper(string code, TerminalConsole console) {
        this.code = code;
        this.console = console;
    }

    public string GetCode() {
        List<string> tmp = new List<string>();
        tmp.AddRange(code.Split('\n'));
        for (int i = 0; i < tmp.Count - 1; i++) {
            if (tmp[i].IndexOf(':') >= 0) {
                if (tmp[i].IndexOf('#') < 0 || (tmp[i].IndexOf('#') > tmp[i].IndexOf(':'))) {
                    int pos = 0;
                    for (int j = 0; j < tmp[i + 1].Length; j++) {
                        if (tmp[i + 1][j] != ' ') {
                            pos = j;
                            break;
                        }
                    }
                    string line = "__tick__()".PadLeft(pos + 10);
                    tmp.Insert(i + 1, line);
                }
            }
        }
        return string.Join("\n", tmp.ToArray());
    }

    public TerminalConsole GetConsole() {
        return console;
    }

}
