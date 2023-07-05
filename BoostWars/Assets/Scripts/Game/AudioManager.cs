using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource SoundPlayer;
    public AudioClip[] aviableSoundClips;
    private Dictionary<string, AudioClip> loadedAudioClips;
    public AudioSource music;


    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Insertar en el diccionario los 4 auds de explosion
        loadedAudioClips = new Dictionary<string, AudioClip>();

        foreach (AudioClip audio in aviableSoundClips)
        {
            loadedAudioClips.Add(audio.name, audio);
        }

    }

    public void playSound(string soundName)
    {
        //reproduce audio sin parar musica
        //Elegir uno de los 4
        if (soundName.StartsWith("explode"))
        {
            soundName = soundName + (((int)(Random.value * 3)) + 1);
        }
        if (soundName.StartsWith("VictoryFF"))
        {
            SoundPlayer.volume = 0.4f;
        }
        if (soundName.StartsWith("PauseSound"))
        {
            SoundPlayer.volume = 0.4f;
        }
        else
        {
            SoundPlayer.volume = 0.6f;
        }

        SoundPlayer.PlayOneShot(loadedAudioClips[soundName]);
    }

    public IEnumerator pauseMusic(string soundName, float time) {
        //metodo que para musica, reproduce audio, resume musica
        music.Pause();
        SoundPlayer.clip = loadedAudioClips[soundName];
        if (soundName.StartsWith("VictoryFF"))
        {
            SoundPlayer.volume = 0.35f;
        }
        else
        {
            SoundPlayer.volume = 1f;
        }
        SoundPlayer.Play();
        while (SoundPlayer.isPlaying)
        {
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(time);
        music.UnPause();
    }

    public void pauseMusicIngame(string soundName)
    {
        music.Pause();
        SoundPlayer.clip = loadedAudioClips[soundName];
        if (soundName.StartsWith("VictoryFF"))
        {
            SoundPlayer.volume = 0.35f;
        }
        else if(soundName.StartsWith("PauseSound"))
        {
            SoundPlayer.volume = 0.4f;
        }
        else
        {
            SoundPlayer.volume = 1f;
        }
        SoundPlayer.Play();
    }

    public void resumeMusic()
    {
        music.UnPause();
    }

}
