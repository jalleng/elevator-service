using ElevatorAPI.Models;

namespace ElevatorAPI.Services;

public class FloorRequestService
{
    private readonly LinkedList<FloorRequest> _requests = new();
    private LinkedListNode<FloorRequest>? _currentNode;
    private Direction _currentDirection = Direction.Up;

    public IEnumerable<FloorRequest> GetAllRequests()
    {
        return _requests;
    }

    public IEnumerable<FloorRequest> GetInternalRequests()
    {
        return _requests.Where(r => r.Origin == Origin.Internal);
    }

    public Direction CurrentDirection => _currentDirection;

    public FloorRequest? GetNextStop()
    {
        if (_currentNode == null && _requests.Count > 0)
        {
            _currentNode = _requests.First;
            _currentDirection = Direction.Up;
            return _currentNode?.Value;
        }

        var nextNode = (_currentDirection == Direction.Up)
                       ? _currentNode?.Next
                       : _currentNode?.Previous;

        // If hit the end, reverse direction and try again
        if (nextNode == null && _requests.Count > 0)
        {
            _currentDirection = (_currentDirection == Direction.Up) ? Direction.Down : Direction.Up;
            nextNode = (_currentDirection == Direction.Up)
                       ? _currentNode?.Next
                       : _currentNode?.Previous;
        }

        if (nextNode == null) return null; // No more floors in either direction

        FloorRequest request = nextNode.Value;

        // Stop if:
        // 1. Requested from inside (must stop)
        // 2. Requested from outside and direction matches
        // 3. Direction is Both
        if (request.Origin == Origin.Internal ||
            request.Direction == _currentDirection ||
            request.Direction == Direction.Both)
        {
            _currentNode = nextNode;
            return request;
        }

        // Skip this floor (wrong direction) - recursively check next
        _currentNode = nextNode;
        return GetNextStop();
    }

    public void AddRequest(FloorRequest request)
    {
        if (_requests.Count == 0)
        {
            _requests.AddFirst(request);
            return;
        }

        // Find correct position to keep sorted by floor number
        var current = _requests.First;
        while (current != null && current.Value.Floor < request.Floor)
        {
            current = current.Next;
        }

        if (current == null)
        {
            // Highest floor requested
            _requests.AddLast(request);
        }
        else if (current.Value.Floor == request.Floor)
        {
            // Floor already exists - merge the requests
            var existing = current.Value;

            // If either is Internal, the merged request must be Internal
            if (request.Origin == Origin.Internal || existing.Origin == Origin.Internal)
            {
                existing.Origin = Origin.Internal;
            }

            // Merge directions
            if (existing.Direction != request.Direction)
            {
                // Different directions requested for same floor = Both
                existing.Direction = Direction.Both;
            }
            // If same direction or already Both, no change needed
        }
        else
        {
            // Insert in middle (floor doesn't exist yet)
            _requests.AddBefore(current, request);
        }
    }

    public bool RemoveRequest(int floor)
    {
        var node = _requests.First;
        while (node != null)
        {
            if (node.Value.Floor == floor)
            {
                if (_currentNode == node)
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

    public void ClearAllRequests()
    {
        _requests.Clear();
        _currentNode = null;
        _currentDirection = Direction.Up;
    }

    public void ServiceCurrentFloor()
    {
        if (_currentNode != null)
        {
            var nodeToRemove = _currentNode;
            _currentNode = _currentNode.Next ?? _currentNode.Previous;
            _requests.Remove(nodeToRemove);
        }
    }
}
