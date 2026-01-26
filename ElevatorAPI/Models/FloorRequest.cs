namespace ElevatorAPI.Models;

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

public record FloorRequest(int Floor, Direction Direction, Origin Origin);
