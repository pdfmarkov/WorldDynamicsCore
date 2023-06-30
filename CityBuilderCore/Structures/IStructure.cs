using System;
using System.Collections.Generic;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// interface for anything placed on the map(roads, decorators, buildings, ....)
    /// </summary>
    public interface IStructure : IKeyed
    {
        /// <summary>
        /// reference to the structure that keeps working even if the structure is replaced
        /// </summary>
        StructureReference StructureReference { get; set; }

        /// <summary>
        /// whether the structure can be removed by the player
        /// </summary>
        bool IsDestructible { get; }
        /// <summary>
        /// whether the structure is automatically removed when something is built on top of it
        /// </summary>
        bool IsDecorator { get; }
        /// <summary>
        /// whether walkers can pass the points of this structure
        /// </summary>
        bool IsWalkable { get; }

        /// <summary>
        /// the structure level mask of this structure<br/>
        /// determines which levels a structure occupies<br/>
        /// structures that have no levels in common can be placed on top of each other
        /// </summary>
        int Level { get; }

        /// <summary>
        /// has to be fired when a structures points changed so the manager can readjust paths<br/>
        /// only viable for structures that are stored in list form like underlying, collections and tiles<br/>
        /// other structures have to be reregistered
        /// </summary>
        event Action<PointsChanged<IStructure>> PointsChanged;

        /// <summary>
        /// retrieves the name of the structure for display in the UI
        /// </summary>
        /// <returns>name of the structure for UI</returns>
        string GetName();

        /// <summary>
        /// retrieves all the points the structure occupies
        /// </summary>
        /// <returns>the points the structure occupies</returns>
        IEnumerable<Vector2Int> GetPoints();
        /// <summary>
        /// checks if the structure occupies a certain point
        /// </summary>
        /// <param name="point">a point on the map</param>
        /// <returns>true if the structure occupies that point</returns>
        bool HasPoint(Vector2Int point);

        /// <summary>
        /// adds points to a structure, for example a tree to a tree structure collection<br/>
        /// this may not be possible for structures with fixed points like buildings<br/>
        /// typically called in a structures Start method
        /// </summary>
        /// <param name="points">the points that will be added to the structure</param>
        void Add(IEnumerable<Vector2Int> points);
        /// <summary>
        /// removes points from the structure, for some structures like buildings removing any point will remove the whole thing<br/>
        /// typically called in a structures OnDestroy method
        /// </summary>
        /// <param name="points">the points to remove</param>
        void Remove(IEnumerable<Vector2Int> points);
    }
}