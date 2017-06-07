using UnityEngine;

public class Terminal : MonoBehaviour {

    private DisplaySystem _display;
    private TerminalConsole _console;
    private KeyboardController _keyboard;
    private TerminalPower _power;
    private bool _use = false;

    public AudioClip startSound;
    public AudioClip loopSound;
    public AudioClip endSound;
    public AudioClip beep;
    public AudioSource source;
    public AudioSource sourceLoop;
    public AudioSource dynamic;

    void Start() {
        if (!Utils.init)
            Utils.InitCharsValues();
        if (!CommandHelper.init)
            CommandHelper.Init();

        _display = new DisplaySystem(GetComponent<MeshRenderer>());
        _console = new TerminalConsole(_display, this);
        _console.SetSound(beep, dynamic);
        _keyboard = new KeyboardController(_console);
        _power = new TerminalPower(_console);
    }

    void Update() {
        if (_power.On) {
            if (_use) {
                _keyboard.Update();
            }
            _console.Update();
        }
        else {
            if (_console.IsStarting)
                _console.StartingUpdate();
        }
    }

    public void SetUse(bool use) {
        _use = use;
    }

    public void PowerOn() {
        if (!_console.IsStarting) {
            _console.OnPower();
            Description desc = transform.parent.GetComponent<Description>();
            desc.descript = "loading";

            source.clip = startSound;
            source.Play();

            sourceLoop.clip = loopSound;
            sourceLoop.Play(80000);
        }
    }

    public void SetPowerOn(bool on) {
        Description desc = transform.parent.GetComponent<Description>();
        if (on) {
            _power.TurnOn();
            desc.descript = "<E> to use";
        }
        else {
            _power.TurnOff();
            desc.descript = "<E> to power on";

            source.clip = endSound;
            source.Play();
            sourceLoop.Stop();
        }
    }

    public TerminalPower GetPower() {
        return _power;
    }

    private void OnApplicationQuit() {
        _console.Close();
    }

}
