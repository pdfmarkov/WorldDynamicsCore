namespace CityBuilderCore
{
    /// <summary>
    /// special layeraffector that only affects as long as its building is working<br/>
    /// the building has to be on the same gameobject or the parent one<br/>
    /// for example stages in THREE only provide entertainment to the area when have workers(<see cref="WorkerUserComponent"/>)
    /// </summary>
    public class LayerAffectorWorking : LayerAffector
    {
        public override bool IsAffecting => _isWorking;

        public IBuilding Building { get; private set; }

        private bool _isWorking;

        protected override void Start()
        {
            base.Start();

            Building = GetComponent<IBuilding>() ?? GetComponentInParent<IBuilding>();
        }

        private void Update()
        {
            if (_isWorking != Building.IsWorking)
            {
                _isWorking = Building.IsWorking;
                checkAffector();
            }
        }
    }
}