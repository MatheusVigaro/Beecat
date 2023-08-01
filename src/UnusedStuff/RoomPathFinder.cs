using System.Collections.Generic;

namespace BeeWorld;

public class RoomPathFinder
{
    public static List<AbstractRoom> FindShortestPath(AbstractRoom start, AbstractRoom end)
    {
        var path = new Dictionary<AbstractRoom, AbstractRoom>();
        var queue = new Queue<AbstractRoom>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var room = queue.Dequeue();
            foreach (var neighborIndex in room.connections)
            {
                if (neighborIndex == -1)
                {
                    continue;
                }

                var neighbor = room.world.GetAbstractRoom(neighborIndex);

                if (neighbor == null)
                {
                    continue;
                }

                if (path.ContainsKey(neighbor))
                {
                    continue;
                }

                path[neighbor] = room;

                if (neighbor == end)
                {
                    goto FoundSolution;
                }

                queue.Enqueue(neighbor);
            }
        }

        return null;

        FoundSolution:
        var result = new List<AbstractRoom>();
        var current = end;
        while (!current.Equals(start))
        {
            result.Add(current);
            current = path[current];
        };

        result.Add(start);
        result.Reverse();

        return result;
    }
}