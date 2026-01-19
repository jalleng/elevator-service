using ElevatorAPI.Models;

namespace ElevatorAPI.Services;

public class FloorRequestService
{
    private readonly LinkedList<FloorRequest> _requests = new();
    private LinkedListNode<FloorRequest>? _currentNode;
    private Direction _currentDirection = Direction.Up;
    private readonly object _lock = new object(); // Lock object for thread safety

    public IEnumerable<FloorRequest> GetAllRequests()
    {
        lock (_lock)
        {
            return _requests.ToList(); // Return a copy to avoid enumeration issues
        }
    }

    public IEnumerable<FloorRequest> GetInternalRequests()
    {
        lock (_lock)
        {
            return _requests.Where(r => r.Origin == Origin.Internal).ToList();
        }
    }

    public Direction CurrentDirection
    {
        get
        {
            lock (_lock)
            {
                return _currentDirection;
            }
        }
    }

    public FloorRequest? GetNextStop()
    {
        lock (_lock)
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

            if (nextNode == null) return null;

            FloorRequest request = nextNode.Value;

            if (request.Origin == Origin.Internal ||
                request.Direction == _currentDirection ||
                request.Direction == Direction.Both)
            {
                _currentNode = nextNode;
                return request;
            }

            // Skip this floor (wrong direction) - recursively check next
            _currentNode = nextNode;
            return GetNextStopInternal(); // Use internal method to avoid re-locking
        }
    }

    private FloorRequest? GetNextStopInternal()
    {
        // Assumes lock is already held - no lock statement here
        var nextNode = (_currentDirection == Direction.Up)
                       ? _currentNode?.Next
                       : _currentNode?.Previous;

        if (nextNode == null && _requests.Count > 0)
        {
            _currentDirection = (_currentDirection == Direction.Up) ? Direction.Down : Direction.Up;
            nextNode = (_currentDirection == Direction.Up)
                       ? _currentNode?.Next
                       : _currentNode?.Previous;
        }

        if (nextNode == null) return null;

        FloorRequest request = nextNode.Value;

        if (request.Origin == Origin.Internal ||
            request.Direction == _currentDirection ||
            request.Direction == Direction.Both)
        {
            _currentNode = nextNode;
            return request;
        }

        _currentNode = nextNode;
        return GetNextStopInternal();
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
                var existing = current.Value;

                if (request.Origin == Origin.Internal || existing.Origin == Origin.Internal)
                {
                    existing.Origin = Origin.Internal;
                }

                if (existing.Direction != request.Direction)
                {
                    existing.Direction = Direction.Both;
                }
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
                var nodeToRemove = _currentNode;
                _currentNode = _currentNode.Next ?? _currentNode.Previous;
                _requests.Remove(nodeToRemove);
            }
        }
    }
}
