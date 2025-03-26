using System;
using DG.Tweening;

namespace Dott
{
    public static class DOTweenTimelineExtensions
    {
        [Obsolete("Use DOTweenTimeline.Play() instead")]
        public static Sequence AsSequence(this DOTweenTimeline timeline)
        {
            timeline.Play();
            return timeline.Sequence;
        }
    }
}