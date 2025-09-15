[System.Serializable]
public class ConnectionInfo
{
    public bool up;
    public bool right;
    public bool down;
    public bool left;

    public ConnectionInfo(bool up, bool right, bool down, bool left)
    {
        this.up = up;
        this.right = right;
        this.down = down;
        this.left = left;
    }


    public bool HasConnection(Direction direction)
    {
        return direction switch
        {
            Direction.Up => up,
            Direction.Right => right,
            Direction.Down => down,
            Direction.Left => left,
            _ => false
        };
    }

    public int ConnectionCount => (up ? 1 : 0) + (right ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0);
}

public enum Direction
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3
}