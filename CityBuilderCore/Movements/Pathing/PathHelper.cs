﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// helper provides various convenience functions for pathfinding<br/>
    /// chooses the right <see cref="IPathfinder"/> for a given <see cref="PathType"/> and passes the pathing parameters to it
    /// </summary>
    public static class PathHelper
    {
        private static NoPathfinding _noPathfinding = new NoPathfinding();

        /// <summary>
        /// checks if there are points adjacent to the one passed within the specified pathfinding(no diagonals)
        /// </summary>
        /// <param name="point">the point from which to check adjecents</param>
        /// <param name="type">pathing type of the pathfinder that will be ckecked</param>
        /// <param name="tag">additional parameter for the pathfinder</param>
        /// <returns></returns>
        public static bool HasAdjacent(Vector2Int point, PathType type, object tag = null)
        {
            foreach (var adjacent in PositionHelper.GetAdjacent(point, Vector2Int.one))
            {
                if (CheckPoint(adjacent, type, tag))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// checks all the adjacent points of the one passed and returns the ones that exist in the given pathing type<br/>
        /// used for roaming where instead of mapping a path from start to end the walker just moves to a random adjacent point
        /// </summary>
        /// <param name="point">the point from which to check adjecents</param>
        /// <param name="type">pathing type of the pathfinder that will be ckecked</param>
        /// <param name="tag">additional parameter for the pathfinder</param>
        /// <returns>the points around the given point that are viable in the specified pathfinding</returns>
        public static IEnumerable<Vector2Int> GetAdjacent(Vector2Int point, PathType type, object tag = null)
        {
            foreach (var adjacent in PositionHelper.GetAdjacent(point, Vector2Int.one))
            {
                if (CheckPoint(adjacent, type, tag))
                    yield return adjacent;
            }

            var linker = getLinker(type);
            if (linker != null)
            {
                foreach (var link in linker.GetLinks(point, tag))
                {
                    if (link.StartPoint == point)
                        yield return link.EndPoint;
                    else
                        yield return link.StartPoint;
                }
            }
        }

        /// <summary>
        /// checks if a point exists within the pathfinder with the given pathing type<br/>
        /// for example points on map pathing that are blocked or points for road pathing that dont have a road
        /// </summary>
        /// <param name="point">the point from which to check adjecents</param>
        /// <param name="type">pathing type of the pathfinder that will be ckecked</param>
        /// <param name="tag">additional parameter for the pathfinder</param>
        /// <returns>true if the point is viable in the given pathing</returns>
        public static bool CheckPoint(Vector2Int point, PathType type, object tag = null)
        {
            return getPathfinder(type).HasPoint(point, tag);
        }

        public static WalkingPath FindPath(IBuilding start, Vector2Int? current, IBuilding target, PathType type, object tag = null)
        {
            if (current.HasValue)
                return FindPath(current.Value, target, type, tag);
            else
                return FindPath(start, target, type, tag);
        }
        public static WalkingPath FindPath(Vector2Int start, Vector2Int target, PathType type, object tag = null)
        {
            return getPathfinder(type).FindPath(new[] { start }, new[] { target }, tag);
        }
        public static WalkingPath FindPath(Vector2Int start, Vector2Int[] targets, PathType type, object tag = null)
        {
            return getPathfinder(type).FindPath(new[] { start }, targets, tag);
        }
        public static WalkingPath FindPath(Vector2Int start, IBuilding target, PathType type, object tag = null)
        {
            var targetAccess = target.GetAccessPoints(type, tag).ToArray();
            if (targetAccess.Length > 0)
                return getPathfinder(type).FindPath(new[] { start }, targetAccess, tag);
            else
                return null;
        }
        public static WalkingPath FindPath(IBuilding start, IBuilding target, PathType type, object tag = null)
        {
            var startAccess = start.GetAccessPoints(type, tag).ToArray();
            var targetAccess = target.GetAccessPoints(type, tag).ToArray();

            if (startAccess.Length > 0 && targetAccess.Length > 0)
                return getPathfinder(type).FindPath(startAccess, targetAccess, tag);
            else
                return null;
        }
        public static WalkingPath FindPath(IBuilding start, Vector2Int target, PathType type, object tag = null)
        {
            var startAccess = start.GetAccessPoints(type, tag).ToArray();
            if (startAccess.Length > 0)
                return getPathfinder(type).FindPath(startAccess, new[] { target }, tag);
            else
                return null;
        }

        public static IGridLink GetLink(Vector2Int start, Vector2Int end, PathType type, object tag = null)
        {
            return getLinker(type)?.GetLink(start, end, tag);
        }
        public static IEnumerable<IGridLink> GetLinks(Vector2Int start, PathType type, object tag = null)
        {
            return getLinker(type)?.GetLinks(start, tag);
        }

        /// <summary>
        /// finds a random point within a range that exists in the given pathfinding<br/>
        /// used in TOWN when walkers are just idling or looking for a place to chill and replenish energy
        /// </summary>
        /// <param name="center">center of the radius for points</param>
        /// <param name="offset">number of points from the center to ignore</param>
        /// <param name="range">how many points from the center to use</param>
        /// <param name="type">pathing type of the pathfinder that will be ckecked</param>
        /// <param name="tag">additional parameter for the pathfinder</param>
        /// <returns>random point in range but outside offset that exists in the pathing</returns>
        public static Vector2Int FindRandomPoint(Vector2Int center, int offset, int range, PathType type, object tag)
        {
            return PositionHelper.GetAdjacent(center, Vector2Int.one, true, offset, range).Where(p => getPathfinder(type).HasPoint(p, tag)).DefaultIfEmpty(center).Random();
        }

        private static IPathfinder getPathfinder(PathType type)
        {
            switch (type)
            {
                case PathType.Any:
                    return Dependencies.Contains<IPathfinder>() ? Dependencies.Get<IPathfinder>() : Dependencies.Get<IRoadPathfinder>();
                case PathType.Road:
                    return Dependencies.Get<IRoadPathfinder>();
                case PathType.RoadBlocked:
                    return Dependencies.Get<IRoadPathfinderBlocked>();
                case PathType.Map:
                    return Dependencies.Get<IMapPathfinder>();
                case PathType.MapGrid:
                    return Dependencies.Get<IMapGridPathfinder>();
                default:
                    return _noPathfinding;
            }
        }

        private static IGridLinker getLinker(PathType type)
        {
            switch (type)
            {
                case PathType.Road:
                case PathType.RoadBlocked:
                    return Dependencies.GetOptional<IRoadGridLinker>();
                case PathType.Any:
                case PathType.MapGrid:
                    return Dependencies.GetOptional<IMapGridLinker>();
                case PathType.Map:
                case PathType.None:
                default:
                    return null;
            }
        }
    }
}