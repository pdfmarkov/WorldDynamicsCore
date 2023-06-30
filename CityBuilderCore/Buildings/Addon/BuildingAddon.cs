using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// temporary building parts that are added(<see cref="Building.AddAddon{T}(T)"/>) and removed(<see cref="Building.RemoveAddon(BuildingAddon)"/>) at runtime and carry over when a building is replaced<br/>
    /// can be used for effects, statuses, animations, ...
    /// </summary>
    public abstract class BuildingAddon : KeyedBehaviour, ISaveData
    {
        [Tooltip("positioned at the buildings world center instead of its transform origin")]
        public bool Center;
        [Tooltip("scale will be adjusted to the buildings size(for example for particles that should cover buildings of different sizes)")]
        public bool Scale;

        protected bool _isTerminated;

        public IBuilding Building { get; set; }

        /// <summary>
        /// Removes the addon from the Building it is located on<br/>
        /// this will usually result in the termination of the addon
        /// </summary>
        public void Remove()
        {
            Building.RemoveAddon(this);
        }

        /// <summary>
        /// called by the Building after the addon has been instantiated and <see cref="Building"/> has been set
        /// </summary>
        public virtual void InitializeAddon()
        {
            if (Center)
                transform.position = Building.WorldCenter;
            if (Scale)
                transform.localScale = Dependencies.Get<IMap>().IsXY ? new Vector3(Building.Size.x, Building.Size.y, 1) : new Vector3(Building.Size.x, 1, Building.Size.y);
        }
        /// <summary>
        /// called by the Building when the addon gets removed<br/>
        /// this should usually terminate the addon(mark as terminated and Destroy)
        /// </summary>
        public virtual void TerminateAddon()
        {
            _isTerminated = true;

            Destroy(gameObject);
        }
        /// <summary>
        /// called be the addons building when the building gets replaced<br/>
        /// this has to move the addon over to the new building<br/>
        /// otherwise it will be destroyed with the old one
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="replacement"></param>
        public virtual void OnReplacing(Transform parent, IBuilding replacement)
        {
            transform.SetParent(parent);
            Building = replacement;
        }

        public virtual void Awake() { }
        public virtual void Start() { }
        public virtual void Update() { }

        #region Saving
        public virtual string SaveData() => string.Empty;
        public virtual void LoadData(string json) { }
        #endregion
    }
}