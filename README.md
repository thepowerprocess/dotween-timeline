# DOTween Timeline
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](LICENSE)

A pocket timeline solution for DOTween Pro. Configure and organize complex tween animations directly in the Inspector.

## Installation
1. **Required**: [**PRO**](https://dotween.demigiant.com/pro.php) version of DOTween.
2. Download the latest ```.unitypackage``` file from the [Releases page](https://github.com/medvejut/dotween-timeline/releases).
3. Import it into your project via **Assets > Import Package**.

## How to use
Add the **DOTween > DOTween Timeline** component to a GameObject (use a separate GameObject for each animation sequence).

Control the timeline from code:

```c#
[SerializeField] private DOTweenTimeline timeline;

var tween = timeline.Play();

tween.OnComplete(() => Debug.Log("OnComplete"));
tween.Pause();
```
## Recommendations

### 1. Disable default DOTween Pro preview controls
In the Inspector, on any DOTween animation component:

![Group 5 (2)](https://github.com/user-attachments/assets/e8e3c39e-a1b0-4d4a-bd2d-de2af567eca7)

### 2. Enable TextMeshPro support in DOTween
Go to _Tools > Demigiant > DOTween Utility Panel_ > press _"Setup DOTween..."_ > enable _TextMeshPro_:

![Mask group](https://github.com/user-attachments/assets/1674e9e9-ac6c-4b73-a278-37a548806a23)

## Extras
### DOTween Timeline Player component
Automatically plays animations without code.\
Just add the **DOTween > DOTween Timeline Player** component to the same GameObject with the Timeline.

### Extra Actions
In addition to standard tweens, you can add special timeline actions via the Add dropdown:

![Mask group (1)](https://github.com/user-attachments/assets/dc48d249-56f2-41cb-8259-b6aa8db3e46e)

### DOTweenCallback component
A visual replacement for Sequence.InsertCallback().\
Use the _onCallback_ UnityEvent in the Inspector or from code:
```c#
[SerializeField] private DOTweenCallback callback;

callback.onCallback.AddListener(() => Debug.Log("Callback"));
```

### DOTweenFrame component
Triggers immediate state changes in a single frame.\
Perfect for setting position, rotation, color, and more, without animation.
