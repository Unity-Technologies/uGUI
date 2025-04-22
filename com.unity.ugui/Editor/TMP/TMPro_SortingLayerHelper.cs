using UnityEngine;


namespace TMPro
{
    // Helpers used by the different sorting layer classes.
    public static class SortingLayerHelper
    {
        // Gets an array of sorting layer names.
        public static string[] sortingLayerNames
        {
            get
            {
                return GetSortingLayerNames();
            }
        }

        static string[] GetSortingLayerNames()
        {
            int layerCount = SortingLayer.layers.Length;

            string[] layerNames = new string[layerCount];

            for (int i = 0; i < layerCount; i++)
            {
                layerNames[i] = SortingLayer.layers[i].name;
            }

            return layerNames;
        }

        internal static int GetSortingLayerIndexFromValue(int value)
        {
            int layerCount = SortingLayer.layers.Length;

            for (int i = 0; i < layerCount; i++)
            {
                if (value == SortingLayer.layers[i].value)
                    return i;
            }

            return -1;
        }

        internal static int GetSortingLayerIndexFromSortingLayerID(int id)
        {
            int layerCount = SortingLayer.layers.Length;

            for (int i = 0; i < layerCount; i++)
            {
                if (id == SortingLayer.layers[i].id)
                    return i;
            }

            return -1;
        }
    }
}
