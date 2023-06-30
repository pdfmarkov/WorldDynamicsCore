using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// building component that can block walkers that use <see cref="PathType.RoadBlocked"/> from using roads it is built on<br/>
    /// when no tags are defined it will block all walkers, otherwise it will block the defined tags which can be switched on and off(<see cref="RoadBlockerPanel"/>)<br/>
    /// the road blockers in THREE use <see cref="WalkerInfo"/>s as tags which are sent using <see cref="WalkerInfo.PathTagSelf"/>
    /// </summary>
    public class RoadBlockerComponent : BuildingComponent
    {
        public override string Key => "ROB";

        [Tooltip("fill out to specify which road to block or leave empty to block all")]
        public Road Road;
        [Tooltip("fill out when you want to distinguis which walkers to block by tag(for example WalkerInfo)")]
        public KeyedObject[] Tags;

        public bool IsTagged => Tags.Length > 0;
        public List<string> BlockedKeys { get; private set; }

        public override void InitializeComponent()
        {
            base.InitializeComponent();

            if (IsTagged)
            {
                BlockedKeys = Tags.Select(t => t.Key).ToList();

                changeTags(BlockedKeys, true);
            }
            else
            {
                Dependencies.Get<IRoadManager>().Block(Building.GetPoints(), Road);
            }
        }

        public override void TerminateComponent()
        {
            base.TerminateComponent();

            if (IsTagged)
            {
                changeTags(BlockedKeys, false);
            }
            else
            {
                Dependencies.Get<IRoadManager>().Unblock(Building.GetPoints(), Road);
            }
        }

        public void SetBlockedKeys(List<string> blockedKeys)
        {
            changeTags(BlockedKeys, false);
            BlockedKeys = blockedKeys;
            changeTags(BlockedKeys, true);
        }

        private void changeTags(List<string> keys, bool block)
        {
            var blockedTags = Tags.Where(t => keys.Contains(t.Key)).ToList();

            if (block)
                Dependencies.Get<IRoadManager>().BlockTags(Building.GetPoints(), blockedTags, Road);
            else
                Dependencies.Get<IRoadManager>().UnblockTags(Building.GetPoints(), blockedTags, Road);

        }

        #region Saving
        [Serializable]
        public class TaggedBlockerData
        {
            public string[] BlockedKeys;
        }

        public override string SaveData()
        {
            if (!IsTagged)
                return string.Empty;

            return JsonUtility.ToJson(new TaggedBlockerData()
            {
                BlockedKeys = BlockedKeys.ToArray()
            });
        }
        public override void LoadData(string json)
        {
            if (!IsTagged)
                return;

            var data = JsonUtility.FromJson<TaggedBlockerData>(json);

            SetBlockedKeys(data.BlockedKeys.ToList());
        }

        #endregion
    }
}
