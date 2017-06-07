using UnityEngine;

public class Timer {

    private float time;
    private float delay;
    public delegate void onTimer();
    private onTimer proc;

    public Timer(float delay, onTimer proc) {
        this.delay = time = delay;
        this.proc = proc;
    }

    public void On() {
        proc();
    }

    public void Update() {
        time -= Time.deltaTime;
        if (time < 0) {
            proc();
            time = delay;
        }
    }

    public void Reset() {
        time = delay;
    }

}