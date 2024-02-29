using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class audio : MonoBehaviour
{
    public AudioClip[] AudioClipsOneShot;

    public AudioSource Source;

    public bool playMusic;
    public float time = .5f;

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        while (true)
        {
            if (playMusic && Source)
            {
                foreach (AudioClip clip in AudioClipsOneShot)
                {
                    Source.PlayOneShot(clip);
                }
            }

            yield return new WaitForSeconds(time);
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}