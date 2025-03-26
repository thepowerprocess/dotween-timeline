using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;

namespace Dott
{
    [AddComponentMenu("DOTween/DOTween Timeline")]
    public class DOTweenTimeline : MonoBehaviour
    {
        [SerializeField] private bool autoOldWay = true;

        [CanBeNull] public Sequence Sequence { get; private set; }

        public Sequence Play()
        {
            GenerateSequence();
            return Sequence.Play();
        }

        public void Pause() => Sequence.Pause();

        public void Restart() => Sequence.Restart();

        public void Rewind() => Sequence.Rewind();

        public void TogglePause() => Sequence.TogglePause();

        private void GenerateSequence()
        {
            if (Sequence != null) { return; }

            Sequence = DOTween.Sequence();
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

        private void OnValidate()
        {
            if (autoOldWay) { return; }

            foreach (var doTweenAnimation in GetComponents<DOTweenAnimation>())
            {
                doTweenAnimation.autoGenerate = false;
            }

            foreach (var doTweenCallback in GetComponents<DOTweenCallback>())
            {
                doTweenCallback.autoGenerate = false;
            }
        }
    }
}