# SoundManager Design Document for Unity

## 1. Overview

### 1.1 Purpose  
The SoundManager will manage all audio playback in the game, including sound effects, music, and ambient sounds, utilizing an an audio source pool to have consistent availability and component organisation.

### 1.2 Scope  
The SoundManager will handle one-shot sound effects, looping sounds, background music and audio layering.

---

## 2. Requirements

### 2.1 Functional Requirements  
- Play one-shot sound effects anywhere in the game.  
- Play background music with fade in/out transitions.  
- Support layered music (e.g., add percussion when in combat).  
- Control global volume and separate volumes for music and SFX.  
- Manage AudioSource pool to optimize performance.  
- Support 3D spatial sound for environmental audio.

### 2.2 Non-Functional Requirements  
- Minimal impact on game performance.  
- Easy integration with game events and scripts.  

---

## 3. Architecture and Design

### 3.1 System Structure 

### 3.2 SoundManager Class  
- Singleton pattern to ensure one global instance.  
- Public API for playing sounds (e.g., `PlaySound`, `PlayMusic`).  
- Methods for volume control and mute toggling.

### 3.3 AudioSource Pooling  
- Pre-instantiate a fixed number of AudioSources.  
- Reuse AudioSources to avoid runtime allocations.  
- Dynamically increase AudioSource count based on requests from other scripts up to a certain maximum.
- Handle concurrency (multiple sounds playing at once).

### 3.4 Music Layering System  
- Support multiple AudioSources for music layers.  
- Allow dynamic enabling/disabling of layers based on game state.

---

## 4. User Interface

### 4.1 Inspector Settings  
- Options to assign AudioClips for default sounds.

---

## 5. Implementation Details

### 5.1 AudioClip Management  
- Use ScriptableObjects or centralized audio libraries to organize clips.

### 5.2 Code Snippets (Example)

```csharp
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
    [SerializeField] private AudioSource[] audioSourcePool;
    [SerializeField] private AudioSource musicSource;

    private int poolIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioSourcePool[poolIndex].clip = clip;
        audioSourcePool[poolIndex].Play();
        poolIndex = (poolIndex + 1) % audioSourcePool.Length;
    }

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }
}
````