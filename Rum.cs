using UnityEngine;

public class Rum : MonoBehaviour {

    public Font font;
    private bool turn = false;
    float deltaTime = 0.0f;

    void Start () {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 40;
    }

    void Update() {
        if (turn)
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

        if (Input.GetKeyDown(KeyCode.F1)) {
            turn = !turn;
        }
    }

    void OnGUI() {
        if (!turn) return;
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(2, 2, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.font = font;
        style.fontSize = 12;
        style.normal.textColor = new Color(1f, 1f, 1f, 1.0f);
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }
}
