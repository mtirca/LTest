using UnityEngine;

namespace Utils
{
    public static class ColorUtil
    {
        public static Color RandomColor()
        {
            return Random.ColorHSV(0, 1, 0, 1, 0, 1, 0.5f, 0.5f);
        }
    }
}