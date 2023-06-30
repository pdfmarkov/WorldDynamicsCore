using CityBuilderCore;
using System;
using UnityEngine;

namespace CityBuilderUrban
{
    public class TornadoWalker : Walker
    {
        public StructureLevelMask DestructionLevel;
        public GameObject DestructionPrefab;

        public override void Initialize(BuildingReference home, Vector2Int start)
        {
            base.Initialize(home, start);

            var x = UnityEngine.Random.Range(0, 25);

            walk(new WalkingPath(new Vector2Int[] { new Vector2Int(x, 31), new Vector2Int(x, 0) }));

            Dependencies.GetOptional<INotificationManager>()?.Notify(new NotificationRequest($"TORNADO !!!", transform));
        }

        private void Update()
        {
            var point = Dependencies.Get<IGridPositions>().GetGridPosition(transform.position);

            if (Dependencies.Get<IStructureManager>().Remove(new Vector2Int[] { point }, DestructionLevel.Value, false) > 0 && DestructionPrefab)
            {
                Instantiate(DestructionPrefab, Dependencies.Get<IGridPositions>().GetWorldPosition(point), Quaternion.identity);
            }
        }

        public override void LoadData(string json)
        {
            base.LoadData(json);

            continueWalk();
        }
    }

    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class ManualTornadoWalkerSpawner : ManualWalkerSpawner<TornadoWalker> { }
}
