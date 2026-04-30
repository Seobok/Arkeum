using System.Collections.Generic;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Actors
{
    [CreateAssetMenu(fileName = "EnemyAttackPattern", menuName = "Arkeum/Enemy Attack Pattern")]
    public sealed class EnemyAttackPatternDefinition : ScriptableObject
    {
        [SerializeField] private Vector2Int editorMin = new Vector2Int(-3, -3);
        [SerializeField] private Vector2Int editorMax = new Vector2Int(3, 3);
        [SerializeField] private bool rotateByFacing = true;
        [SerializeField] private List<Vector2Int> offsets = new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };

        public Vector2Int EditorMin
        {
            get => editorMin;
            set => editorMin = value;
        }

        public Vector2Int EditorMax
        {
            get => editorMax;
            set => editorMax = value;
        }

        public bool RotateByFacing
        {
            get => rotateByFacing;
            set => rotateByFacing = value;
        }

        public List<Vector2Int> Offsets => offsets;

        public bool ContainsTarget(Vector2Int origin, Vector2Int facingDirection, Vector2Int target)
        {
            if (rotateByFacing)
            {
                return TryGetTargetFacing(origin, target, out _);
            }

            return ContainsTargetAtFacing(origin, Vector2Int.right, target);
        }

        public bool TryGetTargetFacing(Vector2Int origin, Vector2Int target, out Vector2Int matchedFacing)
        {
            if (!rotateByFacing)
            {
                matchedFacing = Vector2Int.right;
                return ContainsTargetAtFacing(origin, matchedFacing, target);
            }

            Vector2Int[] clockwiseFacings =
            {
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.up,
            };

            for (int i = 0; i < clockwiseFacings.Length; i++)
            {
                Vector2Int facing = clockwiseFacings[i];
                if (ContainsTargetAtFacing(origin, facing, target))
                {
                    matchedFacing = facing;
                    return true;
                }
            }

            matchedFacing = Vector2Int.right;
            return false;
        }

        public bool ContainsTargetAtFacing(Vector2Int origin, Vector2Int facingDirection, Vector2Int target)
        {
            Vector2Int delta = target - origin;
            for (int i = 0; i < offsets.Count; i++)
            {
                Vector2Int offset = RotateOffset(offsets[i], facingDirection);
                if (offset == delta)
                {
                    return true;
                }
            }

            return false;
        }

        public void ToggleOffset(Vector2Int offset)
        {
            if (offset == Vector2Int.zero)
            {
                return;
            }

            if (offsets.Contains(offset))
            {
                offsets.RemoveAll(existing => existing == offset);
                return;
            }

            offsets.Add(offset);
        }

        public static Vector2Int RotateOffset(Vector2Int offset, Vector2Int facingDirection)
        {
            Vector2Int facing = NormalizeFacing(facingDirection);
            if (facing == Vector2Int.right)
            {
                return offset;
            }

            if (facing == Vector2Int.down)
            {
                return new Vector2Int(offset.y, -offset.x);
            }

            if (facing == Vector2Int.left)
            {
                return new Vector2Int(-offset.x, -offset.y);
            }

            return new Vector2Int(-offset.y, offset.x);
        }

        public static Vector2Int NormalizeFacing(Vector2Int direction)
        {
            if (direction == Vector2Int.zero)
            {
                return Vector2Int.up;
            }

            if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
            {
                return direction.x >= 0 ? Vector2Int.right : Vector2Int.left;
            }

            return direction.y >= 0 ? Vector2Int.up : Vector2Int.down;
        }
    }
}
