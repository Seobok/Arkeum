using UnityEngine;
using UnityEngine.InputSystem;

namespace Arkeum.Prototype
{
    public sealed partial class PrototypeGameController
    {
        private bool TryHandleHubMovement(Keyboard keyboard)
        {
            if (!inputReader.TryGetDirectionalInput(keyboard, out Vector2Int direction))
            {
                return false;
            }

            Vector2Int target = hubPlayerPosition + direction;
            if (TryHandleHubInteractionAt(target))
            {
                return true;
            }

            if (!hubLayout.IsWalkable(target))
            {
                SetMessage("The path is blocked.");
                return true;
            }

            hubPlayerPosition = target;
            SyncHubPlayerView();
            OnHubTileEntered();
            UpdateCamera();
            return true;
        }

        private void EnterHub(string message)
        {
            Phase = GamePhase.Hub;
            Run = null;
            player = null;
            enemies.Clear();
            ClearSpawnedViews();
            BuildHub();
            SetMessage(message);
            hud.DialogueLine = GetUndertakerGreeting();
            EnsureCamera();
            UpdateCamera();
        }

        private void TryUnlockStartingBandage()
        {
            progressionService.TryUnlockStartingBandage(Profile, saveService, StartingBandageUnlockCost, out string message);
            SetMessage(message);
        }

        private void AdvanceUndertakerDialogue()
        {
            if (undertakerLines.Count == 0)
            {
                SeedDialogue();
            }

            hud.DialogueLine = undertakerLines.Dequeue();
            undertakerLines.Enqueue(hud.DialogueLine);
            SetMessage("The undertaker speaks without emotion.");
        }

        private string GetUndertakerGreeting()
        {
            return progressionService.GetUndertakerGreeting(Profile);
        }

        private void BuildHub()
        {
            HubSceneLayout hubSceneLayout = layoutFactory.BuildHubLayout();
            hubLayout = hubSceneLayout.Layout;
            hubStartGatePosition = hubSceneLayout.StartGatePosition;
            hubUnlockPosition = hubSceneLayout.UnlockPosition;
            hubUndertakerPosition = hubSceneLayout.UndertakerPosition;
            hubPlayerPosition = hubSceneLayout.PlayerPosition;

            DrawHubLayout();
            hubPlayerView = viewFactory.CreateActor(actorRoot, "HubPlayer", hubPlayerPosition, new Color(0.91f, 0.86f, 0.78f), 20);
            spawnedViews.Add(hubPlayerView);
        }

        private void DrawHubLayout()
        {
            for (int x = -3; x <= 3; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    if (!hubLayout.IsWalkable(cell))
                    {
                        continue;
                    }

                    spawnedViews.Add(viewFactory.CreateCell(floorRoot, cell, new Color(0.10f, 0.09f, 0.11f), $"HubFloor_{x}_{y}", 0));
                }
            }

            spawnedViews.Add(viewFactory.CreateCell(markerRoot, hubStartGatePosition, new Color(0.62f, 0.29f, 0.22f), "HubStartGate", 2));
            spawnedViews.Add(viewFactory.CreateCell(markerRoot, hubUnlockPosition, new Color(0.84f, 0.73f, 0.28f), "HubUnlock", 2));
            spawnedViews.Add(viewFactory.CreateCell(markerRoot, hubUndertakerPosition, new Color(0.19f, 0.55f, 0.51f), "HubUndertaker", 2));

            GameObject undertaker = viewFactory.CreateActor(actorRoot, "Undertaker", hubUndertakerPosition, new Color(0.19f, 0.55f, 0.51f), 10);
            spawnedViews.Add(undertaker);
        }

        private void SyncHubPlayerView()
        {
            if (hubPlayerView != null)
            {
                hubPlayerView.transform.position = new Vector3(hubPlayerPosition.x, hubPlayerPosition.y, -0.1f);
            }
        }

        private void OnHubTileEntered()
        {
            if (hubPlayerPosition == hubStartGatePosition)
            {
                SetMessage("Move into the start altar to begin a run.");
                return;
            }

            if (hubPlayerPosition == hubUnlockPosition)
            {
                SetMessage(Profile.unlockedStartingBandage
                    ? "The bandage unlock altar is already active."
                    : $"Move into the unlock altar to spend {StartingBandageUnlockCost} gleam.");
                return;
            }

            if (hubPlayerPosition == hubUndertakerPosition)
            {
                SetMessage("Move into the undertaker to continue the conversation.");
                return;
            }

            SetMessage("The embers of the return altar flicker quietly.");
        }

        private bool TryHandleHubInteractionAt(Vector2Int target)
        {
            if (target == hubStartGatePosition)
            {
                StartRun();
                return true;
            }

            if (target == hubUnlockPosition)
            {
                TryUnlockStartingBandage();
                return true;
            }

            if (target == hubUndertakerPosition)
            {
                AdvanceUndertakerDialogue();
                return true;
            }

            return false;
        }

        private void SeedDialogue()
        {
            progressionService.SeedDialogue(undertakerLines);
        }
    }
}
