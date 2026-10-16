using UnityEngine;

/// Counts as pressed while any robot (player or ghost) overlaps it.
/// Polls overlaps each physics tick so teleporting or destroying the ghost can't leave it stuck.
[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class PressurePlate : MonoBehaviour
{
    public LayerMask pressedBy;   // set to Robot
    public Color upColor = new Color(0.8f, 0.2f, 0.2f);
    public Color downColor = new Color(0.2f, 0.9f, 0.3f);

    public bool IsPressed { get; private set; }

    BoxCollider2D box;
    SpriteRenderer sprite;
    ContactFilter2D filter;
    readonly Collider2D[] hits = new Collider2D[4];

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        filter = new ContactFilter2D();
        filter.SetLayerMask(pressedBy);
        filter.useTriggers = false;
        sprite.color = upColor;
    }

    void FixedUpdate()
    {
        Bounds b = box.bounds;
        bool pressed = Physics2D.OverlapBox(b.center, b.size, 0f, filter, hits) > 0;
        if (pressed == IsPressed) return;

        IsPressed = pressed;
        sprite.color = pressed ? downColor : upColor;
        // TODO: plate click sfx goes here
    }
}