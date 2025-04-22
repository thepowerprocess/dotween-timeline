# DOTween Timeline
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](LICENSE)

A pocket timeline solution for DOTween Pro. Configure and organize complex tween animations directly in the Inspector.

![ezgif-478bb6b997c38b](https://github.com/user-attachments/assets/1cc3d251-d4a8-476a-9dc5-0b43ebe395d4)

## Installation
> [!IMPORTANT]
> **Required**: [**PRO**](https://dotween.demigiant.com/pro.php) version of DOTween.

### Releases page
Easiest way is to install DOTween Timeline as an asset package.
1. Download the latest ```.unitypackage``` file from the [Releases page](https://github.com/medvejut/dotween-timeline/releases).
2. Import it into your project via **Assets > Import Package > Custom Package**.

### Git UPM
You can also install this package via Git URL using Unity Package Manager.
1. Since DOTween is not distributed as a upm package by default, you need to manually generate `.asmdef` files for it:\
  Open **Tools > Demigiant > DOTween Utility Panel**, click **Create ASMDEF**
2. Then, add the following line to your `Packages/manifest.json`:
```
"com.medvejut.dotweentimeline": "https://github.com/medvejut/dotween-timeline.git#upm"
```

## How to use
Add the **DOTween > DOTween Timeline** component to a GameObject (use a separate GameObject for each animation sequence).

Control the timeline from code:

```c#
[SerializeField] private DOTweenTimeline timeline;

var tween = timeline.Play();

tween.OnComplete(() => Debug.Log("OnComplete"));
tween.Pause();
```

### Sample
A sample scene is included to help you get started. You can find it here: _Plugins/DOTweenTimeline/Sample_\
Open it to see an example of how to configure and use the timeline in practice.

## Recommendations

### 1. Disable default DOTween Pro preview controls
In the Inspector, on any DOTween animation component:\
![Group 5 (2)](https://github.com/user-attachments/assets/e8e3c39e-a1b0-4d4a-bd2d-de2af567eca7)

### 2. Enable TextMeshPro support in DOTween
Go to _Tools > Demigiant > DOTween Utility Panel_ > press _"Setup DOTween..."_ > enable _TextMeshPro_:\
![Mask group](https://github.com/user-attachments/assets/1674e9e9-ac6c-4b73-a278-37a548806a23)

## Extras
### DOTween Timeline Player component
Automatically plays animations without code.\
Just add the **DOTween > DOTween Timeline Player** component to the same GameObject with the Timeline.

### Extra Actions
In addition to standard tweens, you can add special timeline actions via the Add dropdown:\
![Mask group (1)](https://github.com/user-attachments/assets/dc48d249-56f2-41cb-8259-b6aa8db3e46e)

### DOTweenCallback component
A visual replacement for Sequence.InsertCallback().\
Use the _onCallback_ UnityEvent in the Inspector or from code:
```c#
[SerializeField] private DOTweenCallback callback;

callback.onCallback.AddListener(() => Debug.Log("Callback"));
```
<img width="477" alt="Снимок экрана 2025-04-05 в 22 16 03" src="https://github.com/user-attachments/assets/746fca7e-1d70-4127-ba92-330c0f7470e6" />

### DOTweenFrame component
Triggers immediate state changes in a single frame.\
Perfect for setting position, rotation, color, and more, without animation.\
<img width="490" alt="Снимок экрана 2025-04-05 в 22 20 15" src="https://github.com/user-attachments/assets/df9226e8-dc83-419b-b1ca-daaf6b70811a" />

## Inspired by
- [Animation Creator Timeline (UI Tween)](https://assetstore.unity.com/packages/tools/animation/animation-creator-timeline-ui-tween-186589)
- [DOTween Timeline Preview](https://www.youtube.com/watch?v=hrX0xZ3JCXU)
- [Jitter](https://jitter.video/)
