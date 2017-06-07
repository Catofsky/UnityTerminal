using UnityEngine;

public class KeyControll : MonoBehaviour {

    private bool active = false;
    private bool toUse;
    private Move move;
    public Transform cam;
    private float time;
    private const float DELAY = 1f;
    public Transform refer;
    private Transform spin;
    private TerminalAnimation ta;
    private Terminal _terminal;
    private UIControll uicontroll;

    void Start() {
        move = GetComponent<Move>();
        uicontroll = GetComponent<UIControll>();
        time = DELAY;
    }

    void Update() {
        if (active) {
            if (toUse) {
                cam.position = Vector3.Lerp(spin.position, refer.position, time);
                cam.rotation = Quaternion.Lerp(spin.rotation, refer.rotation, time);
            }
            else {
                cam.position = Vector3.Lerp(refer.position, spin.position, time);
                cam.rotation = Quaternion.Lerp(refer.rotation, spin.rotation, time);
            }
            time -= Time.deltaTime;
            if (time < 0) {
                time = DELAY;
                active = false;
                if (!toUse) move.canWalk = true;
                else _terminal.SetUse(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape) && toUse && !active) {
            Exit();
        }
    }

    public void Use(GameObject go) {
        _terminal = go.transform.FindChild("display").GetComponent<Terminal>();
        if (_terminal.GetPower().On) {
            ta = go.GetComponent<TerminalAnimation>();
            toUse = true;
            spin = go.GetComponent<Spin>().spin.transform;
            active = true;
            move.Freaze();
            move.canWalk = false;
            uicontroll.Hide();
            ta.Open();
        }
        else {
            _terminal = go.transform.FindChild("display").GetComponent<Terminal>();
            _terminal.PowerOn();
        }        
    }

    void Exit() {
        toUse = false;
        active = true;
        _terminal.SetUse(false);
        uicontroll.Show();
        ta.Close();
    }

}
