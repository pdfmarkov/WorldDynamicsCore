using System.Collections.Generic;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// base class for building components implementing <see cref="IBuildingComponent"/>
    /// </summary>
    [RequireComponent(typeof(IBuilding))]
    public abstract class BuildingComponent : MonoBehaviour, IBuildingComponent
    {
        public abstract string Key { get; }

        private IBuilding _building;
        public IBuilding Building { get => _building ?? GetComponent<IBuilding>(); set => _building = value; }

        public virtual void InitializeComponent() { }
        public virtual void SetupComponent() { }
        public virtual void OnReplacing(IBuilding replacement) { }
        public virtual void TerminateComponent() { }

        protected T getOther<T>() where T : class, IBuildingComponent => Building?.GetBuildingComponent<T>();
        protected IEnumerable<T> getOthers<T>() where T : class, IBuildingComponent => Building?.GetBuildingComponents<T>();

        protected BuildingComponentReference<T> registerTrait<T>(T trait) where T : IBuildingTrait<T>
        {
            return Dependencies.Get<IBuildingManager>().RegisterBuildingTrait(trait);
        }
        protected void replaceTrait<T>(T trait, T replacement) where T : IBuildingTrait<T>
        {
            Dependencies.Get<IBuildingManager>().ReplaceBuildingTrait(trait, replacement);
        }
        protected void deregisterTrait<T>(T trait) where T : IBuildingTrait<T>
        {
            Dependencies.Get<IBuildingManager>().DeregisterBuildingTrait(trait);
        }

        public virtual string GetDebugText() => null;
        public virtual string GetDescription() => string.Empty;

        #region Saving
        public virtual string SaveData() => string.Empty;
        public virtual void LoadData(string json) { }
        #endregion
    }
}