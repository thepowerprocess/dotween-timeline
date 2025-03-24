using DG.Tweening;
using UnityEngine;

namespace Dott
{
    public static class DOTweenTimelineExtensions
    {
        public static Sequence AsSequence(this DOTweenTimeline timeline)
        {
            var sequence = DOTween.Sequence();
            var components = timeline.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                switch (component)
                {
                    case DOTweenAnimation animation:
                        animation.CreateTween(regenerateIfExists: true);
                        sequence.Insert(0, animation.tween);
                        break;

                    case IDOTweenAnimation animation:
                        var tween = animation.CreateTween(regenerateIfExists: true);
                        sequence.Insert(0, tween);
                        break;
                }
            }

            return sequence;
        }
    }
}