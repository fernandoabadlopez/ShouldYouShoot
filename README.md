# ShouldYouShoot

A **Multimodal Interaction Game** that blends **Augmented Reality (AR)** and **Virtual Reality (VR)** to challenge players with moral dilemmas involving historical figures.

## Concept

Players encounter historical figures at pivotal moments in history and must decide: **Should you shoot?**  
The game does not reward impulsive shooting — it rewards moral reasoning. Each scenario presents the historical context *before* the figure's most notorious actions and asks the player to wrestle with questions of free will, justice, and consequence.

Example scenarios:
- A young Adolf Hitler painting in Vienna, 1913.
- Mao Zedong as a school teacher before the Cultural Revolution.
- Pol Pot as a student in Paris, 1950.

## Modes

| Mode | Description |
|------|-------------|
| **AR Mode** | Historical figures are overlaid onto the real world using your device camera (AR Foundation + ARCore/ARKit). |
| **VR Mode** | Full immersive virtual environments using the XR Interaction Toolkit (Meta Quest, SteamVR). |

## Technology Stack

- **Unity 2022.3 LTS**
- [AR Foundation 5.1](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1) — cross-platform AR
- [ARCore XR Plugin](https://docs.unity3d.com/Packages/com.unity.xr.arcore@5.1) — Android AR
- [ARKit XR Plugin](https://docs.unity3d.com/Packages/com.unity.xr.arkit@5.1) — iOS AR
- [XR Interaction Toolkit 2.5](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.5) — VR controllers
- [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0) — UI text
- [Input System 1.7](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7) — unified input

## Project Structure

```
Assets/
  Scripts/
    Data/
      HistoricalCharacter.cs   # Character data model
      MoralDilemma.cs          # Dilemma scenario model
    Core/
      GameManager.cs           # Game state machine
      ScoreManager.cs          # Moral scoring system
    Mechanics/
      ShootingMechanic.cs      # AR/VR shooting input handler
    AR/
      ARModeController.cs      # AR Foundation scene controller
    VR/
      VRModeController.cs      # XR Interaction Toolkit controller
    UI/
      UIManager.cs             # HUD, dilemma prompts, outcome display
  Scenes/
    MainMenu.unity
    ARScene.unity
    VRScene.unity
  Resources/
    historical_characters.json # Character + scenario data
Packages/
  manifest.json
ProjectSettings/
  ProjectSettings.asset
```

## Getting Started

1. Install **Unity 2022.3 LTS** via Unity Hub.
2. Open the project folder in Unity Hub → *Add project from disk*.
3. Unity will import packages automatically from `Packages/manifest.json`.
4. For **AR**:  
   - Open `Assets/Scenes/ARScene.unity`.  
   - Build for Android (ARCore) or iOS (ARKit).
5. For **VR**:  
   - Open `Assets/Scenes/VRScene.unity`.  
   - Enable your XR plug-in (Oculus / OpenXR) in *Project Settings → XR Plug-in Management*.  
   - Build for your target headset.
6. For quick desktop testing, open `Assets/Scenes/MainMenu.unity` in Play Mode.

## Moral Scoring

The score is **not** based purely on eliminating targets. The `ScoreManager` weighs:

| Factor | Points |
|--------|--------|
| Correct shoot decision (villain before atrocities) | +100 |
| Incorrect shoot (innocent historical figure) | −200 |
| Sparing a figure who would later do good | +150 |
| Deciding without reading any context clues | −50 |
| Reading all context clues before deciding | up to +30 |
| Time bonus (quick but correct decision) | up to +30 |

## Ethical Note

This game is designed as an **educational thought experiment** to explore moral philosophy, the trolley problem, and questions of historical determinism. It does not glorify violence.