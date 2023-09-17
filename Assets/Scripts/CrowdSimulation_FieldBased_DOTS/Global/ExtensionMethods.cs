using UnityEngine;

namespace Global
{
    /// <summary>
    /// Contains general extension and helper methods
    /// </summary>
    public static class ExtensionMethods
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static int CalculateFlatIndex(this int itemIndex, int listIndex, int gridWidth)
        {
            return itemIndex + listIndex * gridWidth;
        }

        public static void GizmosDrawArrow(Vector3 pos, Vector3 direction, Color c, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.color = c;
            Gizmos.DrawRay(pos + Vector3.up, direction + Vector3.up);

            if (direction.sqrMagnitude == 0) return;
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
            Gizmos.DrawRay(pos + Vector3.up + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + Vector3.up + direction, left * arrowHeadLength);
        }
    }
}