using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    AudioSource hitAudioSource;
    AudioClip blockHitSound;
    bool gameOver;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        blockHitSound = Resources.Load<AudioClip>("Audio/Explosive_crash_and");
        hitAudioSource = GetComponent<AudioSource>();
        if (hitAudioSource == null)
        {
            hitAudioSource = gameObject.AddComponent<AudioSource>();
        }
        hitAudioSource.playOnAwake = false;
        hitAudioSource.spatialBlend = 0f;

        if (blockHitSound == null)
        {
            Debug.LogError("Could not load Explosive_crash_and.mp3 from Assets/Resources/Audio.", this);
        }

    }

    
    void Update()
    {
        if (gameOver)
        {
            return;
        }

        Pointer pointer = Pointer.current;
        if (pointer != null && pointer.press.isPressed){
            Vector3 touchPos = Camera.main.ScreenToWorldPoint(pointer.position.ReadValue());
            if(touchPos.x < transform.position.x){
                spriteRenderer.flipX = true;
                rb.AddForce(Vector2.left * speed);
            }else{
                spriteRenderer.flipX = false;
                rb.AddForce(Vector2.right * speed);
            }


        }else {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision){
        if (!gameOver && collision.gameObject.CompareTag("block"))
        {
            gameOver = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndGame();
            }

            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
            if (hitAudioSource != null && blockHitSound != null)
            {
                hitAudioSource.PlayOneShot(blockHitSound);
            }

            Vector2 hitPoint = collision.contactCount > 0
                ? collision.GetContact(0).point
                : (Vector2)transform.position;
            CreateBlockHitEffect(hitPoint);

        }
    }

    private void CreateBlockHitEffect(Vector2 hitPoint)
    {
        GameObject effectObject = new GameObject("Block Hit Effect");
        effectObject.transform.position = hitPoint;

        ParticleSystem particles = effectObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.duration = 0.25f;
        main.loop = false;
        main.startLifetime = 0.65f;
        main.startSpeed = 3.2f;
        main.startSize = 0.14f;
        main.startColor = new Color(1f, 0.82f, 0.18f, 1f);
        main.gravityModifier = 0.15f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = false;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.08f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient fade = new Gradient();
        fade.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1f, 0.9f, 0.25f), 0f),
                new GradientColorKey(new Color(1f, 0.25f, 0.05f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(fade);

        ParticleSystemRenderer particleRenderer = effectObject.GetComponent<ParticleSystemRenderer>();
        if (spriteRenderer != null)
        {
            particleRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            particleRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
        }

        particles.Emit(28);
        particles.Play();
    }

}
