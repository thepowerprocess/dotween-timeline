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
        private IDOTweenAnimation[] currentPlayAnimations;

        public bool IsPlaying => DottEditorPreview.IsPlaying;
        public float ElapsedTime => (float)(DottEditorPreview.CurrentTime - startTime);
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

            var shift = (float)DottEditorPreview.CurrentTime;
            GoTo(animations, shift);
            DottEditorPreview.Start();
            startTime = DottEditorPreview.CurrentTime - shift;
            Paused = false;
        }

        public void GoTo(IDOTweenAnimation[] animations, in float time)
        {
            DottEditorPreview.Stop();

            Sort(animations).ForEach(PreviewTween);
            DottEditorPreview.GoTo(time);
            startTime = 0;
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