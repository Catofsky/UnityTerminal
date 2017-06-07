using UnityEngine;

namespace TerminalSystem {

    public class KeysController {

        private TextTyper typer;

        private Timer stickingStart;
        private bool isSticking = false;
        private float startSticking = 0.5f;

        private Timer sticking;
        private float stickingDelay = 0.1f;

        public KeysController(TextTyper typer) {
            this.typer = typer;
            stickingStart = new Timer(startSticking, () => isSticking = true);
            sticking = new Timer(stickingDelay, () => typer.OnKeyDown('\b'));
        }

        public void Update() {
            if (Input.GetKey(KeyCode.Backspace)) {
                stickingStart.Update();
                if (isSticking) {
                    sticking.Update();
                }
            }
            if (Input.GetKeyUp(KeyCode.Backspace)) {
                isSticking = false;
                sticking.Reset();
                stickingStart.Reset();
            }
        }

    }

}