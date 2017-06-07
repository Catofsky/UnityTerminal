using UnityEngine;

public class TerminalAnimation : MonoBehaviour {

    private Animator anim;

	void Start () {
        anim = GetComponent<Animator>();
    }

    public void Open() {
        anim.SetFloat("speed", 0.4f);
        anim.PlayInFixedTime("Open");
    }

    public void Close() {
        anim.SetFloat("speed", -0.4f);
        anim.PlayInFixedTime("Open");
    }

}