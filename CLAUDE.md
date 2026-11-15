# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity 6 project (6000.6.5f1) based on the "Unity Essentials" learning template. Uses Universal Render Pipeline (URP) and the new Input System. The solution file is `My project.slnx` with a single assembly `Assembly-CSharp.csproj`.

## Key Packages

- **Rendering**: URP (`com.unity.render-pipelines.universal` 17.6.0)
- **Input**: New Input System (`com.unity.inputsystem` 1.20.0)
- **Camera**: Cinemachine 6.6.0
- **UI**: TextMesh Pro (via `com.unity.ugui`), uGUI
- **AI**: Unity AI Assistant and Inference packages

## Architecture

### VR Historia (Juego de Terror) — `Assets/_VR_Historia/`

All horror game content lives under `Assets/_VR_Historia/`:

- **Scripts/** — 44 C# scripts organized by system:
  - `Core/` — GameManagerVR, InventarioVR, DiarioDisparador
  - `Atmosphere/` — ambient sound, post-processing
  - `Dialogue/` — DialogueGraph, VRDialogueManager, VRSubtitleDisplay
  - `Interaction/` — InteractionProfile, VRInteractableObject, VRInteractionManager
  - `ISTR/` — 22 scripts for the ISTR building simulation (terror scenarios, rooms, audio, visuals)
  - `Scenes/` — chapter controllers (Despertar, Exploracion, Revelacion)
  - `Teleport/` — TeleportZone, VRTeleportManager, WorldMap
  - `UI/` — VRFadeScreen, VRHUD, VRMenuPrincipal
  - `Editor/` — VRScenarioBuilder
- **Data/** — ScriptableObject assets:
  - `Dialogues/` — dialogue graph assets (DG_*)
  - `Interactions/` — interaction profile assets (IP_*)
  - `Zones/` — teleport zone assets (TZ_*) + WorldMap_Principal
  - `Items/` — item definitions
- **Prefabs/** — VR prefabs (ChoiceButton, ObjetivoItem)

### Unity Essentials Template — `Assets/_Unity Essentials/`

Original learning template content (do not mix game code here):

- **Scenes** numbered 0-6 (plus -1 Starter): MainMenu, Playground Navigation, 3D, Audio, Programming, 2D TopDown, Bonus.
- **Scripts** in two locations:
  - `Scripts/Provided Scripts/` — runtime gameplay scripts (e.g., `PlayerController2D`, `Collectible2D`)
  - `Source Files/Scripts/` — scene management, camera, doors, audio, UI, and collectible counting
  - `Source Files/ThirdPersonController/Scripts/` — character controller and physics push
  - `Source Files/InputSystem/` — input wrapper (`StarterAssetsInputs`)
- **CustomSceneManager** (`Source Files/Scripts/CustomSceneManager.cs`) is a singleton using `DontDestroyOnLoad` — all scene loading goes through it.

### XR Configuration

- `Assets/XR/` — OpenXR plugin settings and loader
- `Assets/XRI/` — XR Interaction Toolkit settings

### General

- **Input** uses `InputActionReference` and `PlayerInput` component (new Input System), not the legacy `Input.GetAxis` API.
- **Physics**: 3D scenes use `CharacterController` + `OnControllerColliderHit`; 2D scenes use `Rigidbody2D` + `OnTriggerEnter2D`.

## Working with This Project

- Open in Unity 6 (6000.6.5f1 or compatible).
- C# scripts target the `Assembly-CSharp` assembly — no assembly definitions are used, so all scripts compile together.
- URP render pipeline assets are in `Assets/Settings/` (Performant, Balanced, HighFidelity variants).
- The `Library/`, `Temp/`, `Logs/`, and `UserSettings/` directories are Unity-generated and should not be version-controlled.
- `.meta` files must stay in sync with their corresponding assets — never delete a `.meta` file without also removing its asset (and vice versa).
