/// One physics tick of input. This is all the ghost ever replays.
public struct InputFrame
{
    public float move;        // -1 (left), 0, or +1 (right)
    public bool jumpPressed;  // true only on the tick the jump key went down
    public bool jumpHeld;     // true while the jump key is down (variable jump height)
}