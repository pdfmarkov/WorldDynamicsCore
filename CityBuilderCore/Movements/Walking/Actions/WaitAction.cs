using System;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// action that simply waits for a set time
    /// </summary>
    [Serializable]
    public class WaitAction : WalkerAction
    {
        [SerializeField]
        private float _time;

        public WaitAction()
        {

        }
        public WaitAction(float time)
        {
            _time = time;
        }

        public override void Start(Walker walker)
        {
            base.Start(walker);

            walker.wait(walker.AdvanceProcess, _time);
        }
        public override void Continue(Walker walker)
        {
            base.Continue(walker);

            walker.continueWait(walker.AdvanceProcess);
        }
        public override void Cancel(Walker walker)
        {
            base.Cancel(walker);

            walker.cancelWait();
        }
    }
}
