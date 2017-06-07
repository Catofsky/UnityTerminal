using System;
using System.Collections.Generic;

public class ActionInvoker {

    private Queue<Action> _actions;
    private bool _active = false;
    private object locker = new object();

    public ActionInvoker() {
        _actions = new Queue<Action>();
    }

    public void Add(Action act) {
        if (_active)
            lock (locker)
                _actions.Enqueue(act);
    }

    public void Update() {
        lock (locker)
            if (_actions.Count != 0 && _active) {
                while (_actions.Count != 0)
                    _actions.Dequeue().Invoke();
            }
    }

    public void Reset() {
        _active = false;
        _actions = new Queue<Action>();
    }

    public void CanAdd() {
        _active = true;
    }

    public void Pause() {
        _active = false;
    }

}