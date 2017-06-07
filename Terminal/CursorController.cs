public class CursorController {

    public int x;
    public int y;

    private DisplaySystem screen;
    private Timer flicker;
    private float flickerDelay = 0.5f;
    private bool flickerOn = false;

    private int borderWidth;
    private int borderHeight;

    public CursorController(DisplaySystem screen, int borderWidth, int borderHeight) {
        this.borderWidth = borderWidth;
        this.borderHeight = borderHeight;
        x = borderWidth;
        y = borderHeight;
        flicker = new Timer(flickerDelay, OnFlicker);
        this.screen = screen;
    }

    public void MoveUp(int count) {
        x = borderWidth;
        y = y - count < borderHeight ? borderHeight : y - count;
    }

    public void Update() {
        flicker.Update();
    }

    public void Flick() {
        flicker.On();
    }

    public void SetPosition(int x, int y) {
        screen.ResetInvertBlock(this.x, this.y);
        this.x = x;
        this.y = y;
    }

    public void Next() {
        screen.ResetInvertBlock(x, y);
        if (x >= DisplaySystem.COLS - 1 - borderWidth) {
            NextLine();
        }
        else {
            x++;
        }
    }

    public void NextLine() {
        screen.ResetInvertBlock(x, y);
        x = borderWidth;
        if (y >= DisplaySystem.LINES - 1 - borderHeight) {
            screen.ScrollUp(borderWidth, borderHeight);
        }
        else {
            y++;
        }
    }

    void OnFlicker() {
        if (flickerOn) {
            screen.ResetInvertBlock(x, y);
            screen.Apply();
            flickerOn = false;
        }
        else {
            screen.SetInvertBlock(x, y);
            screen.Apply();
            flickerOn = true;
        }
    }

}
