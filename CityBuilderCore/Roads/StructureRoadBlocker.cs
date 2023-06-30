﻿using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// blocker that uses the points of an <see cref="IStructure"/> on the same or the parent component<br/>
    /// blocking prevents a <see cref="Walker"/> with <see cref="PathType.RoadBlocked"/> from using a point
    /// </summary>
    public class StructureRoadBlocker : MonoBehaviour
    {
        public IStructure Structure { get; private set; }

        private void Start()
        {
            Structure = GetComponent<IStructure>() ?? GetComponentInParent<IStructure>();
            Structure.PointsChanged += structurePointsChanged;

            Dependencies.Get<IRoadManager>().Block(Structure.GetPoints());
        }

        private void structurePointsChanged(PointsChanged<IStructure> change)
        {
            var roadManager = Dependencies.Get<IRoadManager>();

            roadManager.Unblock(change.RemovedPoints);
            roadManager.Block(change.AddedPoints);
        }

        private void OnDestroy()
        {
            if (!gameObject.scene.isLoaded)
                return;
            if (Dependencies.GetOptional<IGameSaver>()?.IsLoading == true)
                return;

            Dependencies.Get<IRoadManager>().Unblock(Structure.GetPoints());
        }
    }
}