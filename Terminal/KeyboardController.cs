using System;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardController {

    private Array _keys = Enum.GetValues(typeof(KeyCode));
    private TerminalConsole _console;

    public KeyboardController(TerminalConsole console) {
        _console = console;
    }

    public void Update() {
        List<string> keys = new List<string>();
        foreach (KeyCode key in _keys) {
            if (Input.GetKeyDown(key))
                _console.OnKeyDown(key);
            if (Input.GetKey(key)) {

                switch (key) {
                    case KeyCode.Mouse0:
                    case KeyCode.Mouse1:
                    case KeyCode.Mouse2:
                    case KeyCode.Mouse3:
                    case KeyCode.Mouse4:
                    case KeyCode.Mouse5:
                    case KeyCode.Mouse6:
                        return;
                    case KeyCode.LeftArrow:
                        keys.Add("left");
                        break;
                    case KeyCode.RightArrow:
                        keys.Add("right");
                        break;
                    case KeyCode.UpArrow:
                        keys.Add("up");
                        break;
                    case KeyCode.DownArrow:
                        keys.Add("down");
                        break;
                    default:
                        keys.Add(key.ToString().ToLower());
                        break;
                }
                
                _console.OnKey(key);
            }
            if (Input.GetKeyUp(key))
                _console.OnKeyUp(key);
        }
        _console.DownedKeysList = keys.ToArray();
        if (Input.anyKeyDown) {
            _console.OnKeyDown(Input.inputString);
        }
    }

}