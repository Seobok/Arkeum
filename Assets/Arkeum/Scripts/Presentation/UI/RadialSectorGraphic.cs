using UnityEngine;
using UnityEngine.UI;

namespace Arkeum.Production.Presentation.UI
{
    public sealed class RadialSectorGraphic : MaskableGraphic
    {
        private const int MinSegmentCount = 2;
        private const int MaxSegmentCount = 96;

        [SerializeField, Range(0f, 360f)] private float centerAngleDegrees = 90f;
        [SerializeField, Range(1f, 360f)] private float sweepAngleDegrees = 45f;
        [SerializeField, Range(0f, 1f)] private float innerRadiusNormalized;
        [SerializeField, Range(0f, 1f)] private float outerRadiusNormalized = 1f;

        public void SetSector(float centerAngle, float sweepAngle, float innerRadius, float outerRadius)
        {
            centerAngleDegrees = Mathf.Repeat(centerAngle, 360f);
            sweepAngleDegrees = Mathf.Clamp(sweepAngle, 1f, 360f);
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

            int segmentCount = Mathf.Clamp(Mathf.CeilToInt(sweepAngleDegrees / 6f), MinSegmentCount, MaxSegmentCount);
            float startAngle = centerAngleDegrees - sweepAngleDegrees * 0.5f;
            float angleStep = sweepAngleDegrees / segmentCount;

            for (int i = 0; i < segmentCount; i++)
            {
                float angle0 = (startAngle + angleStep * i) * Mathf.Deg2Rad;
                float angle1 = (startAngle + angleStep * (i + 1)) * Mathf.Deg2Rad;

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
