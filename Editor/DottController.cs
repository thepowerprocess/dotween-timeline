using System;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEditor;

namespace Dott.Editor
{
    public class DottController : IDisposable
    {
        private double startTime;
        private DottAnimation[] currentPlayAnimations;

        public bool IsPlaying => DottEditorPreview.IsPlaying;
        public float ElapsedTime => (float)(DottEditorPreview.CurrentTime - startTime);

        public bool Loop
        {
            get => EditorPrefs.GetBool("Dott.Loop", false);
            set => EditorPrefs.SetBool("Dott.Loop", value);
        }

        public DottController()
        {
            DottEditorPreview.Completed += DottEditorPreviewOnCompleted;
        }

        public void Play(DottAnimation[] animations)
        {
            currentPlayAnimations = animations;

            animations.ForEach(PreviewTween);
            DottEditorPreview.Start();
            startTime = DottEditorPreview.CurrentTime;
        }

        public void GoTo(DottAnimation[] animations, in float time)
        {
            DottEditorPreview.Stop();

            foreach (var animation in animations)
            {
                var tween = PreviewTween(animation);
                if (tween != null)
                {
                    var tweenTime = time - animation.Delay;
                    tween.Goto(tweenTime, andPlay: false);
                }
            }
        }

        public void Stop()
        {
            currentPlayAnimations = null;
            DottEditorPreview.Stop();
        }

        [CanBeNull]
        private static Tween PreviewTween(DottAnimation animation)
        {
            if (!animation.IsValid || !animation.IsActive) { return null; }

            var tween = animation.CreateEditorPreview();
            if (tween == null) { return null; }

            DottEditorPreview.Add(tween, animation.IsFrom);
            return tween;
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