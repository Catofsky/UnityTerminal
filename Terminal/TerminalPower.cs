using UnityEngine;

public class TerminalPower {

    private bool _on;
    public bool On { get { return _on; } }
    public TerminalConsole _console;

    public TerminalPower(TerminalConsole console) {
        _on = false;
        _console = console;
    }

    public void TurnOn() {
        _on = true;
    }

    public void TurnOff() {
        _on = false;
    }

}
