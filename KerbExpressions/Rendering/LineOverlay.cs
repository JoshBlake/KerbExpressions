using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbExpressions.Rendering
{
    class LineOverlay : MonoBehaviour
    {
        const string ShaderName = "Particles/Alpha Blended";

        private readonly Dictionary<LineData, LineDisplay> _lineDataDisplay = new Dictionary<LineData, LineDisplay>();

        Material _shaderMaterial;

        void Awake()
        {
            var shader = Shader.Find(ShaderName);
            _shaderMaterial = new Material(shader);
        }

        void Update()
        {
            foreach (var kvp in _lineDataDisplay)
            {
                kvp.Value.Update(kvp.Key);
            }
        }

        void OnDestroy()
        {
            foreach (var kvp in _lineDataDisplay)
            {
                kvp.Value.Dispose();
            }
            _lineDataDisplay.Clear();
        }

        public void AddLine(LineData data)
        {
            if (!_lineDataDisplay.ContainsKey(data))
            {
                var display = new LineDisplay(_shaderMaterial, transform);
                _lineDataDisplay[data] = display;
            }
        }

        public void RemoveLine(LineData data)
        {
            _lineDataDisplay.Remove(data);
        }
    }
}
