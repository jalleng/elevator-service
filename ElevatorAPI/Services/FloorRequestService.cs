using ElevatorAPI.Models;

namespace ElevatorAPI.Services;

public class FloorRequestService
{
  private readonly LinkedList<FloorRequest> _requests = new();
  private LinkedListNode<FloorRequest>? _currentNode;
  private Direction _currentDirection = Direction.Up;
  private readonly Lock _lock = new();

  public IEnumerable<FloorRequest> GetAllRequests()
  {
    lock (_lock)
    {
      return [.. _requests];
    }
  }

  public IEnumerable<FloorRequest> GetInternalRequests()
  {
    lock (_lock)
    {
      return [.. _requests.Where(r => r.Origin == Origin.Internal)];
    }
  }

  public FloorRequest? GetNextStop()
  {
    lock (_lock)
    {
      // Set current node if not already set and list is not empty
      InitializeCurrentNodeIfNeeded();
      if (_currentNode == null) return null;

      var current = _currentNode.Value;
      if (MatchesStopCriteria(current))
      {
        return current;
      }

      // Current floor doesn't match, find next matching floor
      while (true)
      {
        var nextNode = GetNextNodeInDirection(_currentNode);

        if (nextNode == null)
        {
          ReverseDirection();
          if (MatchesStopCriteria(_currentNode.Value))
            return _currentNode.Value;

          nextNode = GetNextNodeInDirection(_currentNode);
          if (nextNode == null) return null;
        }

        _currentNode = nextNode;
        if (MatchesStopCriteria(nextNode.Value))
          return nextNode.Value;
      }
    }
  }

  public void AddRequest(FloorRequest request)
  {
    lock (_lock)
    {
      if (_requests.Count == 0)
      {
        _requests.AddFirst(request);
        return;
      }

      var current = _requests.First;
      while (current != null && current.Value.Floor < request.Floor)
      {
        current = current.Next;
      }

      if (current == null)
      {
        _requests.AddLast(request);
      }
      else if (current.Value.Floor == request.Floor)
      {
        current.Value = MergeRequests(current.Value, request);
      }
      else
      {
        _requests.AddBefore(current, request);
      }
    }
  }

  public bool RemoveRequest(int floor)
  {
    lock (_lock)
    {
      var node = _requests.First;
      while (node != null)
      {
        if (node.Value.Floor == floor)
        {
          if (_currentNode == node) // Handle edge case where current node is being removed.
          {
            _currentNode = node.Next ?? node.Previous;
          }
          _requests.Remove(node);
          return true;
        }
        node = node.Next;
      }
      return false;
    }
  }

  public void ClearAllRequests()
  {
    lock (_lock)
    {
      _requests.Clear();
      _currentNode = null;
      _currentDirection = Direction.Up;
    }
  }

  public void ServiceCurrentFloor()
  {
    lock (_lock)
    {
      if (_currentNode != null)
      {
        var current = _currentNode.Value;

        // Check if we need to keep the request for the opposite direction
        bool shouldKeepForOppositeDirection =
            current.Origin == Origin.External &&
            (current.Direction == Direction.Both ||
             current.Direction != _currentDirection);

        if (shouldKeepForOppositeDirection)
        {
          // Update the request to only serve the remaining direction
          var oppositeDirection = _currentDirection == Direction.Up ? Direction.Down : Direction.Up;
          _currentNode.Value = current with { Direction = oppositeDirection };

          // Move to next node
          var nextNode = GetNextNodeInDirection(_currentNode);

          if (nextNode == null && _requests.Count > 1)
          {
            ReverseDirection();
            nextNode = GetNextNodeInDirection(_currentNode);
          }

          _currentNode = nextNode;
        }
        else
        {
          // Remove the request entirely
          var nodeToRemove = _currentNode;
          _currentNode = GetNextNodeInDirection(_currentNode);

          if (_currentNode == null && _requests.Count > 1)
          {
            ReverseDirection();
            _currentNode = GetNextNodeInDirection(nodeToRemove);
          }

          _requests.Remove(nodeToRemove);
        }
      }
    }
  }

  // Stop for all internal requests and external requests that match the current direction or have been requested for both directions.
  private bool MatchesStopCriteria(FloorRequest request)
  {
    return request.Origin == Origin.Internal ||
            request.Direction == _currentDirection ||
            request.Direction == Direction.Both;

  }

  private LinkedListNode<FloorRequest>? GetNextNodeInDirection(LinkedListNode<FloorRequest> node)
  {
    return _currentDirection == Direction.Up ? node.Next : node.Previous;
  }

  private void ReverseDirection()
  {
    _currentDirection = _currentDirection == Direction.Up ? Direction.Down : Direction.Up;
  }

  private FloorRequest MergeRequests(FloorRequest existing, FloorRequest newRequest)
  {
    // Direction: If origins differ OR directions differ, set to Both
    Direction newDirection;
    if (newRequest.Origin != existing.Origin)
    {
      // Different origins (one internal, one external) - need to service in both directions
      newDirection = Direction.Both;
    }
    else if (existing.Direction != newRequest.Direction)
    {
      // Same origin, different directions
      newDirection = Direction.Both;
    }
    else
    {
      // Same origin and direction
      newDirection = existing.Direction;
    }

    // Origin: Only set to Internal if BOTH are internal
    // This ensures external directional requirements are preserved
    var newOrigin = (newRequest.Origin == Origin.Internal && existing.Origin == Origin.Internal)
        ? Origin.Internal
        : Origin.External;

    return existing with { Origin = newOrigin, Direction = newDirection };
  }

  private void InitializeCurrentNodeIfNeeded()
  {
    if (_currentNode == null && _requests.Count > 0)
    {
      _currentNode = _requests.First;
      _currentDirection = _currentNode!.Value.Direction != Direction.Both
          ? _currentNode.Value.Direction
          : Direction.Up;
    }
  }
}
