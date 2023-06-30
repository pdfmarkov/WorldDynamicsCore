using System;
using UnityEngine;

namespace CityBuilderCore
{
    public abstract class ObjectSetBase : ScriptableObject
    {
        public abstract Type GetObjectType();
        public abstract void SetObjects(object[] objects);
    }
}