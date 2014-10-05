using UnityEngine;
using System.Collections;

public class Audio : MonoBehaviour {

    public static GameObject audioContainer;

    public static float audioLevel = 1.0f;


    void Awake () {
        audioContainer = gameObject;
    }


    public static AudioSource play(string wav, float volume = 1.0f, bool loop = false) {
        return Audio.play(wav, Vector3.zero, volume, 1.0f, loop);
    }


    public static AudioSource play(string wav, float volume, float pitch, bool loop = false) {
        return Audio.play(wav, Vector3.zero, volume, pitch, loop);
    }


    public static AudioSource play(string wav, Vector3 pos, float volume = 1.0f, float pitch = 1.0f, bool loop = false) {
        AudioClip clip = Resources.Load(wav) as AudioClip;

        if (!clip) {
            print("Error while loading audio clip!");
            return null;
        }

        return Audio.play(clip, pos, volume, pitch, loop);
    }


    public static AudioSource play(AudioClip clip, float volume = 1.0f, float pitch = 1.0f, bool loop = false) {
        return Audio.play(clip, Vector3.zero, volume, pitch, loop);
    }


	public static AudioSource play(AudioClip clip, Vector3 pos, float volume = 1.0f, float pitch = 1.0f, bool loop = false) {
        // create an empty game object at given pos
        GameObject go = new GameObject("Audio: " + clip.name);
        go.transform.parent = audioContainer.transform;
        go.transform.position = pos;

        // Create the audio source
        AudioSource source = go.AddComponent<AudioSource>(); //(AudioSource); // as AudioSource;
        source.loop = loop;

        if(!source) {
        	print("Error while creating audio source component!");
        	return null;
        }

        // set audio source props
        source.clip = clip;
        source.volume = volume * audioLevel;
        source.pitch = pitch;

        // play it
        source.Play();

        // destroy it
        if (!loop) Destroy(go, clip.length);

        // return it in case we need it for something
        return source;
    }


    /*void stop() {
        Source.Stop();
        Destroy(go, clip.length);
    }*/

}
