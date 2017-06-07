using UnityEngine;

public class Move : MonoBehaviour {

    public bool active = true;
    public bool canWalk = true;
    public float sensitive = 250f;
	public float speed = 1f;
    public float gravity = 10f;
    public float jumpForce = 10f;
    private float axis = 0f;

    private Vector3 moveDirection = Vector3.zero;

    public GameObject cam;
    private CharacterController cc;
    private Transform rot;

    private SoundSteps sound;
    public AudioSource source;
    public float stepDelay;
    public AudioClip[] stepClip;

    void Start () {
        cc = GetComponent<CharacterController>();
        rot = transform.FindChild("roter");
        sound = new SoundSteps(source);
        sound.SetDelay(stepDelay);
        sound.stepClip = stepClip;

        Cursor.visible = false;
    }

    void Update () {
        if (!active) return;

        if (canWalk) {
            transform.Rotate(0, Input.GetAxis("Mouse X") * sensitive, 0);
            axis += Input.GetAxis("Mouse Y") * sensitive;
            axis = Mathf.Clamp(axis, -90, 90);
            cam.transform.localEulerAngles = new Vector3(-axis, 0, 0);
        }
        else {
            rot.transform.Rotate(0, Input.GetAxis("Mouse X") * sensitive, 0);
            axis += Input.GetAxis("Mouse Y") * sensitive;
            axis = Mathf.Clamp(axis, -90, 90);
            cam.transform.localEulerAngles = new Vector3(-axis, 0, 0);
        }

        if(canWalk) {
            sound.Update();
            if (cc.isGrounded) {
                moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                moveDirection *= speed;
                moveDirection = transform.TransformDirection(moveDirection);

                if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
                    sound.PlayStep();

                if (Input.GetButtonDown("Jump")) {
                    moveDirection.y += jumpForce;
                }
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;
        cc.Move(moveDirection * Time.deltaTime);
    }

    public void Freaze() {
        moveDirection = Vector3.zero;
    }

}
