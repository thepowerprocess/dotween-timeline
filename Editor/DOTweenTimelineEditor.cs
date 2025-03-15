using System;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEditor;
using UnityEngine;

namespace Dott.Editor
{
    [CustomEditor(typeof(DOTweenTimeline))]
    public class DOTweenTimelineEditor : UnityEditor.Editor
    {
        private DOTweenTimeline Timeline => (DOTweenTimeline)target;

        private DottController controller;
        private DottSelection selection;
        private DottView view;
        private float? dragTweenTimeShift;
        private DottAnimation[] animations;

        public override bool RequiresConstantRepaint() => true;

        public override void OnInspectorGUI()
        {
            animations = Timeline.GetComponents<ABSAnimationComponent>().Select(DottAnimation.Create).ToArray();
            selection.Validate(animations);

            view.DrawTimeline(animations, selection.Animation, controller.IsPlaying, controller.ElapsedTime,
                controller.Loop, controller.FreezeFrame, controller.Paused);

            if (selection.Animation != null)
            {
                view.DrawInspector(selection.GetAnimationEditor());
            }

            if (controller.Paused && Event.current.type == EventType.Repaint)
            {
                controller.GoTo(animations, controller.ElapsedTime);
            }
        }

        private void OnEnable()
        {
            controller = new DottController();
            selection = new DottSelection();
            view = new DottView();

            view.TweenSelected += OnTweenSelected;
            view.TweenDrag += DragSelectedAnimation;

            view.TimeDragStart += controller.Stop;
            view.TimeDragEnd += OnTimeDragEnd;
            view.TimeDrag += GoTo;

            view.AddClicked += AddAnimation;
            view.CallbackClicked += AddCallback;
            view.RemoveClicked += Remove;
            view.DuplicateClicked += Duplicate;

            view.PlayClicked += Play;
            view.StopClicked += controller.Stop;
            view.LoopToggled += ToggleLoop;
            view.FreezeFrameClicked += ToggleFreeze;
        }

        private void OnDisable()
        {
            view.TweenSelected -= OnTweenSelected;
            view.TweenDrag -= DragSelectedAnimation;

            view.TimeDragStart -= controller.Stop;
            view.TimeDragEnd -= OnTimeDragEnd;
            view.TimeDrag += GoTo;

            view.AddClicked -= AddAnimation;
            view.CallbackClicked -= AddCallback;
            view.RemoveClicked -= Remove;
            view.DuplicateClicked -= Duplicate;

            view.PlayClicked -= Play;
            view.StopClicked -= controller.Stop;
            view.LoopToggled -= ToggleLoop;
            view.FreezeFrameClicked -= ToggleFreeze;

            controller.Dispose();
            controller = null;

            selection.Dispose();
            selection = null;

            view = null;

            animations = null;
        }

        private void Play()
        {
            controller.Play(animations);
        }

        private void GoTo(float time)
        {
            controller.GoTo(animations, time);
        }

        private void OnTimeDragEnd()
        {
            if (controller.FreezeFrame)
            {
                controller.Pause();
            }
            else
            {
                controller.Stop();
            }
        }

        private void DragSelectedAnimation(float time)
        {
            dragTweenTimeShift ??= time - selection.Animation.Delay;

            var delay = time - dragTweenTimeShift.Value;
            delay = Mathf.Max(0, delay);
            delay = (float)Math.Round(delay, 2);
            selection.Animation.Delay = delay;
        }

        private void OnTweenSelected(DottAnimation animation)
        {
            selection.Set(animation);
            // clear focus to correctly update inspector
            GUIUtility.keyboardControl = 0;

            dragTweenTimeShift = null;
        }

        private void AddAnimation()
        {
            Add<DOTweenAnimation>(Timeline);
        }

        private void AddCallback()
        {
            Add<DOTweenCallback>(Timeline);
        }

        private void Add<T>(DOTweenTimeline timeline) where T : ABSAnimationComponent
        {
            var component = ObjectFactory.AddComponent<T>(timeline.gameObject);
            var animation = DottAnimation.Create(component);
            selection.Set(animation);
        }

        private void Remove()
        {
            Undo.DestroyObjectImmediate(selection.Animation.Component);
            selection.Clear();
        }

        private void Duplicate()
        {
            var source = selection.Animation.Component;

            var dest = source.gameObject.AddComponent(source.GetType());
            EditorUtility.CopySerialized(source, dest);

            var animation = DottAnimation.Create((ABSAnimationComponent)dest);
            selection.Set(animation);
        }

        private void ToggleLoop(bool value)
        {
            controller.Loop = value;
        }

        private void ToggleFreeze(bool value)
        {
            controller.FreezeFrame = value;

            if (!value)
            {
                controller.Stop();
            }
        }
    }
}