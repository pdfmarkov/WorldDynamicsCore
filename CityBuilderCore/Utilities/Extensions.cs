using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    public static class Extensions
    {
        public static int GetActiveChildCount(this Transform transform)
        {
            int count = 0;
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeSelf)
                    count++;
            }
            return count;
        }
        public static bool HasActiveChildren(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeSelf)
                    return true;
            }
            return false;
        }

        public static T Random<T>(this IEnumerable<T> collection)
        {
            return collection.ElementAt(UnityEngine.Random.Range(0, collection.Count()));
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }

        public static int GetTotalQuantity(this IEnumerable<ItemQuantity> items)
        {
            return items.Sum(i => i.Quantity);
        }
        public static float GetTotalUnitQuantity(this IEnumerable<ItemQuantity> items)
        {
            return items.Sum(i => i.UnitQuantity);
        }

        public static int GetItemQuantity(this IEnumerable<ItemQuantity> items, Item item)
        {
            var entry = items.FirstOrDefault(i => i.Item == item);
            if (entry == null)
                return 0;
            else
                return entry.Quantity;
        }
        public static int GetItemQuantity(this IEnumerable<ItemQuantity> items, ItemCategory itemCategory)
        {
            return items.Where(i => Array.IndexOf(itemCategory.Items, i) >= 0).Sum(i => i.Quantity);
        }

        public static float GetUnitQuantity(this IEnumerable<ItemQuantity> items, Item item)
        {
            var entry = items.FirstOrDefault(i => i.Item == item);
            if (entry == null)
                return 0;
            else
                return entry.UnitQuantity;
        }

        public static void AddQuantity(this IList<ItemQuantity> items, Item item, int quantity)
        {
            var entry = items.FirstOrDefault(i => i.Item == item);
            if (entry == null)
                items.Add(new ItemQuantity(item, quantity));
            else
                entry.Quantity += quantity;
        }

        /// <summary>
        /// tries to remove the specified quantity of an item from a list
        /// </summary>
        /// <param name="items"></param>
        /// <param name="item">the kind of item to remove</param>
        /// <param name="quantity">the quantity to remove</param>
        /// <returns>the remaining quantity that was not available in the list</returns>
        public static int RemoveQuantity(this IList<ItemQuantity> items, Item item, int quantity)
        {
            var entry = items.FirstOrDefault(i => i.Item == item);
            if (entry == null)
                return quantity;

            if (entry.Quantity <= quantity)
            {
                items.Remove(entry);
                return quantity - entry.Quantity;
            }
            else
            {
                entry.Quantity -= quantity;
                return 0;
            }
        }

        public static bool ContainsQuantity(this IEnumerable<ItemQuantity> storage, Item item, int quantity)
        {
            foreach (var storedItem in storage)
            {
                if (storedItem == null || storedItem.Item != item)
                    continue;

                quantity -= storedItem.Quantity;
                if (quantity <= 0)
                    return true;
            }

            return false;
        }

        public static bool SubtractQuantity(this IEnumerable<ItemStack> stacks, Item item, int quantity)
        {
            if (!stacks.Select(s => s.Items).ContainsQuantity(item, quantity))
                return false;

            foreach (var stack in stacks.OrderBy(s => s.FillDegree))
            {
                quantity = stack.SubtractQuantity(item, quantity);
                if (quantity == 0)
                    break;
            }

            return true;
        }

        public static int SubtractAvailableQuantity(this IEnumerable<ItemStack> stacks, Item item, int quantity)
        {
            int wantedAmount = quantity;
            foreach (var stack in stacks.OrderBy(s => s.FillDegree))
            {
                quantity = stack.SubtractQuantity(item, quantity);
                if (quantity == 0)
                    break;
            }

            return wantedAmount - quantity;
        }

        public static bool FitsQuantity(this IEnumerable<ItemStack> stacks, Item item, int quantity)
        {
            foreach (var stack in stacks)
            {
                if (stack.HasItems && stack.Items.Item != item)
                    continue;//theres a different item in that stack

                int capacity = stack.UnitCapacity * item.UnitSize;

                if (stack.Items != null)
                    capacity -= stack.Items.Quantity;

                quantity -= capacity;

                if (quantity <= 0)
                    return true;
            }

            return false;
        }

        public static int AddQuantity(this IEnumerable<ItemStack> stacks, Item item, int quantity)
        {
            if (!stacks.FitsQuantity(item, quantity))
                return quantity;

            foreach (var stack in stacks)
            {
                if (stack.HasItems && stack.Items.Item != item)
                    continue;//theres a different item in that stack

                quantity = stack.AddQuantity(item, quantity);
                if (quantity == 0)
                    break;
            }

            return quantity;
        }

        public static float GetDistance(this IEnumerable<Vector3> vectors)
        {
            Vector3? last = null;
            var distance = 0f;
            foreach (var vector in vectors)
            {
                if (last.HasValue)
                    distance += Vector3.Distance(last.Value, vector);
                last = vector;
            }
            return distance;
        }

        public static int GetMaxAxisDistance(this Vector2Int position, Vector2Int other)
        {
            return Mathf.Max(Mathf.Abs(other.x - position.x), Mathf.Abs(other.y - position.y));
        }

        public static Coroutine StartChecker(this MonoBehaviour behaviour, Action onCheck, float? interval = null)
        {
            return behaviour.StartCoroutine(check(onCheck, interval ?? Dependencies.Get<IGameSettings>().CheckInterval));
        }
        private static IEnumerator check(Action onCheck, float interval)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0, interval));

            while (true)
            {
                yield return new WaitForSeconds(interval);
                onCheck();
            }
        }
        public static Coroutine StartChecker(this MonoBehaviour behaviour, Func<bool> onCheck, float? interval = null)
        {
            return behaviour.StartCoroutine(check(onCheck, interval ?? Dependencies.Get<IGameSettings>().CheckInterval));
        }
        private static IEnumerator check(Func<bool> onCheck, float interval)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0, interval));

            while (true)
            {
                yield return new WaitForSeconds(interval);
                if (!onCheck())
                    break;
            }
        }

        public static void Delay(this MonoBehaviour behaviour, float seconds, Action action)
        {
            behaviour.StartCoroutine(delay(seconds, action));
        }
        private static IEnumerator delay(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action();
        }

        public static void Delay(this MonoBehaviour behaviour, int frames, Action action)
        {
            behaviour.StartCoroutine(delay(frames, action));
        }
        private static IEnumerator delay(int frames, Action action)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }
            action();
        }

        public static void DelayToEnd(this MonoBehaviour behaviour, Action action)
        {
            behaviour.StartCoroutine(delayToEnd(action));
        }
        private static IEnumerator delayToEnd(Action action)
        {
            yield return new WaitForEndOfFrame();
            action();
        }

        public static int GetQuantity(this IEnumerable<PopulationHousing> housings, Population population, bool includeReserved = false)
        {
            var housing = housings.FirstOrDefault(h => h.Population == population);
            if (housing == null)
                return 0;

            if (includeReserved)
                return housing.Quantity + housing.Reserved;
            else
                return housing.Quantity;
        }
        public static int GetCapacity(this IEnumerable<PopulationHousing> housings, Population population)
        {
            var housing = housings.FirstOrDefault(h => h.Population == population);
            if (housing == null)
            {
                return 0;
            }

            return housing.Capacity;
        }
        public static int GetRemainingCapacity(this IEnumerable<PopulationHousing> housings, Population population)
        {
            var housing = housings.FirstOrDefault(h => h.Population == population);
            if (housing == null)
                return 0;

            return housing.GetRemainingCapacity();
        }

        /// <summary>
        /// reserves up to quantity and returns the remainder
        /// </summary>
        /// <param name="housings"></param>
        /// <param name="population"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public static int Reserve(this IEnumerable<PopulationHousing> housings, Population population, int quantity)
        {
            var housing = housings.FirstOrDefault(h => h.Population == population);
            if (housing == null)
                return quantity;

            int quantityReserved = Mathf.Min(housing.GetRemainingCapacity(), quantity);
            housing.Reserved += quantityReserved;
            return quantity - quantityReserved;
        }
        /// <summary>
        /// inhabits up to quantity and returns the remainder
        /// </summary>
        /// <param name="housings"></param>
        /// <param name="population"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public static int Inhabit(this IEnumerable<PopulationHousing> housings, Population population, int quantity)
        {
            var housing = housings.FirstOrDefault(h => h.Population == population);
            if (housing == null)
                return quantity;

            int quantityInhabited = Mathf.Min(housing.Capacity - housing.Quantity, quantity);

            housing.Reserved = Mathf.Max(0, housing.Reserved - quantityInhabited);
            housing.Quantity += quantityInhabited;

            return quantity - quantityInhabited;
        }
        /// <summary>
        /// abandons up to quantity and returns the remainder
        /// </summary>
        /// <param name="housings"></param>
        /// <param name="population"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public static int Abandon(this IEnumerable<PopulationHousing> housings, Population population, int quantity)
        {
            var housing = housings.FirstOrDefault(h => h.Population == population);
            if (housing == null)
                return 0;

            int quantityAbandoned = Mathf.Min(housing.Quantity, quantity);

            housing.Quantity -= quantityAbandoned;

            return quantity - quantityAbandoned;
        }
        /// <summary>
        /// removes an amount of population relative to the total capacity
        /// </summary>
        /// <param name="housings"></param>
        /// <param name="ratio"></param>
        public static void Kill(this IEnumerable<PopulationHousing> housings, float ratio)
        {
            foreach (var housing in housings)
            {
                housing.Quantity = Mathf.Max(0, housing.Quantity - (int)(housing.Capacity * ratio));
            }
        }

        public static void FollowPath(this MonoBehaviour behaviour, Transform pivot, IEnumerable<Vector3> path, float speed, Action finished)
        {
            behaviour.StartCoroutine(followPath(behaviour.transform, pivot, path, speed, finished));
        }
        private static IEnumerator followPath(Transform transform, Transform pivot, IEnumerable<Vector3> path, float speed, Action finished)
        {
            var positions = path.ToList();
            int index = 0;

            Vector3 last = positions[index];
            Vector3 next = positions[index + 1];

            float distance = Vector3.Distance(last, next);
            float moved = 0f;
            float step = speed * Time.deltaTime;

            transform.position = last;
            Dependencies.Get<IGridRotations>().SetRotation(pivot, next - last);

            yield return null;

            while (true)
            {
                moved += step;

                if (moved > distance)
                {
                    moved -= distance;

                    index++;

                    if (index >= positions.Count - 1)
                    {
                        transform.position = next;
                        finished();
                        yield break;
                    }
                    else
                    {
                        last = positions[index];
                        next = positions[index + 1];

                        distance = Vector3.Distance(last, next);

                        Dependencies.Get<IGridRotations>().SetRotation(pivot, next - last);
                    }
                }

                transform.position = Vector3.Lerp(last, next, moved / distance);

                yield return null;
            }
        }

        public static Vector2Int RotateBuildingPoint(this IBuilding building, Vector2Int point)
        {
            return building.Rotation.RotateBuildingPoint(building.Point, point, building.RawSize);
        }

        public static List<IBuilding> GetRandom(this IBuildingManager buildingManager, int count, Predicate<IBuilding> predicate)
        {
            var buildings = buildingManager.GetBuildings().Where(b => predicate(b)).ToList();

            if (count > 0 && count < buildings.Count)
            {
                var chosenBuildings = new List<IBuilding>();
                for (int i = 0; i < count; i++)
                {
                    var building = buildings.Random();
                    buildings.Remove(building);
                    chosenBuildings.Add(building);
                }
                buildings = chosenBuildings;
            }

            return buildings;
        }
        public static IEnumerable<T> GetBuildingParts<T>(this IBuildingManager buildingManager)
        {
            return buildingManager.GetBuildings().SelectMany(b => b.GetBuildingParts<T>());
        }
        public static IEnumerable<T> GetBuildingComponents<T>(this IBuildingManager buildingManager)
            where T : IBuildingComponent
        {
            return buildingManager.GetBuildings().SelectMany(b => b.GetBuildingComponents<T>());
        }
        public static IEnumerable<T> GetBuildingAddons<T>(this IBuildingManager buildingManager)
           where T : BuildingAddon
        {
            return buildingManager.GetBuildings().SelectMany(b => b.GetBuildingAddons<T>());
        }

        public static Vector2Int Abs(this Vector2Int value) => new Vector2Int(Math.Abs(value.x), Math.Abs(value.y));
        public static Vector3Int Abs(this Vector3Int value) => new Vector3Int(Math.Abs(value.x), Math.Abs(value.y), Math.Abs(value.z));
        public static Vector2 Abs(this Vector2 value) => new Vector2(Mathf.Abs(value.x), Mathf.Abs(value.y));
        public static Vector3 Abs(this Vector3 value) => new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));

        public static byte[] ToBytes<T>(this T[,] array) where T : struct
        {
            var buffer = new byte[array.GetLength(0) * array.GetLength(1) * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))];
            Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
            return buffer;
        }
        public static void FromBytes<T>(this T[,] array, byte[] buffer) where T : struct
        {
            var len = Math.Min(array.GetLength(0) * array.GetLength(1) * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), buffer.Length);
            Buffer.BlockCopy(buffer, 0, array, 0, len);
        }

        public static byte[] ToBytes<T>(this T[,,] array) where T : struct
        {
            var buffer = new byte[array.GetLength(0) * array.GetLength(1) * array.GetLength(2) * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))];
            Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
            return buffer;
        }
        public static void FromBytes<T>(this T[,,] array, byte[] buffer) where T : struct
        {
            var len = Math.Min(array.GetLength(0) * array.GetLength(1) * array.GetLength(2) * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), buffer.Length);
            Buffer.BlockCopy(buffer, 0, array, 0, len);
        }

        /// <summary>
        /// calculates the world position in the center of the cell at the specified point
        /// </summary>
        /// <param name="gridPositions"></param>
        /// <param name="point">a point on the map</param>
        /// <returns>absolute world space position(transform.position) of the cells center</returns>
        public static Vector3 GetWorldCenterPosition(this IGridPositions gridPositions, Vector2Int point)
        {
            return gridPositions.GetCenterFromPosition(gridPositions.GetWorldPosition(point));
        }
        /// <summary>
        /// calculates the world position in the center of an area
        /// </summary>
        /// <param name="gridPositions"></param>
        /// <param name="point">start point of the area on the map</param>
        /// <param name="size">size of the area</param>
        /// <returns>absolute world space position(transform.position) of the cells center</returns>
        public static Vector3 GetWorldCenterPosition(this IGridPositions gridPositions, Vector2Int point, Vector2Int size)
        {
            return (gridPositions.GetCenterFromPosition(gridPositions.GetWorldPosition(point)) + gridPositions.GetCenterFromPosition(gridPositions.GetWorldPosition(point + size))) / 2f;
        }
        /// <summary>
        /// calculates the closest cell center
        /// </summary>
        /// <param name="gridPositions"></param>
        /// <param name="position">absolute world space position(transform.position)</param>
        /// <returns>absolute world space position(transform.position) of the closest cell center</returns>
        public static Vector3 GetWorldCenterPosition(this IGridPositions gridPositions, Vector3 position)
        {
            return gridPositions.GetCenterFromPosition(gridPositions.GetWorldPosition(position));
        }
        /// <summary>
        /// calculates the cell position from any position on the map<br/>
        /// for example in a 1,1 sized map 0.68/5.36 will return 0.0/5.0
        /// </summary>
        /// <param name="gridPositions"></param>
        /// <param name="position">absolute world space position(transform.position)</param>
        /// <returns>absolute world space position(transform.position) in the corner of the cell</returns>
        public static Vector3 GetWorldPosition(this IGridPositions gridPositions, Vector3 position)
        {
            return gridPositions.GetWorldPosition(gridPositions.GetGridPosition(position));
        }

        public static float GetRotation(this IGridRotations gridRotations, Transform transform)
        {
            if (!transform)
                return 0f;
            return gridRotations.GetRotation(transform.right);
        }

        public static void ApplyHeight(this IGridHeights gridHeights, Transform transform, PathType pathType = PathType.Map, float? overrideValue = null)
        {
            gridHeights.ApplyHeight(transform, transform.position, pathType, overrideValue);
        }

        public static void ReceiveAll(this IItemReceiver receiver, ItemStorage storage)
        {
            foreach (var itemQuantity in storage.GetItemQuantities().ToList())
            {
                receiver.Receive(storage, itemQuantity.Item, itemQuantity.Quantity);
            }
        }

        public static Vector2Int GetRandomPoint(this IMap map)
        {
            return new Vector2Int(UnityEngine.Random.Range(0, map.Size.x), UnityEngine.Random.Range(0, map.Size.y));
        }
        public static Vector2Int GetRandomPoint(this IMap map, Vector2Int center, float radius)
        {
            var positions = Dependencies.Get<IGridPositions>();
            var rotations = Dependencies.Get<IGridRotations>();
            var worldCenter = positions.GetWorldCenterPosition(center);
            for (int i = 0; i < 10; i++)
            {
                var angle = UnityEngine.Random.Range(0f, 360f);
                var direction = rotations.GetDirection(angle);

                var position = worldCenter + direction * UnityEngine.Random.Range(0, radius);
                var point = positions.GetGridPosition(position);

                if (map.IsInside(point))
                    return point;
            }
            return center;
        }

        /// <summary>
        /// checks if a point is available for building(not blocked, nothing in the way)
        /// </summary>
        /// <param name="structureManager"></param>
        /// <param name="point">a point on the map</param>
        /// <param name="mask">the structure level to check, 0 for all</param>
        /// <returns>true is the point is available</returns>
        public static bool CheckAvailability(this IStructureManager structureManager, Vector2Int point, int mask)
        {
            var map = Dependencies.Get<IMap>();

            if (!map.IsBuildable(point, mask))
                return false;
            if (!map.IsInside(point))
                return false;
            if (structureManager.HasStructure(point, mask, null, isDecorator: false))
                return false;

            return true;
        }
        /// <summary>
        /// checks if a point is available for building(not blocked, nothing in the way)
        /// </summary>
        /// <param name="map"></param>
        /// <param name="point">a point on the map</param>
        /// <param name="mask">the structure level to check, 0 for all</param>
        /// <returns>true is the point is available</returns>
        public static bool CheckAvailability(this IMap map, Vector2Int point, int mask)
        {
            var structureManager = Dependencies.Get<IStructureManager>();

            if (!map.IsBuildable(point, mask))
                return false;
            if (!map.IsInside(point))
                return false;
            if (structureManager.HasStructure(point, mask, null, isDecorator: false))
                return false;

            return true;
        }

        /// <summary>
        /// checks if a point already has a structure, with some additional options
        /// </summary>
        /// <param name="structureManager"></param>
        /// <param name="point">a point on the map</param>
        /// <param name="mask">structure level to check, o for all</param>
        /// <param name="isWalkable">if you only want to check for structures that are walkable or non walkable</param>
        /// <param name="isUnderlying">whether to check for underlying structures</param>
        /// <param name="isDecorator">if you only want to check for decorators or non decorators</param>
        /// <returns>true if a structure was found on the point</returns></returns>
        public static bool HasStructure(this IStructureManager structureManager, Vector2Int point, int mask = 0, bool? isWalkable = null, bool? isUnderlying = null, bool? isDecorator = null) => structureManager.GetStructures(point, mask, isWalkable, isUnderlying, isDecorator).Any();

        public static IEnumerable<Vector2Int> GetRandomAvailablePoints(this IStructureManager manager, int count, int mask = 0, Predicate<Vector2Int> predicate = null)
        {
            var map = Dependencies.Get<IMap>();

            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var point = map.GetRandomPoint();
                    if (predicate != null && !predicate(point))
                        continue;
                    if (!manager.CheckAvailability(point, mask))
                        continue;
                    yield return point;
                    break;
                }
            }
        }
        public static IEnumerable<Vector2Int> GetRandomAvailablePoints(this IStructureManager manager, Vector2Int center, float radius, int count, int mask = 0, Predicate<Vector2Int> predicate = null)
        {
            var map = Dependencies.Get<IMap>();

            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var point = map.GetRandomPoint(center, radius);
                    if (predicate != null && !predicate(point))
                        continue;
                    if (!manager.CheckAvailability(point, mask))
                        continue;
                    yield return point;
                    break;
                }
            }
        }

        public static void Remove(this IStructure structure, params Vector2Int[] points)
        {
            structure.Remove(points);
        }
        public static void Add(this IStructure structure, params Vector2Int[] points)
        {
            structure.Add(points);
        }

        public static IEnumerable<IHousing> GetHousings(this IPopulationManager _) => Dependencies.Get<IBuildingManager>().GetBuildingTraits<IHousing>();

        /// <summary>
        /// removes any destroyed or null entries
        /// </summary>
        /// <param name="components">the list that null entries will be removed from</param>
        public static void Cleanup(this IList components)
        {
            for (int i = components.Count - 1; i >= 0; i--)
            {
                if (components[i] == null)
                    components.RemoveAt(i);
                else if (components[i] is UnityEngine.Object o && !o)
                    components.RemoveAt(i);
            }
        }

        public static Texture2D Scale(this Texture src, int width, int height)
        {
            RenderTexture rt = RenderTexture.GetTemporary(width, height);
            Graphics.Blit(src, rt);

            RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(rt.width, rt.height);

            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            tex.Apply();

            RenderTexture.ReleaseTemporary(rt);
            RenderTexture.active = currentActiveRT;

            return tex;
        }

        public static string ToDisplayString(this ItemQuantity[] itemQuantities)
        {
            if (itemQuantities == null)
                return string.Empty;
            return string.Join(", ", itemQuantities.Select(q => q.ToString()));
        }
    }
}