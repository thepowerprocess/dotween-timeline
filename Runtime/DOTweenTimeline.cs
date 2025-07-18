using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dott
{
    [ExecuteAlways]
    [AddComponentMenu("DOTween/DOTween Timeline")]
    public class DOTweenTimeline : MonoBehaviour
    {
        //if timeline is disabled or deleted it unhides the animation components
        private static bool HIDE_ANIMATION_COMPONENTS = true;

        [CanBeNull] public Sequence Sequence { get; private set; }

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

#if UNITY_EDITOR
        private void OnEnable()
        {
            HideAnimationComponents();
        }

        public void OnDisable()
        {
            ShowAnimationComponents();
        }

        // Public method for editor cleanup
        public void RestoreHiddenAnimations()
        {
            ShowAnimationComponents();
        }

        // Public method for editor to handle new component addition
        public void HandleNewComponentAdded()
        {
            // Small delay to ensure the component is fully initialized
            EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    HideAnimationComponents();
                }
            };
        }

        // Static method to restore hidden animations on any GameObject
        public static void RestoreHiddenAnimationsOnGameObject(GameObject gameObject)
        {
            if (gameObject == null) return;

            // Restore DOTweenAnimation components
            var animations = gameObject.GetComponents<DOTweenAnimation>();
            foreach (var animation in animations)
            {
                if (animation != null && (animation.hideFlags & HideFlags.HideInInspector) != 0)
                {
                    animation.hideFlags &= ~HideFlags.HideInInspector;
                }
            }

            // Restore DOTweenCallback components
            var callbacks = gameObject.GetComponents<DOTweenCallback>();
            foreach (var callback in callbacks)
            {
                if (callback != null && (callback.hideFlags & HideFlags.HideInInspector) != 0)
                {
                    callback.hideFlags &= ~HideFlags.HideInInspector;
                }
            }

            // Restore DOTweenFrame components
            var frames = gameObject.GetComponents<DOTweenFrame>();
            foreach (var frame in frames)
            {
                if (frame != null && (frame.hideFlags & HideFlags.HideInInspector) != 0)
                {
                    frame.hideFlags &= ~HideFlags.HideInInspector;
                }
            }
        }

        private void HideAnimationComponents()
        {
            //disable hiding animation components if timeline is disabled
            if (!HIDE_ANIMATION_COMPONENTS || !enabled) return;
            // Hide DOTweenAnimation components
            var allAnimations = GetComponents<DOTweenAnimation>();
            foreach (var animation in allAnimations)
            {
                if (animation != this && animation != null && (animation.hideFlags & HideFlags.HideInInspector) == 0) // Don't hide the timeline itself and only hide if not already hidden
                {
                    // Use HideFlags to hide from inspector
                    animation.hideFlags |= HideFlags.HideInInspector;
                }
            }

            // Hide DOTweenCallback components
            var allCallbacks = GetComponents<DOTweenCallback>();
            foreach (var callback in allCallbacks)
            {
                if (callback != this && callback != null && (callback.hideFlags & HideFlags.HideInInspector) == 0) // Don't hide the timeline itself and only hide if not already hidden
                {
                    // Use HideFlags to hide from inspector
                    callback.hideFlags |= HideFlags.HideInInspector;
                }
            }

            // Hide DOTweenFrame components
            var allFrames = GetComponents<DOTweenFrame>();
            foreach (var frame in allFrames)
            {
                if (frame != this && frame != null && (frame.hideFlags & HideFlags.HideInInspector) == 0) // Don't hide the timeline itself and only hide if not already hidden
                {
                    // Use HideFlags to hide from inspector
                    frame.hideFlags |= HideFlags.HideInInspector;
                }
            }
        }

        private void ShowAnimationComponents()
        {
            // if (!HIDE_ANIMATION_COMPONENTS) return;
            // Show DOTweenAnimation components
            var allAnimations = GetComponents<DOTweenAnimation>();
            foreach (var animation in allAnimations)
            {
                if (animation != null && (animation.hideFlags & HideFlags.HideInInspector) != 0)
                {
                    // Remove HideInInspector flag
                    animation.hideFlags &= ~HideFlags.HideInInspector;
                }
            }

            // Show DOTweenCallback components
            var allCallbacks = GetComponents<DOTweenCallback>();
            foreach (var callback in allCallbacks)
            {
                if (callback != null && (callback.hideFlags & HideFlags.HideInInspector) != 0)
                {
                    // Remove HideInInspector flag
                    callback.hideFlags &= ~HideFlags.HideInInspector;
                }
            }

            // Show DOTweenFrame components
            var allFrames = GetComponents<DOTweenFrame>();
            foreach (var frame in allFrames)
            {
                if (frame != null && (frame.hideFlags & HideFlags.HideInInspector) != 0)
                {
                    // Remove HideInInspector flag
                    frame.hideFlags &= ~HideFlags.HideInInspector;
                }
            }
        }
#endif

        public void OnValidate()
        {
            // Disable auto-generation for DOTweenAnimation components
            foreach (var doTweenAnimation in GetComponents<DOTweenAnimation>())
            {
                doTweenAnimation.autoGenerate = false;
            }

#if UNITY_EDITOR
            // Auto-hide is now always enabled by default
            HideAnimationComponents();
#endif
        }

        private void OnDestroy()
        {
            // Already handled by SetLink, but needed to avoid warnings from children DOTweenAnimation.OnDestroy
            Sequence?.Kill();

#if UNITY_EDITOR
            // Restore hidden animations when timeline is destroyed
            ShowAnimationComponents();
#endif
        }
    }
}