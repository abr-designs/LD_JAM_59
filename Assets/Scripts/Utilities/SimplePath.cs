using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Utilities
{
    public class SimplePath : MonoBehaviour
    {
        //FIXME Does Bezier even make sense for this? It feels that having extra handles is just annoying
        internal enum MOTION
        {
            LINEAR,
            SMOOTH
        }

        [SerializeField, Space(10f)]
        internal bool looping;

        [SerializeField]
        internal MOTION motion;

        [SerializeField]
        internal List<Vector3> pathPoints = new()
        {
            Vector3.zero,
            Vector3.forward,
        };
        
        [SerializeField, Min(3), ShowIf(nameof(SimplePath.motion), MOTION.LINEAR)]
        internal int catmullResolution = 12;
        

        // Arc-length table: maps sample index → cumulative world distance
        private float[] m_arcLengthTable;
        private float m_totalLength;
        private float m_distanceTravelled;

        //UnityFunctions
        //================================================================================================================//

        private void Start()
        {
            Assert.IsNotNull(pathPoints);
            Assert.IsTrue(pathPoints.Count >= 2);
            
            BakeArcLengthTable();

        }

        //Get Path Position
        //================================================================================================================//

        public Vector3 GetPosition(float distance, float speed, out Vector3 tangent, out float t)
        {
            t = m_totalLength / distance;
            return SamplePath(distance, speed, out tangent, out t);
        }
        
        // Arc-length Baking
        //================================================================================================================//

        private void BakeArcLengthTable()
        {
            var totalSamples = motion == MOTION.LINEAR
                ? pathPoints.Count + (looping ? 1 : 0)
                : looping
                    ? pathPoints.Count * catmullResolution + 1
                    : (pathPoints.Count - 1) * catmullResolution + 1;

            m_arcLengthTable = new float[totalSamples];
            m_arcLengthTable[0] = 0f;

            var previous = SamplePathByIndex(0, totalSamples);

            for (var i = 1; i < totalSamples; i++)
            {
                var current = SamplePathByIndex(i, totalSamples);
                m_arcLengthTable[i] = m_arcLengthTable[i - 1] + Vector3.Distance(previous, current);
                previous = current;
            }

            m_totalLength = m_arcLengthTable[totalSamples - 1];
        }

        // Returns a world-space point at sample index i out of totalSamples
        private Vector3 SamplePathByIndex(int i, int totalSamples)
        {
            if (motion == MOTION.LINEAR)
            {
                if (!looping)
                    return transform.TransformPoint(pathPoints[Mathf.Clamp(i, 0, pathPoints.Count - 1)]);

                var idx = i % pathPoints.Count;
                return transform.TransformPoint(pathPoints[idx]);
            }

            // SMOOTH (Catmull-Rom)
            // For the looping closing sample, explicitly return point[0] to guarantee no float precision gap
            if (looping && i == totalSamples - 1)
                return transform.TransformPoint(pathPoints[0]);

            if (!looping && i == totalSamples - 1)
                return transform.TransformPoint(pathPoints[^1]);

            var segCount = looping ? pathPoints.Count : pathPoints.Count - 1;

            var globalT = (float)i / (totalSamples - 1);
            var scaledT = globalT * segCount;
            var seg = Mathf.FloorToInt(scaledT);
            var localT = scaledT - seg;

            // When globalT == 1.0 (closing sample), seg == segCount; wrap it back
            if (looping)
                seg %= pathPoints.Count;
            else
                seg = Mathf.Clamp(seg, 0, pathPoints.Count - 2);

            var p0 = GetCatmullPoint(seg - 1);
            var p1 = GetCatmullPoint(seg);
            var p2 = GetCatmullPoint(seg + 1);
            var p3 = GetCatmullPoint(seg + 2);

            return LerpFunctions.CatmullRom(localT, p0, p1, p2, p3);
        }

        // Returns a world-space point at a given cumulative arc distance
        private Vector3 SamplePath(float distance, float speed, out Vector3 tangent, out float t)
        {
            distance = Mathf.Clamp(distance, 0f, m_totalLength);
            t = m_totalLength / distance;
            tangent = Vector3.zero;
            
            if (m_arcLengthTable == null) 
                return transform.TransformPoint(pathPoints[0]);

            int totalSamples = m_arcLengthTable.Length;

            // Binary search for the two surrounding samples
            int lo = 0, hi = totalSamples - 1;
            while (lo < hi - 1)
            {
                int mid = (lo + hi) / 2;
                if (m_arcLengthTable[mid] < distance) 
                    lo = mid;
                else 
                    hi = mid;
            }

            float segStart = m_arcLengthTable[lo];
            float segEnd = m_arcLengthTable[hi];
            float segLength = segEnd - segStart;

            float localT = segLength > 0f ? (distance - segStart) / segLength : 0f;

            Vector3 a = SamplePathByIndex(lo, totalSamples);
            Vector3 b = SamplePathByIndex(hi, totalSamples);
            
            tangent = ((b - a) * speed).normalized;
            
            return Vector3.Lerp(a, b, localT);
        }

        //Curve Functions
        //================================================================================================================//

        #region Curve Functions

        internal Vector3 GetCatmullPoint(int index)
        {
            if (looping)
            {
                var wrappedIndex = (index % pathPoints.Count + pathPoints.Count) % pathPoints.Count;
                return transform.TransformPoint(pathPoints[wrappedIndex]);
            }

            if (index < 0)
            {
                var first = transform.TransformPoint(pathPoints[0]);
                var second = transform.TransformPoint(pathPoints[1]);
                return first + (first - second);
            }

            if (index >= pathPoints.Count)
            {
                var last = transform.TransformPoint(pathPoints[^1]);
                var beforeLast = transform.TransformPoint(pathPoints[^2]);
                return last + (last - beforeLast);
            }

            return transform.TransformPoint(pathPoints[index]);
        }

        #endregion //Curve Functions

        //Unity Editor Functions
        //================================================================================================================//
#if UNITY_EDITOR
        
        internal void AddPoint()
        {
            const float DEFAULT_DISTANCE = 2f;
            if(pathPoints == null)
                pathPoints = new List<Vector3>();

            Vector3 localPosition;

            switch (pathPoints.Count)
            {
                case >= 2:
                {
                    var previousPointA = pathPoints[^2];
                    var previousPointB = pathPoints[^1];
               
                    var tangent = previousPointB - previousPointA;
                
                    localPosition = previousPointB + tangent.normalized * tangent.magnitude;
                    break;
                }
                case 1:
                    localPosition = pathPoints[^1] + transform.forward.normalized * DEFAULT_DISTANCE;
                    break;
                default:
                    localPosition = Vector3.zero;
                    break;
            }
            
            pathPoints.Add(localPosition);
            
            //If the points changed, we need to make sure we properly update the inspector
            EditorUtility.SetDirty(gameObject);
        }


        private void OnDrawGizmos()
        {
            var arraySize = pathPoints.Count;

            if (arraySize < 2)
                return;

            Handles.color = Color.yellow;

            switch (motion)
            {
                case MOTION.LINEAR:
                    DrawLinearPath();
                    break;
                case MOTION.SMOOTH:
                    DrawCatmullPath();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            for (int i = 0; i < arraySize; i++)
            {
                Handles.color = i == 0 ? Color.blue : Color.white;
                var point = GetWorldPathPoint(i);
                var handleSize = HandleUtility.GetHandleSize(point) * 0.075f;
                Handles.SphereHandleCap(0, point, Quaternion.identity, handleSize, EventType.Repaint);
            }

            return;

            void DrawLinearPath()
            {
                for (int i = 1; i < arraySize; i++)
                {
                    Handles.DrawLine(
                        GetWorldPathPoint(i - 1),
                        GetWorldPathPoint(i));
                }

                if (looping)
                    Handles.DrawLine(GetWorldPathPoint(arraySize - 1), GetWorldPathPoint(0));
            }

            void DrawCatmullPath()
            {
                int segmentCount = looping ? arraySize : arraySize - 1;

                for (int i = 0; i < segmentCount; i++)
                {
                    Vector3 p0 = GetCatmullPoint(i - 1);
                    Vector3 p1 = GetCatmullPoint(i);
                    Vector3 p2 = GetCatmullPoint(i + 1);
                    Vector3 p3 = GetCatmullPoint(i + 2);

                    Vector3 previousPoint = p1;

                    for (int step = 1; step <= catmullResolution; step++)
                    {
                        float t = step / (float)catmullResolution;
                        Vector3 currentPoint = LerpFunctions.CatmullRom(t, p0, p1, p2, p3);
                        Handles.DrawLine(previousPoint, currentPoint);
                        previousPoint = currentPoint;
                    }
                }
            }

            Vector3 GetWorldPathPoint(int index, bool invert = false)
            {
                var point = pathPoints[index];
                return invert
                    ? transform.InverseTransformPoint(point)
                    : transform.TransformPoint(point);
            }
        }
        
#endif
        //================================================================================================================//
    }
}