using System;
using Pick.Mode;
using UnityEngine;

namespace Utils
{
    public static class Resource
    {
        private static string GetCrosshairPath(PickMode pickMode)
        {
            return pickMode switch
            {
                PickMode.Pixel => "Crosshairs/crosshair002",
                PickMode.Brush => "Crosshairs/crosshair117",
                _ => throw new Exception("Bad picker value")
            };
        }
        
        public static Sprite LoadCrosshairSprite(PickMode pickMode)
        {
            return Resources.Load<Sprite>(GetCrosshairPath(pickMode));
        }
        
        public static Texture2D LoadCrosshairTexture(PickMode pickMode)
        {
            return Resources.Load<Texture2D>(GetCrosshairPath(pickMode));
        }
    }
}