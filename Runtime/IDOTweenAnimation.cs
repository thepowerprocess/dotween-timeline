using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;

namespace Dott
{
    public interface IDOTweenAnimation
    {
        Tween CreateTween(bool regenerateIfExists, bool andPlay = true);

        #region Timeline editor dependencies

        float Delay { get; set; }
        float Duration { get; }
        int Loops { get; }
        bool IsValid { get; }
        bool IsActive { get; }
        bool IsFrom { get; }
        string Label { get; }
        Component Component { get; }
        [CanBeNull] Tween CreateEditorPreview();

        #endregion
    }
}