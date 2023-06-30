using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// collection of points or positions that can be followed by a walker
    /// </summary>
    public class WalkingPath
    {
        public int Length => _isPointPath ? _points.Length : _positions.Length;

        public Vector2Int StartPoint => _isPointPath ? _points.FirstOrDefault() : _grid.GetGridPosition(StartPosition);
        public Vector3 StartPosition => _isPointPath ? _grid.GetWorldPosition(StartPoint) : _positions.FirstOrDefault();

        public Vector2Int EndPoint => _isPointPath ? _points.LastOrDefault() : _grid.GetGridPosition(EndPosition);
        public Vector3 EndPosition => _isPointPath ? _grid.GetWorldPosition(EndPoint) : _positions.LastOrDefault();

        private bool _isPointPath;
        private bool _isCanceled;
        private Vector2Int[] _points;
        private Vector3[] _positions;

        private IGridPositions _grid;

        public WalkingPath(Vector2Int[] points)
        {
            _grid = Dependencies.Get<IGridPositions>();

            _isPointPath = true;
            _points = points;
        }
        public WalkingPath(Vector3[] positions)
        {
            _grid = Dependencies.Get<IGridPositions>();

            _isPointPath = false;
            _positions = positions;
        }

        public IEnumerable<Vector2Int> GetPoints()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return GetPoint(i);
            }
        }
        public Vector2Int GetPoint(int index)
        {
            if (_isPointPath)
                return _points.ElementAtOrDefault(index);
            else
                return _grid.GetGridPosition(GetPosition(index));
        }

        public IEnumerable<Vector3> GetPositions()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return GetPosition(i);
            }
        }
        public Vector3 GetPosition(int index)
        {
            if (_isPointPath)
                return _grid.GetWorldPosition(GetPoint(index));
            else
                return _positions.ElementAtOrDefault(index);
        }

        public Vector3 GetPosition(int index, float time, float timePerStep) => Vector3.Lerp(GetPreviousPosition(index), GetNextPosition(index), time / timePerStep);

        public bool HasEnded(int index) => _isCanceled || index >= Length - 1;
        public Vector2Int GetPreviousPoint(int index)
        {
            if (_isPointPath)
                return _points.ElementAtOrDefault(index);
            else
                return _grid.GetGridPosition(GetPreviousPosition(index));
        }
        public Vector2Int GetNextPoint(int index)
        {
            if (_isPointPath)
                return index + 1 < _points.Length ? _points.ElementAtOrDefault(index + 1) : _points.Last();
            else
                return _grid.GetGridPosition(GetNextPosition(index));
        }
        public Vector3 GetPreviousPosition(int index)
        {
            if (_isPointPath)
                return _grid.GetWorldPosition(GetPreviousPoint(index));
            else
                return _positions.ElementAtOrDefault(index);
        }
        public Vector3 GetNextPosition(int index)
        {
            if (_isPointPath)
                return _grid.GetWorldPosition(GetNextPoint(index));
            else
                return index + 1 < _positions.Length ? _positions.ElementAtOrDefault(index + 1) : _positions.Last();
        }
        public float GetDistance(int index) => Vector3.Distance(GetPreviousPosition(index), GetNextPosition(index));
        public Vector3 GetDirection(int index)
        {
            return GetNextPosition(index) - GetPreviousPosition(index);
        }

        public float GetDistance()
        {
            float distance = 0f;
            for (int i = 0; i < Length; i++)
            {
                distance += GetDistance(i);
            }
            return distance;
        }

        public WalkingPath GetReversed()
        {
            if (_isPointPath)
                return new WalkingPath(_points.Reverse().ToArray());
            else
                return new WalkingPath(_positions.Reverse().ToArray());
        }

        public IEnumerator Walk(Walker walker, float delay, Action finished, Action<Vector2Int> moved = null)
        {
            walker.transform.position = StartPosition;

            if (Length > 1)
            {
                walker.onDirectionChanged(GetDirection(0));
            }

            walker.CurrentWalking = new WalkingState(this);

            if (delay > 0f)
            {
                walker.CurrentWaiting = new WaitingState(walker.Info.Delay);
            }

            yield return ContinueWalk(walker, finished, moved);
        }
        public IEnumerator ContinueWalk(Walker walker, Action finished, Action<Vector2Int> moved = null)
        {
            var heights = Dependencies.GetOptional<IGridHeights>();

            heights?.ApplyHeight(walker.Pivot, walker.PathType, walker.HeightOverride);

            if (walker.CurrentWaiting != null)
            {
                yield return walker.CurrentWaiting.Wait();
                walker.CurrentWaiting = null;
            }

            walker.IsWalking = true;

            var rotations = Dependencies.GetOptional<IGridRotations>();
            var w = walker.CurrentWalking;

            var last = GetPreviousPosition(w.Index);
            var next = GetNextPosition(w.Index);

            float distance;

            var link = PathHelper.GetLink(GetPreviousPoint(w.Index), GetNextPoint(w.Index), walker.PathType, walker.PathTag);
            if (link == null)
                distance = GetDistance(w.Index);
            else
                distance = link.Distance;

            while (true)
            {
                w.Moved += Time.deltaTime * walker.Speed;

                if (w.Moved >= distance)
                {
                    moved?.Invoke(GetNextPoint(w.Index));

                    w.Moved -= distance;
                    w.Index++;

                    if (HasEnded(w.Index))
                    {
                        if (!_isCanceled)
                            walker.transform.position = EndPosition;
                        walker.CurrentWalking = null;
                        walker.IsWalking = false;
                        finished();
                        yield break;
                    }
                    else
                    {
                        last = GetPreviousPosition(w.Index);
                        next = GetNextPosition(w.Index);

                        link = PathHelper.GetLink(GetPreviousPoint(w.Index), GetNextPoint(w.Index), walker.PathType, walker.PathTag);
                        if (link == null)
                            distance = GetDistance(w.Index);
                        else
                            distance = link.Distance;

                        walker.onDirectionChanged(GetDirection(w.Index));
                    }
                }

                if (link == null)
                {
                    var position = Vector3.Lerp(last, next, w.Moved / distance);

                    walker.transform.position = position;
                    heights?.ApplyHeight(walker.Pivot, walker.PathType, walker.HeightOverride);
                }
                else
                {
                    link.Walk(walker, w.Moved, GetPreviousPoint(w.Index));
                }

                yield return null;
            }
        }

        public IEnumerator WalkAgent(Walker walker, float delay, Action finished, Action<Vector2Int> moved = null)
        {
            var destination = _grid.GetCenterFromPosition(EndPosition);
            destination += Dependencies.Get<IMap>().GetVariance();

            walker.CurrentAgentWalking = new WalkingAgentState(this, destination);

            if (delay > 0f)
            {
                walker.CurrentWaiting = new WaitingState(walker.Info.Delay);
            }

            yield return ContinueWalkAgent(walker, finished, moved);
        }
        public IEnumerator ContinueWalkAgent(Walker walker, Action finished, Action<Vector2Int> moved = null)
        {
            if (walker.CurrentWaiting != null)
            {
                yield return walker.CurrentWaiting.Wait();
                walker.CurrentWaiting = null;
            }

            var saver = Dependencies.GetOptional<IGameSaver>();
            if (saver != null && saver.IsLoading)
                yield return null;//nav mesh may be invalid

            walker.IsWalking = true;
            walker.Agent.enabled = true;
            walker.Agent.SetDestination(walker.CurrentAgentWalking.Destination);

            if (walker.Agent.velocity != Vector3.zero)
                walker.Agent.velocity = walker.CurrentAgentWalking.Velocity;

            yield return null;

            while (true)
            {
                walker.CurrentAgentWalking.Velocity = walker.Agent.velocity;

                if (_isCanceled || (!walker.Agent.pathPending && !walker.Agent.hasPath))
                {
                    walker.Agent.enabled = false;

                    var position = walker.Pivot.position;
                    var point = walker.GridPoint;

                    walker.transform.position = _grid.GetWorldPosition(point);
                    walker.Pivot.position = position;

                    walker.CurrentAgentWalking = null;
                    walker.IsWalking = false;

                    moved?.Invoke(point);

                    finished();
                    yield break;
                }

                yield return null;
            }
        }

        public void Cancel()
        {
            _isCanceled = true;
        }

        public static IEnumerator TryWalk(Walker walker, float delay, Vector2Int waitPosition, Func<WalkingPath> pathGetter, Action planned, Action finished, Action canceled = null, Action<Vector2Int> moved = null)
        {
            if (walker.CurrentWaiting == null)
                walker.CurrentWaiting = new WaitingState(walker.Info.MaxWait);

            do
            {
                var path = pathGetter();
                if (path != null)
                {
                    var remainingDelay = delay - walker.CurrentWaiting.WaitTime;
                    walker.CurrentWaiting = null;
                    planned?.Invoke();
                    yield return path.Walk(walker, remainingDelay, finished, moved);
                    yield break;
                }
                else
                {
                    walker.transform.position = Dependencies.Get<IGridPositions>().GetWorldPosition(waitPosition);
                }

                yield return new WaitForSeconds(1f);
                walker.CurrentWaiting.WaitTime++;
            }
            while (!walker.CurrentWaiting.IsFinished);

            walker.CurrentWaiting = null;
            if (canceled == null)
                finished();
            else
                canceled();
        }
        public static IEnumerator TryWalk(Walker walker, float delay, Vector2Int position, IBuilding structure, PathType pathType, object pathTag, Action planned, Action finished, Action canceled = null, Action<Vector2Int> moved = null)
        {
            yield return TryWalk(walker, delay, position, () => PathHelper.FindPath(position, structure, pathType, pathTag), planned, finished, canceled, moved);
        }

        public static IEnumerator Roam(Walker walker, float delay, Vector2Int start, int maxMemory, int maxSteps, PathType pathType, object pathTag, Action finished, Action<Vector2Int> moved = null)
        {
            walker.CurrentRoaming = new RoamingState();

            if (delay > 0f)
            {
                walker.CurrentWaiting = new WaitingState(walker.Info.Delay);
            }

            var roaming = walker.CurrentRoaming;

            roaming.Steps = 0;
            roaming.Moved = 0f;

            memorize(start, roaming.Memory, maxMemory);

            roaming.Current = start;
            roaming.Next = roam(roaming.Current, roaming.Memory, pathType, pathTag);

            walker.transform.position = Dependencies.Get<IGridPositions>().GetWorldPosition(roaming.Current);

            yield return null;

            yield return ContinueRoam(walker, maxMemory, maxSteps, pathType, pathTag, finished, moved);
        }
        public static IEnumerator ContinueRoam(Walker walker, int maxMemory, int maxSteps, PathType pathType, object pathTag, Action finished, Action<Vector2Int> moved = null)
        {
            var heights = Dependencies.GetOptional<IGridHeights>();

            heights?.ApplyHeight(walker.Pivot, walker.PathType, walker.HeightOverride);

            if (walker.CurrentWaiting != null)
            {
                yield return walker.CurrentWaiting.Wait();
                walker.CurrentWaiting = null;
            }

            walker.IsWalking = true;

            var positions = Dependencies.Get<IGridPositions>();
            var rotations = Dependencies.GetOptional<IGridRotations>();
            var roaming = walker.CurrentRoaming;

            var from = positions.GetWorldPosition(roaming.Current);
            var to = positions.GetWorldPosition(roaming.Next);
            
            float distance;

            var link = PathHelper.GetLink(roaming.Current, roaming.Next, walker.PathType, walker.PathTag);
            if (link == null)
                distance = Vector3.Distance(from, to);
            else
                distance = link.Distance;

            while (roaming.Steps < maxSteps)
            {
                roaming.Moved += Time.deltaTime * walker.Speed;

                if (roaming.Moved >= distance)
                {
                    moved?.Invoke(roaming.Next);

                    roaming.Moved -= distance;
                    roaming.Current = roaming.Next;

                    roaming.Steps++;

                    if (roaming.Steps >= maxSteps)
                    {
                        break;
                    }
                    else
                    {
                        memorize(roaming.Current, roaming.Memory, maxMemory);
                        roaming.Next = roam(roaming.Current, roaming.Memory, pathType, pathTag);
                    }

                    from = positions.GetWorldPosition(roaming.Current);
                    to = positions.GetWorldPosition(roaming.Next);

                    link = PathHelper.GetLink(roaming.Current, roaming.Next, walker.PathType, walker.PathTag);
                    if (link == null)
                        distance = Vector3.Distance(from, to);
                    else
                        distance = link.Distance;
                }


                if (link == null)
                {
                    var position = Vector3.Lerp(from, to, roaming.Moved / distance);

                    walker.transform.position = position;
                    heights?.ApplyHeight(walker.Pivot, walker.PathType, walker.HeightOverride);

                    walker.onDirectionChanged(to - from);
                }
                else
                {
                    link.Walk(walker, roaming.Moved, roaming.Current);
                }

                yield return null;
            }

            walker.transform.position = positions.GetWorldPosition(roaming.Current);
            yield return null;

            walker.CurrentRoaming = null;
            walker.IsWalking = false;
            finished();
        }
        private static void memorize(Vector2Int point, List<Vector2Int> memory, int maxMemory)
        {
            memory.Remove(point);
            memory.Add(point);

            while (memory.Count > maxMemory)
                memory.RemoveAt(0);
        }

        private static Vector2Int roam(Vector2Int current, List<Vector2Int> memory, PathType pathType, object pathTag = null)
        {
            var options = PathHelper.GetAdjacent(current, pathType, pathTag).ToList();

            if (options.Count == 0)
            {
                return current;
            }

            var firstTime = options.Where(o => !memory.Contains(o)).ToList();

            if (firstTime.Count == 0)
            {
                return options.OrderBy(o => memory.IndexOf(o)).First();
            }
            else if (firstTime.Count == 1)
            {
                return firstTime[0];
            }
            else
            {
                return firstTime[UnityEngine.Random.Range(0, firstTime.Count)];
            }
        }

        #region Saving
        [Serializable]
        public class WalkingPathData
        {
            public bool IsPointPath;
            public bool IsCanceled;
            public List<Vector2Int> Points;
            public List<Vector3> Positions;

            public WalkingPath GetPath() => WalkingPath.FromData(this);
        }

        public WalkingPathData GetData()
        {
            return new WalkingPathData()
            {
                IsPointPath = _isPointPath,
                IsCanceled = _isCanceled,
                Points = _isPointPath ? _points.ToList() : null,
                Positions = _isPointPath ? null : _positions?.ToList()
            };
        }
        public static WalkingPath FromData(WalkingPathData data)
        {
            if (data == null)
                return null;

            if (data.IsPointPath)
                return new WalkingPath(data.Points.ToArray()) { _isCanceled = data.IsCanceled };
            else
                return new WalkingPath(data.Positions.ToArray()) { _isCanceled = data.IsCanceled };
        }
        #endregion
    }
}