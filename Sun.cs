using UnityEngine;

public class Sun : MonoBehaviour {

    public float speed = 1f;
    private Vector3 _vector;

    void Update () {
        transform.Rotate(transform.up , Time.deltaTime * speed);
	}

}
