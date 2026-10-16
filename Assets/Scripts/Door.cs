using UnityEngine;

/// Solid while closed. Opens (collider off, sprite faded) while its plates are held down.
[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class Door : MonoBehaviour
{
    public PressurePlate[] plates;
    [Tooltip("On: every plate must be held. Off: any one plate opens it.")]
    public bool requireAll = false;

    public bool IsOpen { get; private set; }

    BoxCollider2D box;
    SpriteRenderer sprite;

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        SetOpen(false);
    }

    void FixedUpdate()
    {
        bool shouldOpen = requireAll ? AllPressed() : AnyPressed();
        if (shouldOpen != IsOpen) SetOpen(shouldOpen);
    }

    bool AnyPressed()
    {
        foreach (var p in plates) if (p.IsPressed) return true;
        return false;
    }

    bool AllPressed()
    {
        foreach (var p in plates) if (!p.IsPressed) return false;
        return plates.Length > 0;
    }

    void SetOpen(bool open)
    {
        IsOpen = open;
        box.enabled = !open;
        Color c = sprite.color;
        c.a = open ? 0.2f : 1f;
        sprite.color = c;
        // TODO: door open/close sfx goes here
    }
}