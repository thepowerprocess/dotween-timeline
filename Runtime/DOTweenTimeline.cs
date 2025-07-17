using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;

namespace Dott
{
    [AddComponentMenu("DOTween/DOTween Timeline")]
    public class DOTweenTimeline : MonoBehaviour
    {
        [Tooltip("Automatically hide other DOTweenAnimation components on this GameObject when timeline is active")]
        public bool autoHideAnimationComponents = true;

        [CanBeNull] public Sequence Sequence { get; private set; }

        private DOTweenAnimation[] hiddenAnimations;
        private bool wasAutoHideEnabled;

        // Do not override the onKill callback because it is used internally to reset the Sequence
        public Sequence Play()
        {
            TryGenerateSequence();
            return Sequence.Play();
        }

        // Wrapper for UnityEvent (requires void return type)
        public void DOPlay() => Play();

        public Sequence Restart()
        {
            TryGenerateSequence();
            Sequence.Restart();
            return Sequence;
        }

        private void TryGenerateSequence()
        {
            if (Sequence != null) { return; }

            Sequence = DOTween.Sequence();
            Sequence.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            Sequence.OnKill(() => Sequence = null);
            var components = GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                switch (component)
                {
                    case DOTweenAnimation animation:
                        animation.CreateTween(regenerateIfExists: true);
                        Sequence.Insert(0, animation.tween);
                        break;

                    case IDOTweenAnimation animation:
                        var tween = animation.CreateTween(regenerateIfExists: true);
                        Sequence.Insert(0, tween);
                        break;
                }
            }
        }

        private void OnEnable()
        {
            if (autoHideAnimationComponents)
            {
                HideOtherAnimations();
            }
        }

        public void OnDisable()
        {
            if (wasAutoHideEnabled)
            {
                ShowOtherAnimations();
            }
        }

        // Public method for editor cleanup
        public void RestoreHiddenAnimations()
        {
            if (wasAutoHideEnabled)
            {
                ShowOtherAnimations();
            }
        }

        // Static method to restore hidden animations on any GameObject
        public static void RestoreHiddenAnimationsOnGameObject(GameObject gameObject)
        {
            if (gameObject == null) return;

            var animations = gameObject.GetComponents<DOTweenAnimation>();
            foreach (var animation in animations)
            {
                if (animation != null && (animation.hideFlags & HideFlags.HideInInspector) != 0)
                {
                    animation.hideFlags &= ~HideFlags.HideInInspector;
                }
            }
        }

        private void OnDestroy()
        {
            // Already handled by SetLink, but needed to avoid warnings from children DOTweenAnimation.OnDestroy
            Sequence?.Kill();

            // Restore hidden animations when timeline is destroyed
            if (wasAutoHideEnabled)
            {
                ShowOtherAnimations();
            }

            // Also restore any DOTweenAnimation components that might still be hidden
            RestoreAllHiddenAnimationsOnGameObject();
        }

        private void RestoreAllHiddenAnimationsOnGameObject()
        {
            var allAnimations = GetComponents<DOTweenAnimation>();
            foreach (var animation in allAnimations)
            {
                if (animation != null && (animation.hideFlags & HideFlags.HideInInspector) != 0)
                {
                    animation.hideFlags &= ~HideFlags.HideInInspector;
                }
            }
        }

        private void HideOtherAnimations()
        {
            if (!autoHideAnimationComponents) return;

            var allAnimations = GetComponents<DOTweenAnimation>();
            var otherAnimations = new System.Collections.Generic.List<DOTweenAnimation>();

            foreach (var animation in allAnimations)
            {
                if (animation != this) // Don't hide the timeline itself
                {
                    otherAnimations.Add(animation);
                }
            }

            hiddenAnimations = otherAnimations.ToArray();
            wasAutoHideEnabled = true;

            foreach (var animation in hiddenAnimations)
            {
                if (animation != null)
                {
                    // Use HideFlags to hide from inspector
                    animation.hideFlags |= HideFlags.HideInInspector;
                }
            }
        }

        private void ShowOtherAnimations()
        {
            if (hiddenAnimations == null) return;

            foreach (var animation in hiddenAnimations)
            {
                if (animation != null)
                {
                    // Remove HideInInspector flag
                    animation.hideFlags &= ~HideFlags.HideInInspector;
                }
            }

            hiddenAnimations = null;
            wasAutoHideEnabled = false;
        }

        public void OnValidate()
        {
            foreach (var doTweenAnimation in GetComponents<DOTweenAnimation>())
            {
                doTweenAnimation.autoGenerate = false;
            }

            // Handle auto-hide setting changes
            if (autoHideAnimationComponents && !wasAutoHideEnabled)
            {
                HideOtherAnimations();
            }
            else if (!autoHideAnimationComponents && wasAutoHideEnabled)
            {
                ShowOtherAnimations();
            }
        }
    }
}