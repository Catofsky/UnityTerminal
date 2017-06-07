using System.Collections.Generic;

public class ConsoleWriter {

    private TextTyper _typer;
    private List<char> _outputs = new List<char>();
    private bool _writing = false;
    private object locker = new object();

    public ConsoleWriter(TextTyper typer) {
        _typer = typer;
    }

    public void AddToOutput(string text) {
        lock (locker) {
            _outputs.AddRange(text);
        }
        _writing = true;
    }
	
    public void WriteOutputs() {
        if (_writing) {
            if (_outputs.Count == 0) {
                Reset();
                return;
            }            

            lock (locker) {
                List<char> outs = _outputs;
                _typer.PrintLines(new string(outs.ToArray()));
            }
                        
            _outputs = new List<char>();
            _typer.Apply();
        }
    }

    public void Reset() {
        _outputs = new List<char>();
        _writing = false;
    }

}
