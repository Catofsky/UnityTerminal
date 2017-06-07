using UnityEngine;

public class SoundSteps {

    private AudioSource source;
    public AudioClip[] stepClip;
    private float stepDelay = 2f;
    private float stepTime;
    private int old = 0;

    public SoundSteps(AudioSource source) {
        this.source = source;
        stepTime = 0;
    }

    public void PlayStep() {
        if (stepTime < 0) {
            System.Random ran = new System.Random();
            int cur = ran.Next(0, stepClip.Length);
            if (cur == old)
                if (stepClip.Length - 1 == cur)
                    cur = 0;
                else
                    cur++;
            source.clip = stepClip[cur];
            old = cur;
            source.Play();
            stepTime = stepDelay;
        }
    }

    public void Update() {
        if (stepTime >= 0)
            stepTime -= Time.deltaTime;
    }

    public void SetDelay(float step) {
        stepDelay = step;
    }

}