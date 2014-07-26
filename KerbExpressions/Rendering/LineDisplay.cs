using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbExpressions.Rendering
{
    class LineDisplay : IDisposable
    {
        private LineRenderer _line;
        private LineRenderer _arrowHead;
        private Transform _parentTransform;

        public LineDisplay(Material material, Transform parentTransform)
        {
            _parentTransform = parentTransform;
            _line = CreateLineRenderer(material, parentTransform);
            _arrowHead = CreateLineRenderer(material, parentTransform);
        }

        public void Update(LineData data)
        {
            //var start = _parentTransform.position + data.Start;
            //var end = _parentTransform.position + data.End;
            var start = data.Start;
            var end = data.End;

            var vec = end - start;
            var arrowLength = Mathf.Clamp(vec.magnitude * 0.5f, 0, data.Width * 4);
            vec.Normalize();
            Vector3 midPoint = end - vec * arrowLength;

            _line.SetColors(data.Color, data.Color);
            _line.SetWidth(data.Width, data.Width);

            _arrowHead.SetColors(data.Color, data.Color);
            _arrowHead.SetWidth(data.Width * 3, 0);

            _line.useWorldSpace = data.UseWorldSpace;
            _arrowHead.useWorldSpace = data.UseWorldSpace;

            _line.SetPosition(0, start);
            _line.SetPosition(1, midPoint);

            _arrowHead.SetPosition(0, midPoint);
            _arrowHead.SetPosition(1, end);

            _line.enabled = true;
            _arrowHead.enabled = true;
        }

        public void Dispose()
        {
            DestroyRenderers();
        }

        LineRenderer CreateLineRenderer(Material material, Transform parentTransform)
        {
            var go = new GameObject("LineOverlay item");
            go.layer = parentTransform.gameObject.layer;
            go.transform.parent = parentTransform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            var lineRenderer = go.AddComponent<LineRenderer>();
            lineRenderer.material = material;

            lineRenderer.SetVertexCount(2);

            return lineRenderer;
        }

        private void DestroyRenderers()
        {
            if (_line != null)
            {
                GameObject.Destroy(_line);
                _line = null;
            }

            if (_arrowHead != null)
            {
                GameObject.Destroy(_arrowHead);
                _arrowHead = null;
            }
        }
    }
}
