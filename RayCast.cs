using UnityEngine;
using UnityEngine.UI;

public class RayCast : MonoBehaviour {

    private const float VIEWDISTANCE = 2.5f;
    public Text text;
    public Text desctiption;
    public GameObject bg;
    public Move move;
    public KeyControll kc;

    void Start () {
        text.text = "";
        desctiption.text = "";
    }

    void Update() {

        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.forward));
        bool defined = Physics.Raycast(ray, out hit);

        if (defined && move.canWalk && hit.distance < VIEWDISTANCE) {
            try {
                GameObject go = hit.transform.gameObject;
                Description desc = go.GetComponent<Description>();
                bg.SetActive(true);
                text.text = desc.title;
                desctiption.text = desc.descript;

                if (desc.title == "Terminal" && Input.GetKeyDown(KeyCode.E)) {
                    kc.Use(go);
                }
                else if (desc.title == "Drone" && Input.GetKeyDown(KeyCode.E)) {
                    go.GetComponent<Drone>().Run();
                }
            }
            catch {
                text.text = "";
                desctiption.text = "";
                bg.SetActive(false);
            }
        }
        else {
            text.text = "";
            desctiption.text = "";
            bg.SetActive(false);
        }

    }
}
