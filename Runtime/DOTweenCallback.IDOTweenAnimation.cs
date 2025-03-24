using DG.Tweening;
using UnityEngine;

namespace Dott
{
    public partial class DOTweenCallback : IDOTweenAnimation
    {
        float IDOTweenAnimation.Delay
        {
            get => delay;
            set => delay = value;
        }

        float IDOTweenAnimation.Duration => 0;
        int IDOTweenAnimation.Loops => 0;
        bool IDOTweenAnimation.IsValid => true;
        bool IDOTweenAnimation.IsActive => true;
        bool IDOTweenAnimation.IsFrom => false;
        string IDOTweenAnimation.Label => string.Empty;
        Component IDOTweenAnimation.Component => this;

        Tween IDOTweenAnimation.CreateEditorPreview() =>
            // Need to return a valid tween for preview playback
            DOTween.Sequence().InsertCallback(delay, () => { });
    }
}