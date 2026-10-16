using UnityEngine;

/// <summary>
/// Sound effects generated in code (simple synth tones and noise), so the game has
/// audio without any asset files. Swap these for real clips later if you like:
/// just replace the property bodies with your own AudioClip references.
/// </summary>
public static class Sfx
{
    const int SampleRate = 44100;

    static AudioClip hurt, spikes, alert, win, gameOver;

    public static AudioClip Hurt => hurt != null ? hurt : hurt = Make("Hurt", 0.35f, t =>
        0.6f * Sweep(220f, 70f, 0.35f, t) * Mathf.Exp(-t * 10f) +
        0.35f * Noise() * Mathf.Exp(-t * 25f));

    public static AudioClip Spikes => spikes != null ? spikes : spikes = Make("Spikes", 0.2f, t =>
        0.5f * Noise() * Mathf.Exp(-t * 22f) +
        0.4f * Sine(1100f, t) * Mathf.Exp(-t * 35f));

    // Quick two-note "ba-DING" for when an enemy spots you
    public static AudioClip Alert => alert != null ? alert : alert = Make("Alert", 0.55f, t =>
    {
        bool first = t < 0.15f;
        float freq = first ? 700f : 1050f;
        float local = first ? t : t - 0.15f;
        float env = Mathf.Clamp01(local * 300f) * (first ? 0.8f : Mathf.Exp(-local * 6f));
        return 0.5f * (Sine(freq, t) + 0.4f * Sine(freq * 2f, t)) * env;
    });

    // Rising four-note arpeggio
    public static AudioClip Win => win != null ? win : win = Make("Win", 1.2f, t =>
    {
        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
        float v = 0f;
        for (int k = 0; k < notes.Length; k++)
        {
            float local = t - k * 0.14f;
            if (local < 0f) continue;
            float env = Mathf.Clamp01(local * 300f) * Mathf.Exp(-local * 4f);
            v += 0.35f * (Sine(notes[k], local) + 0.4f * Sine(notes[k] * 2f, local)) * env;
        }
        return v;
    });

    // Low falling tone
    public static AudioClip GameOver => gameOver != null ? gameOver : gameOver = Make("GameOver", 1f, t =>
    {
        float tone = Sweep(330f, 90f, 1f, t);
        return (0.3f * tone + 0.2f * Mathf.Sign(tone)) * (1f - t);
    });

    // ---------- Playback ----------

    /// <summary>Plays a clip with no positional effect (UI-style, always full volume).</summary>
    public static void Play2D(AudioClip clip, float volume = 1f)
    {
        Play(clip, Vector3.zero, volume, 0f);
    }

    /// <summary>Plays a clip at a world position; gets quieter with distance.</summary>
    public static void Play3D(AudioClip clip, Vector3 position, float volume = 1f)
    {
        Play(clip, position, volume, 1f);
    }

    static void Play(AudioClip clip, Vector3 position, float volume, float spatialBlend)
    {
        var go = new GameObject("SFX " + clip.name);
        go.transform.position = position;

        var source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = spatialBlend;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 2f;
        source.maxDistance = 25f;
        source.Play();

        Object.Destroy(go, clip.length + 0.1f);
    }

    // ---------- Synth helpers ----------

    static AudioClip Make(string name, float duration, System.Func<float, float> sample)
    {
        int count = Mathf.CeilToInt(duration * SampleRate);
        var data = new float[count];
        for (int i = 0; i < count; i++)
            data[i] = Mathf.Clamp(sample(i / (float)SampleRate), -1f, 1f);

        var clip = AudioClip.Create(name, count, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    static float Sine(float frequency, float t)
    {
        return Mathf.Sin(2f * Mathf.PI * frequency * t);
    }

    static float Noise()
    {
        return Random.value * 2f - 1f;
    }

    // Sine whose pitch glides from f0 to f1 over 'length' seconds
    static float Sweep(float f0, float f1, float length, float t)
    {
        float phase = 2f * Mathf.PI * (f0 * t + (f1 - f0) * t * t / (2f * length));
        return Mathf.Sin(phase);
    }
}
