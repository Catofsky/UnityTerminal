using System;

public class EventWithArgs<T> : EventArgs {

    public T Value {
        get;
        private set;
    }

    public EventWithArgs(T value) {
        Value = value;
    }

}
