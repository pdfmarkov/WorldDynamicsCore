using System.IO;
using UnityEditor;
using UnityEngine;

namespace CityBuilderCore.Editor
{
    public static class EditorHelper
    {
        public static T DuplicateAsset<T>(T asset) where T : Object
        {
            var path = AssetDatabase.GetAssetPath(asset);
            var extension = Path.GetExtension(path);

            var counter = 0;
            string newPath;

            do
            {
                counter++;
                newPath = path.Replace(extension, string.Empty) + counter + extension;
            }
            while (AssetDatabase.LoadAssetAtPath<Object>(newPath));

            AssetDatabase.CopyAsset(path, newPath);

            var newAsset = AssetDatabase.LoadAssetAtPath<T>(newPath);

            if (newAsset is BuildingInfo building)
            {
                building.Key += counter;
                building.Name += counter;
            }
            else if (newAsset is WalkerInfo walker)
            {
                walker.Name += counter;
            }
            else if (newAsset is Item item)
            {
                item.Name += counter;
            }

            EditorUtility.SetDirty(newAsset);

            return newAsset;
        }

        public static T DuplicateAsset<T>(T asset, string key, string name) where T : Object
        {
            var path = AssetDatabase.GetAssetPath(asset);
            var filename = Path.GetFileName(path);
            var extension = Path.GetExtension(path);

            var counter = -1;
            string newPath;

            do
            {
                counter++;
                newPath = path.Replace(filename, name) + (counter == 0 ? string.Empty : counter.ToString()) + extension;
            }
            while (AssetDatabase.LoadAssetAtPath<Object>(newPath));

            AssetDatabase.CopyAsset(path, newPath);

            var newAsset = AssetDatabase.LoadAssetAtPath<T>(newPath);

            string postfix = counter == 0 ? string.Empty : counter.ToString();

            if (newAsset is BuildingInfo building)
            {
                building.Key = key + postfix;
                building.Name = name.Replace("Info", string.Empty) + postfix;
            }
            else if (newAsset is WalkerInfo walker)
            {
                walker.Key = key + postfix;
                walker.Name = name.Replace("Info", string.Empty) + postfix;
            }
            else if (newAsset is Item item)
            {
                item.Key = key + postfix;
                item.Name = name.Replace("Info", string.Empty) + postfix;
            }

            EditorUtility.SetDirty(newAsset);

            return newAsset;
        }

    }
}
