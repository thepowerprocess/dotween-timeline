# DOTween Timeline
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](LICENSE)

A pocket timeline solution for DOTween Pro. Configure and organize complex tween animations directly in the Inspector.

## Installation
1. **Required**: [**PRO**](https://dotween.demigiant.com/pro.php) version of DOTween.
2. Download the ```.unitypackage``` file from the [Releases page](https://github.com/medvejut/dotween-timeline/releases).
3. Import it into your project via **Assets > Import Package**.

## Usage
Add the component **DOTween > DOTween Timeline** to your GameObject (use a separate GameObject for each animation sequence).

Tweens will play automatically, but you can also control them from code:

```c#
[SerializeField] private DOTweenTimeline timeline;

var tween = timeline.AsSequence().Pause();

tween.OnComplete(() => Debug.Log("OnComplete"));
tween.Play();
```

## DOTweenCallback component
Sequence.InsertCallback(), but it's a component.

1. Add the component **DOTween > DOTween Callback** to your GameObject.
2. Configure the _delay_ and optional _id_
3. Set the _onCallback_ UnityEvent in the Inspector or from code:
```c#
[SerializeField] private DOTweenCallback callback;

callback.onCallback.AddListener(() => Debug.Log("Callback"));
```
