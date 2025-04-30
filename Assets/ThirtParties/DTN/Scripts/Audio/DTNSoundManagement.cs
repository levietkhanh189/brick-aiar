using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DTNSoundManagement : MonoBehaviour
{
    public DTNSound[] sounds;
    public static DTNSoundManagement instance;
    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null){
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);
        foreach (DTNSound item in sounds)
        {
            item.source = gameObject.AddComponent<AudioSource>();
            item.source.clip = item.clip;
            item.source.volume = item.volume;
            item.source.loop = item.loop;
            // item.source.awake = item.playAwake;
        }
    }

    void Start(){
        foreach (DTNSound item in sounds)
        {
            
            if (item.loop == true) item.source.Play();
        }
    }

    // Update is called once per frame
    public void Play (string name){
        DTNSound sound = Array.Find(sounds, sound => sound.name == name);
        if (sound == null) {
            Debug.LogWarning("Sound: " + name + " is not found");
            return;
        }
        sound.source.Play();
    }
}
