using UnityEngine;

public class UIControll : MonoBehaviour {

    public GameObject UI;
    public bool on = true;

    public void Show() {
        UI.SetActive(true);
    }

    public void Hide() {
        UI.SetActive(false);
    }

}
