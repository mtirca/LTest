using UnityEngine;

namespace Utils
{
    public static class MSUtils
    {
        /// <summary>
        /// Scans the artifact's wavelength array and returns the index that is closest to the target physical wavelength.
        /// </summary>
        public static int GetClosestWavelengthIndex(int[] wavelengths, int targetWavelength)
        {
            int bestIndex = 0;
            int minDifference = int.MaxValue;

            for (int i = 0; i < wavelengths.Length; i++)
            {
                int difference = Mathf.Abs(wavelengths[i] - targetWavelength);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }
    }
}