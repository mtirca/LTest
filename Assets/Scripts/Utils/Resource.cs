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
                PickMode.Curve => "Crosshairs/crosshair001",
                PickMode.Brush => "Crosshairs/crosshair117",
                _ => throw new Exception("Bad picker value")
            };
        }
        
        public static Sprite LoadCrosshair(PickMode pickMode)
        {
            return Resources.Load<Sprite>(GetCrosshairPath(pickMode));
        }
    }
}