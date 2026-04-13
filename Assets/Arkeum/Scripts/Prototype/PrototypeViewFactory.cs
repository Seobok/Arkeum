using UnityEngine;

namespace Arkeum.Prototype
{
    public sealed class PrototypeViewFactory
    {
        private Sprite squareSprite;

        public GameObject CreateCell(Transform parent, Vector2Int cell, Color color, string name, int sortingOrder = 0)
        {
            GameObject tile = new GameObject(name);
            tile.transform.SetParent(parent, false);
            tile.transform.position = new Vector3(cell.x, cell.y, 0f);

            SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSquareSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            tile.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            return tile;
        }

        public GameObject CreateActor(Transform parent, string name, Vector2Int cell, Color color, int sortingOrder)
        {
            GameObject actor = new GameObject(name);
            actor.transform.SetParent(parent, false);
            actor.transform.position = new Vector3(cell.x, cell.y, -0.1f);

            SpriteRenderer renderer = actor.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSquareSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            actor.transform.localScale = new Vector3(0.72f, 0.72f, 1f);

            return actor;
        }

        private Sprite GetSquareSprite()
        {
            if (squareSprite != null)
            {
                return squareSprite;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            squareSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            return squareSprite;
        }
    }
}
