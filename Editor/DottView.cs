using System;
using System.Linq;
using DG.DemiEditor;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;

namespace Dott.Editor
{
    public class DottView
    {
        private bool isTimeDragging;
        private bool isTweenDragging;
        private static readonly AddMoreItem[] AddMoreItems = CreateAddMoreItems();

        public event Action TimeDragEnd;
        public event Action<float> TimeDrag;
        public event Action<IDOTweenAnimation> TweenSelected;
        public event Action<float> TweenDrag;
        public event Action AddClicked;
        public event Action<Type> AddMore;
        public event Action RemoveClicked;
        public event Action DuplicateClicked;
        public event Action StopClicked;
        public event Action PlayClicked;
        public event Action<bool> LoopToggled;
        public event Action<bool> FreezeFrameClicked;

        public void DrawTimeline(IDOTweenAnimation[] animations, [CanBeNull] IDOTweenAnimation selected, bool isPlaying, float currentPlayingTime, bool isLooping, bool isFreezeFrame, bool isPaused)
        {
            var rect = DottGUI.GetTimelineControlRect(animations.Length);

            DottGUI.Background(rect);
            DottGUI.Header(rect);

            var timeScale = CalculateTimeScale(animations);
            var timeDragStarted = false;
            var timeRect = DottGUI.Time(rect, timeScale, ref isTimeDragging, () => timeDragStarted = true, TimeDragEnd);
            var tweensRect = DottGUI.Tweens(rect, animations, timeScale, selected, ref isTweenDragging, TweenSelected);

            if (DottGUI.AddButton(rect))
            {
                AddClicked?.Invoke();
            }

            DottGUI.AddMoreButton(rect, AddMoreItems, item => AddMore?.Invoke(item.Type));

            if (selected != null && DottGUI.RemoveButton(rect))
            {
                RemoveClicked?.Invoke();
            }

            if (selected != null && DottGUI.DuplicateButton(rect))
            {
                DuplicateClicked?.Invoke();
            }

            if (isPlaying || isPaused)
            {
                var time = currentPlayingTime * timeScale;
                var verticalRect = timeRect.Add(tweensRect);
                DottGUI.TimeVerticalLine(verticalRect, time, isPaused);

                if (isPaused)
                {
                    DottGUI.PlayheadLabel(timeRect, time);
                }
            }

            if (isTimeDragging)
            {
                var time = DottGUI.GetScaledTimeUnderMouse(timeRect);
                DottGUI.TimeVerticalLine(timeRect.Add(tweensRect), time, underLabel: true);
                DottGUI.PlayheadLabel(timeRect, time);

                if (Event.current.type is EventType.MouseDrag || timeDragStarted)
                {
                    TimeDrag?.Invoke(time / timeScale);
                }
            }

            if (isTweenDragging && selected != null)
            {
                var time = DottGUI.GetScaledTimeUnderMouse(timeRect);

                if (Event.current.type == EventType.MouseDrag)
                {
                    TweenDrag?.Invoke(time / timeScale);
                }
            }

            switch (isPlaying)
            {
                case true when DottGUI.StopButton(rect):
                    StopClicked?.Invoke();
                    break;
                case false when DottGUI.PlayButton(rect):
                    PlayClicked?.Invoke();
                    break;
            }

            var loopResult = DottGUI.LoopToggle(rect, isLooping);
            if (loopResult != isLooping)
            {
                LoopToggled?.Invoke(loopResult);
            }

            var freezeFrameResult = DottGUI.FreezeFrameToggle(rect, isFreezeFrame);
            if (freezeFrameResult != isFreezeFrame)
            {
                FreezeFrameClicked?.Invoke(freezeFrameResult);
            }

            if (selected != null && Event.current.type == EventType.MouseDown)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    TweenSelected?.Invoke(null);
                    Event.current.Use();
                }
            }
        }

        public void DrawInspector(UnityEditor.Editor editor)
        {
            DottGUI.Inspector(editor);
        }

        private static float CalculateTimeScale(IDOTweenAnimation[] animations)
        {
            var maxTime = animations.Length > 0
                ? animations.Max(animation => animation.Delay + animation.Duration * Mathf.Max(1, animation.Loops))
                : 1f;
            return 1f / maxTime;
        }

        private static AddMoreItem[] CreateAddMoreItems()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(type => type.IsClass && !type.IsAbstract && typeof(IDOTweenAnimation).IsAssignableFrom(type))
                .ToArray();

            return types
                .Select((type, _) => new AddMoreItem(new GUIContent($"Add {type.Name.Replace("DOTween", "")}"), type))
                .Prepend(new AddMoreItem(new GUIContent("Add Tween"), typeof(DOTweenAnimation)))
                .ToArray();
        }

        public struct AddMoreItem
        {
            public readonly GUIContent Content;
            public readonly Type Type;

            public AddMoreItem(GUIContent content, Type type)
            {
                Content = content;
                Type = type;
            }
        }
    }
}