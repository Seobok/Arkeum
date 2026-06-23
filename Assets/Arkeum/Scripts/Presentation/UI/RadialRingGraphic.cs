using UnityEngine;
using UnityEngine.UI;

namespace Arkeum.Production.Presentation.UI
{
    public sealed class RadialRingGraphic : MaskableGraphic
    {
        private const int SegmentCount = 96;

        [SerializeField, Range(0f, 1f)] private float innerRadiusNormalized;
        [SerializeField, Range(0f, 1f)] private float outerRadiusNormalized = 1f;

        public void SetRadii(float innerRadius, float outerRadius)
        {
            innerRadiusNormalized = Mathf.Clamp01(Mathf.Min(innerRadius, outerRadius));
            outerRadiusNormalized = Mathf.Clamp01(Mathf.Max(innerRadius, outerRadius));
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Rect rect = rectTransform.rect;
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            float innerRadius = radius * innerRadiusNormalized;
            float outerRadius = radius * outerRadiusNormalized;
            if (outerRadius <= 0f || outerRadius <= innerRadius)
            {
                return;
            }

            for (int i = 0; i < SegmentCount; i++)
            {
                float angle0 = Mathf.PI * 2f * i / SegmentCount;
                float angle1 = Mathf.PI * 2f * (i + 1) / SegmentCount;

                Vector2 outer0 = new Vector2(Mathf.Cos(angle0), Mathf.Sin(angle0)) * outerRadius;
                Vector2 outer1 = new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * outerRadius;
                Vector2 inner0 = new Vector2(Mathf.Cos(angle0), Mathf.Sin(angle0)) * innerRadius;
                Vector2 inner1 = new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * innerRadius;

                int index = vh.currentVertCount;
                vh.AddVert(inner0, color, Vector2.zero);
                vh.AddVert(outer0, color, Vector2.zero);
                vh.AddVert(outer1, color, Vector2.zero);
                vh.AddVert(inner1, color, Vector2.zero);
                vh.AddTriangle(index, index + 1, index + 2);
                vh.AddTriangle(index, index + 2, index + 3);
            }
        }
    }
}
