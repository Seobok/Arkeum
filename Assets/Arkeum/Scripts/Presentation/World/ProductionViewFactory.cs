using UnityEngine;
using UnityEngine.UI;

namespace Arkeum.Production.Presentation.World
{
    public sealed class ProductionViewFactory
    {
        private Sprite squareSprite;

        public GameObject CreateCell(
            Transform parent,
            Vector2Int cell,
            Sprite sprite,
            Color tint,
            string name,
            int sortingOrder = 0)
        {
            GameObject tile = new GameObject(name);
            tile.transform.SetParent(parent, false);
            tile.transform.position = new Vector3(cell.x, cell.y, 0f);

            SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite != null ? sprite : GetSquareSprite();
            renderer.color = tint;
            renderer.sortingOrder = sortingOrder;
            tile.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            return tile;
        }

        public ActorView CreateActor(
            Transform parent,
            string name,
            Vector2Int cell,
            Sprite sprite,
            Color tint,
            int sortingOrder)
        {
            GameObject actor = new GameObject(name);
            actor.transform.SetParent(parent, false);
            actor.transform.position = new Vector3(cell.x, cell.y, -0.1f);

            SpriteRenderer renderer = actor.AddComponent<SpriteRenderer>();
            Sprite fallbackSprite = GetSquareSprite();
            renderer.sprite = sprite != null ? sprite : fallbackSprite;
            renderer.color = tint;
            renderer.sortingOrder = sortingOrder;
            actor.transform.localScale = new Vector3(0.72f, 0.72f, 1f);

            ActorView actorView = actor.AddComponent<ActorView>();
            actorView.Initialize(renderer, fallbackSprite);
            actorView.SetPositionImmediate(cell);

            var outline = actorView.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.yellow;
            return actorView;
        }

        public DamageEffectView CreateDamageEffect(
            Transform parent,
            Vector2Int cell,
            Sprite[] frames,
            Color tint,
            string name,
            int sortingOrder,
            float frameRate,
            float scale)
        {
            GameObject effect = new GameObject(name);
            effect.transform.SetParent(parent, false);
            effect.transform.position = new Vector3(cell.x, cell.y, -0.2f);
            effect.transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);

            Sprite fallbackSprite = GetSquareSprite();
            SpriteRenderer renderer = effect.AddComponent<SpriteRenderer>();
            renderer.sprite = GetFirstValidSprite(frames) ?? fallbackSprite;
            renderer.color = tint;
            renderer.sortingOrder = sortingOrder;

            DamageEffectView effectView = effect.AddComponent<DamageEffectView>();
            effectView.Initialize(renderer, frames, fallbackSprite, frameRate);
            return effectView;
        }

        private static Sprite GetFirstValidSprite(Sprite[] frames)
        {
            if (frames == null)
            {
                return null;
            }

            for (int i = 0; i < frames.Length; i++)
            {
                if (frames[i] != null)
                {
                    return frames[i];
                }
            }

            return null;
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
