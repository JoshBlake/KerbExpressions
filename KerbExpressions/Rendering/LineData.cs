using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbExpressions.Rendering
{
    class LineData
    {
        public Vector3 Start;
        public Vector3 End;
        public Color Color;
        public float Width = 0.002f;
        public bool UseWorldSpace;

        public LineData(Vector3 start, Vector3 end, Color color)
        {
            Start = start;
            End = end;
            Color = color;
        }
    }
}
