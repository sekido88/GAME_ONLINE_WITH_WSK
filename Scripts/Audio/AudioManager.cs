using UnityEngine;
using System.Collections.Generic;
using System;

public class AudioManager : MonoBehaviour {
   private static AudioManager instance;
   public static AudioManager Instance {
       get {
           if (instance == null) {
               GameObject go = new GameObject("AudioManager");
               instance = go.AddComponent<AudioManager>();
               DontDestroyOnLoad(go);
           }
           return instance;
       }
   }

   [Serializable]
   public class Sound {
       public string name;
       public AudioClip clip;
       [Range(0f, 1f)]
       public float volume = 1f;
       [Range(0.1f, 3f)]
       public float pitch = 1f;
       public bool loop = false;
   }

   public Sound[] sounds;
   public AudioSource musicSource;
   public AudioSource sfxSource;
   private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();

   void Awake() {
       if (instance == null) {
           instance = this;
           DontDestroyOnLoad(gameObject);
       } else {
           Destroy(gameObject);
           return;
       }

       foreach (Sound s in sounds) {
           SetupSound(s);
       }
   }

   private void SetupSound(Sound sound) {   
       soundDictionary[sound.name] = sound;
   }

   private void Start() {
       PlayMusic("background");
   }

   public void PlayMusic(string name) {
       if (soundDictionary.TryGetValue(name, out Sound sound)) {
           musicSource.clip = sound.clip;
           musicSource.Play();
       } else {
           Debug.LogWarning($"Sound {name} not found!");
       }
   }

   public void PlaySFX(string name) {
       if (soundDictionary.TryGetValue(name, out Sound sound)) {
           sfxSource.PlayOneShot(sound.clip, sound.volume);
       } else {
           Debug.LogWarning($"Sound {name} not found!");
       }
   }
   
   public void StopSfx() {
        sfxSource.Stop();
   }
   public void StopMusic() {
       musicSource.Stop();
   }

   public void PauseMusic() {
       musicSource.Pause();
   }

   public void ResumeMusic() {
       musicSource.UnPause();
   }

   public void SetMusicVolume(float volume) {
       musicSource.volume = Mathf.Clamp01(volume);
   }

   public void SetSFXVolume(float volume) {
       sfxSource.volume = Mathf.Clamp01(volume);
   }

   public void SetMusicPitch(float pitch) {
       musicSource.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
   }

   public bool IsMusicPlaying() {
       return musicSource.isPlaying;
   }
}