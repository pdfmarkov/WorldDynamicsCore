using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// highlights service walkers by displaying the service icon above them
    /// </summary>
    public class WalkerServicesBar : WalkerValueBar
    {
        [Tooltip("prefab for the icon instance")]
        public SpriteRenderer Prefab;

        private IMainCamera _mainCamera;
        private Service _service;
        private ServiceCategory _serviceCategory;
        private SpriteRenderer _spriteRenderer;

        private void Start()
        {
            _mainCamera = Dependencies.Get<IMainCamera>();

            setBar();
        }

        private void Update()
        {
            setBar();
        }

        public override void Initialize(Walker walker, IWalkerValue value)
        {
            base.Initialize(walker, value);

            _service = value as Service;
            _serviceCategory = value as ServiceCategory;
        }

        private void setBar()
        {
            transform.forward = _mainCamera.Camera.transform.forward;

            if ((_service != null && _service.HasValue(Walker)) || (_serviceCategory != null && _serviceCategory.HasValue(Walker)))
            {
                if (_spriteRenderer == null)
                {
                    _spriteRenderer = Instantiate(Prefab, transform);
                    _spriteRenderer.sprite = ((ServiceWalker)Walker).Service.Icon;
                    _spriteRenderer.transform.localPosition = Vector3.zero;
                }
            }
            else
            {
                if (_spriteRenderer != null)
                {
                    Destroy(_spriteRenderer.gameObject);
                    _spriteRenderer = null;
                }
            }
        }
    }
}