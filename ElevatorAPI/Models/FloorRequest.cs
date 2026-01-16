public enum Direction
{
    Up, // For external requests with the up button selected.
    Down, // For external requests with the down button selected.
    Both // For all internal requests. And all external requests that have both the up and down button selected.
}

public enum Origin
{
    Internal, // Request made from inside the elevator
    External // Request made from outside the elevator
}

public class FloorRequest
{
    public int Floor { get; set; }
    public Direction Direction { get; set; }
    public Origin Origin { get; set; }

    public FloorRequest(int floor, Direction direction, Origin origin)
    {
        Floor = floor; // The destination floor associated with this request. For external requests, this is the caller's current floor (where the hall button was pressed).
        Direction = direction; // The direction of the request (Up or Down)
        Origin = origin; // The origin of the request (Internal or External)
    }
}
