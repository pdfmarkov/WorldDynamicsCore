using UnityEngine;

namespace CityBuilderCore
{
    public class DefaultViewManager : MonoBehaviour, IViewsManager
    {
        [Tooltip(@"view that is active at the start and when no other one is
in the defense demo for example this is the view that displays health bars")]
        public View DefaultView;

        public bool HasActiveView => _activeView;
        public View ActiveView => _activeView;

        private View _activeView;

        protected virtual void Awake()
        {
            Dependencies.Register<IViewsManager>(this);
        }

        private void Start()
        {
            if (DefaultView)
                ActivateView(DefaultView);
        }

        public void ActivateView(View view)
        {
            if (_activeView)
                _activeView.Deactivate();
            _activeView = view;
            if (_activeView)
                _activeView.Activate();
        }
        public void DeactivateView(View view)
        {
            if (_activeView != view)
                return;

            ActivateView(DefaultView);
        }
    }
}