using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEditor;

namespace Dott.Editor
{
    public class DottController : IDisposable
    {
        private double startTime;
        private float gotoTime;
        private IDOTweenAnimation[] currentPlayAnimations;

        public bool IsPlaying => DottEditorPreview.IsPlaying;
        public float ElapsedTime => Paused ? gotoTime : (float)(DottEditorPreview.CurrentTime - startTime);
        public bool Paused { get; private set; }

        public bool Loop
        {
            get => EditorPrefs.GetBool("Dott.Loop", false);
            set => EditorPrefs.SetBool("Dott.Loop", value);
        }

        public bool FreezeFrame
        {
            get => EditorPrefs.GetBool("Dott.FreezeFrame", false);
            set => EditorPrefs.SetBool("Dott.FreezeFrame", value);
        }

        public DottController()
        {
            DottEditorPreview.Completed += DottEditorPreviewOnCompleted;
        }

        public void Play(IDOTweenAnimation[] animations)
        {
            currentPlayAnimations = animations;

            Sort(animations).ForEach(PreviewTween);
            DottEditorPreview.Start();
            startTime = DottEditorPreview.CurrentTime;
            Paused = false;
        }

        public void GoTo(IDOTweenAnimation[] animations, in float time)
        {
            DottEditorPreview.Stop();

            gotoTime = time;
            var sortedAnimations = Sort(animations);
            foreach (var animation in sortedAnimations)
            {
                var tween = PreviewTween(animation);
                if (tween != null)
                {
                    var tweenTime = time - animation.Delay;
                    if (tween is Sequence)
                    {
                        // Sequences have no real delay, so we don't need to subtract it
                        tweenTime = time;
                        if (time < animation.Delay)
                        {
                            // Ensure time is 0 before the first child tween starts
                            // to prevent unexpected onRewind calls
                            tweenTime = 0;
                        }
                    }

                    tween.Goto(tweenTime, andPlay: false);
                }
            }

            DottEditorPreview.QueuePlayerLoopUpdate();
        }

        public void Stop()
        {
            currentPlayAnimations = null;
            Paused = false;
            DottEditorPreview.Stop();
        }

        public void Pause()
        {
            Paused = true;
        }

        [CanBeNull]
        private static Tween PreviewTween(IDOTweenAnimation animation)
        {
            if (!animation.IsValid || !animation.IsActive) { return null; }

            var tween = animation.CreateEditorPreview();
            if (tween == null) { return null; }

            DottEditorPreview.Add(tween, animation.IsFrom, animation.AllowEditorCallbacks);
            return tween;
        }

        private static IEnumerable<IDOTweenAnimation> Sort(IDOTweenAnimation[] animations)
        {
            return animations.OrderBy(animation => animation.Delay);
        }

        private void DottEditorPreviewOnCompleted()
        {
            if (!Loop)
            {
                Stop();
                return;
            }

            DottEditorPreview.Stop();
            Play(currentPlayAnimations);
        }

        public void Dispose()
        {
            Stop();
            DottEditorPreview.Completed -= DottEditorPreviewOnCompleted;
        }
    }
}