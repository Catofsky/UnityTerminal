using System;
using System.IO;

public class EventStreamWriter : StreamWriter {

    #region Event
    public event EventHandler<EventWithArgs<string>> StringWritten;
    #endregion

    #region CTOR
    public EventStreamWriter(Stream s) : base(s) { }
    #endregion

    #region Private Methods
    private void LaunchEvent(string txtWritten) {
        if (StringWritten != null) {
            StringWritten(this, new EventWithArgs<string>(txtWritten));
        }
    }
    #endregion


    #region Overrides

    public override void Write(string value) {
        base.Write(value);
        LaunchEvent(value);
    }

    public override void Write(bool value) {
        base.Write(value);
        LaunchEvent(value.ToString());
    }

    #endregion
}
