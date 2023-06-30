using System;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// helper class for <see cref="Walker"/> that hold the current status of a running process<br/>
    /// a process is a series of <see cref="WalkerAction"/>s that is usually started from within the walker<br/> 
    /// ProcessState takes care of tracking the currently active action as well as advancing to the next nooe or canceling the process <br/>
    /// <see cref="Walker.StartProcess(WalkerAction[], string)"/> starts a new process, when a walker is loaded use <see cref="Walker.continueProcess"/><br/>
    /// a process can be canceled by <see cref="Walker.CancelProcess"/> which may not end the process immediately but rather when the walker is in an ok state next
    /// </summary>
    public class ProcessState
    {
        public string Key { get; private set; }
        public WalkerAction[] Actions { get; private set; }
        public int CurrentIndex { get; private set; }
        public bool IsCanceled { get; private set; }

        public WalkerAction CurrentAction => Actions.ElementAtOrDefault(CurrentIndex);

        private ProcessState()
        {

        }
        public ProcessState(string key, WalkerAction[] actions)
        {
            Key = key;
            Actions = actions;
        }

        public void Start(Walker walker)
        {
            CurrentAction?.Start(walker);
        }
        public bool Advance(Walker walker)
        {
            if (IsCanceled)
                return false;

            CurrentAction?.End(walker);
            CurrentIndex++;
            if (CurrentIndex >= Actions.Length)
                return false;
            CurrentAction?.Start(walker);
            return true;
        }
        public void Continue(Walker walker)
        {
            CurrentAction?.Continue(walker);
        }
        public void Cancel(Walker walker)
        {
            IsCanceled = true;
            CurrentAction?.Cancel(walker);
        }

        #region Saving
        [Serializable]
        public class ProcessData
        {
            public string Key;
            [SerializeReference]
            public WalkerAction[] Actions;
            public int CurrentAction;
            public bool IsCanceled;
        }

        public ProcessData GetData() => new ProcessData()
        {
            Key = Key,
            Actions = Actions,
            CurrentAction = CurrentIndex,
            IsCanceled = IsCanceled
        };
        public static ProcessState FromData(ProcessData data)
        {
            if (data == null || data.Actions == null || data.Actions.Length == 0)
                return null;
            return new ProcessState()
            {
                Key = data.Key,
                Actions = data.Actions,
                CurrentIndex = data.CurrentAction,
                IsCanceled = data.IsCanceled
            };
        }
        #endregion
    }
}