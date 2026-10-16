using UnityEngine;

/// <summary>
/// One-shot particle bursts built in code, so effects need no prefabs or materials.
/// Particles use unscaled time, so they keep moving during slow motion and the end freeze.
/// </summary>
public static class JuiceFx
{
    static Material particleMaterial;

    public static void Burst(Vector3 position, Color color, int count, float speed, float lifetime,
                             float size = 0.15f, float gravity = 0.8f)
    {
        if (particleMaterial == null)
            particleMaterial = new Material(Shader.Find("Sprites/Default"));

        var go = new GameObject("FX Burst");
        go.transform.position = position;

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // must be stopped before configuring

        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.useUnscaledTime = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.6f, lifetime);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.4f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.5f, size);
        main.startColor = color;
        main.gravityModifier = gravity;
        main.stopAction = ParticleSystemStopAction.Destroy; // cleans itself up

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        // Fade out and shrink over the particle's life
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = fade;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = particleMaterial;

        ps.Play();
    }
}
