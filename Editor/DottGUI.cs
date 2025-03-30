using System;
using DG.DemiEditor;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dott.Editor
{
    public static class DottGUI
    {
        private const float ROW_HEIGHT = 20;
        private const int BOTTOM_HEIGHT = 30;
        private const int TIME_HEIGHT = 20;
        private const int HEADER_HEIGHT = 28;
        private static readonly Vector2 PlayButtonSize = new(44, 24);
        private static readonly Vector2 LoopToggleSize = new(24, 24);
        private static readonly Vector2 FreezeToggleSize = new(24, 24);

        private static readonly Color[] Colors =
        {
            Color.red, Color.green, Color.blue,
            Color.yellow, Color.cyan, Color.magenta
        };

        public static Rect GetTimelineControlRect(int tweenCount)
        {
            return EditorGUILayout.GetControlRect(false, HEADER_HEIGHT + TIME_HEIGHT + tweenCount * ROW_HEIGHT + BOTTOM_HEIGHT);
        }

        public static void Background(Rect rect)
        {
            RoundRect(rect, Color.black.SetAlpha(0.3f), borderRadius: 4);
            RoundRect(rect, Color.black, borderRadius: 4, borderWidth: 1);
        }

        public static void Header(Rect rect)
        {
            rect = rect.SetHeight(HEADER_HEIGHT);

            var style = EditorStyles.boldLabel;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(rect, "Timeline", style);

            var bottomLine = new Rect(rect.x, rect.y + rect.height, rect.width, 1);
            EditorGUI.DrawRect(bottomLine, Color.black);
        }

        public static Rect Time(Rect rect, float timeScale, ref bool isDragging, Action start, Action end)
        {
            rect = rect.ShiftY(HEADER_HEIGHT).SetHeight(TIME_HEIGHT);

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9, normal = { textColor = Color.white.SetAlpha(0.5f) }
            };

            const int count = 10;
            const float step = 1f / count;
            for (var i = 0; i < count; i++)
            {
                var time = i * step;
                var position = new Rect(rect.x + i * step * rect.width, rect.y, step * rect.width, rect.height);
                time /= timeScale;
                GUI.Label(position, time.ToString("0.00"), style);
            }

            var bottomLine = new Rect(rect.x, rect.y + rect.height, rect.width, 1);
            EditorGUI.DrawRect(bottomLine, Color.black);

            ProcessDragEvents(rect, ref isDragging, start, end);

            return rect;
        }

        private static void ProcessDragEvents(Rect rect, ref bool isDragging, Action start, Action end)
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown when !isDragging && rect.Contains(Event.current.mousePosition):
                    isDragging = true;
                    start?.Invoke();
                    break;

                case EventType.MouseUp when isDragging:
                    isDragging = false;
                    end?.Invoke();
                    break;
            }
        }

        public static float GetScaledTimeUnderMouse(Rect timeRect)
        {
            var time = (Event.current.mousePosition.x - timeRect.x) / timeRect.width;
            time = Mathf.Clamp01(time);
            return time;
        }

        public static Rect Tweens(Rect rect, IDOTweenAnimation[] animations, float timeScale, [CanBeNull] IDOTweenAnimation selected, ref bool isTweenDragging, Action<IDOTweenAnimation> tweenSelected)
        {
            rect = rect.ShiftY(HEADER_HEIGHT + TIME_HEIGHT).SetHeight(animations.Length * ROW_HEIGHT);

            IDOTweenAnimation startDrag = null;

            for (var i = 0; i < animations.Length; i++)
            {
                var animation = animations[i];
                var rowRect = new Rect(rect.x, rect.y + i * ROW_HEIGHT, rect.width, ROW_HEIGHT);
                var isSelected = selected?.Component == animation.Component;
                var tweenRect = Element(animation, rowRect, isSelected, timeScale);

                ProcessDragEvents(tweenRect, ref isTweenDragging, start: Start, end: null);

                var bottomLine = new Rect(rowRect.x, rowRect.y + rowRect.height, rowRect.width, 1);
                EditorGUI.DrawRect(bottomLine, Color.black);
                continue;

                void Start()
                {
                    startDrag = animation;
                    tweenSelected?.Invoke(animation);
                }
            }

            if (startDrag == null && Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                tweenSelected?.Invoke(null);
            }

            return rect;
        }

        private static Rect Element(IDOTweenAnimation animation, Rect rowRect, bool isSelected, float timeScale)
        {
            if (animation.CallbackView)
            {
                return Callback(animation, rowRect, isSelected, timeScale);
            }

            return Tween(animation, rowRect, isSelected, timeScale);
        }

        private static Rect Callback(IDOTweenAnimation animation, Rect rowRect, bool isSelected, float timeScale)
        {
            void Label(Rect rect, GUIContent content, GUIStyle style)
            {
                GUI.Label(rect, content, style);
            }

            void Icon(bool isHovered, Rect iconRect)
            {
                var iconColor = Color.white.SetAlpha(0.6f);
                if (isSelected)
                {
                    iconColor = new Color(0.2f, 0.6f, 1f);
                }
                else if (isHovered)
                {
                    iconColor = Color.white.SetAlpha(0.5f);
                }

                var icon = animation.CustomIcon ?? IconCallback;
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true, 0, iconColor, 0, 0);
            }

            void Underline(bool isHovered, Rect textRect)
            {
                if (!isSelected && !isHovered) { return; }

                var underlineRect = new Rect(textRect.x, textRect.yMax - 4, textRect.width, 1);
                var color = isHovered ? Color.white.SetAlpha(0.7f) : Color.white;
                EditorGUI.DrawRect(underlineRect, color);
            }

            var textStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold, fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                richText = true
            };

            var iconX = CalculateX(rowRect, animation.Delay, timeScale);
            var iconRect = new Rect(iconX, rowRect.y, width: 10, height: 20);

            var labelContent = new GUIContent(animation.Label);

            textStyle.padding = new RectOffset((int)iconRect.width + 4, 0, 0, 1);
            var textWidth = textStyle.CalcSize(labelContent).x;
            var rect = new Rect(iconRect.x, rowRect.y, textWidth, rowRect.height);

            var onRightSide = rect.x > rowRect.x + rowRect.width * 0.5f;
            var outOfBounds = rect.xMax > rowRect.xMax;
            if (onRightSide && outOfBounds)
            {
                (textStyle.padding.right, textStyle.padding.left) = (textStyle.padding.left, textStyle.padding.right);
                rect.x = iconRect.xMax - textWidth;
            }

            var textOnlyRect = rect.Shift(textStyle.padding.left, 0, -textStyle.padding.horizontal, 0);
            var isHovered = rect.Contains(Event.current.mousePosition);

            Icon(isHovered, iconRect);
            Underline(isHovered, textOnlyRect);
            Label(rect, labelContent, textStyle);

            return rect;
        }

        private static Rect Tween(IDOTweenAnimation animation, Rect rowRect, bool isSelected, float timeScale)
        {
            var isInfinite = animation.Loops == -1;
            var loops = Mathf.Max(1, animation.Loops);
            var start = CalculateX(rowRect, animation.Delay, timeScale);
            var width = isInfinite
                ? rowRect.width - start + rowRect.x
                : animation.Duration * loops * timeScale * rowRect.width;

            var tweenRect = new Rect(start, rowRect.y, width, rowRect.height).Expand(-1);
            var alphaMultiplier = animation.IsActive ? 1f : 0.4f;

            RoundRect(tweenRect, Color.gray.SetAlpha(0.3f * alphaMultiplier), borderRadius: 4);

            if (isSelected)
            {
                RoundRect(tweenRect, Color.white.SetAlpha(0.9f * alphaMultiplier), borderRadius: 4, borderWidth: 2);
            }
            else
            {
                var mouseHover = tweenRect.Contains(Event.current.mousePosition);
                if (mouseHover)
                {
                    RoundRect(tweenRect, Color.white.SetAlpha(0.9f), borderRadius: 4, borderWidth: 1);
                }
            }

            var colorLine = new Rect(tweenRect.x + 1, tweenRect.y + tweenRect.height - 3, tweenRect.width - 2, 2);
            Random.InitState(animation.Component.GetInstanceID());
            var color = Colors.GetRandom();
            EditorGUI.DrawRect(colorLine, color.SetAlpha(0.6f * alphaMultiplier));

            var label = animation.Label;
            var style = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold, fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white.SetAlpha(alphaMultiplier) }
            };
            GUI.Label(tweenRect, label, style);

            return tweenRect;
        }

        private static float CalculateX(Rect rowRect, float time, float timeScale)
        {
            return rowRect.x + time * timeScale * rowRect.width;
        }

        public static void TimeVerticalLine(Rect rect, float time)
        {
            var verticalLine = new Rect(rect.x + time * rect.width, rect.y, 1, rect.height);
            EditorGUI.DrawRect(verticalLine, Color.white);
        }

        public static void Inspector(UnityEditor.Editor editor)
        {
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft
            };

            EditorGUILayout.Space();

            Splitter(new Color(0.12f, 0.12f, 0.12f, 1.333f));

            var backgroundRect = GUILayoutUtility.GetRect(1f, EditorGUIUtility.singleLineHeight);
            var labelRect = backgroundRect;
            backgroundRect = ToFullWidth(backgroundRect);
            EditorGUI.DrawRect(backgroundRect, new Color(0.1f, 0.1f, 0.1f, 0.2f));
            EditorGUI.LabelField(labelRect, "Inspector", headerStyle);

            Splitter(new Color(0.19f, 0.19f, 0.19f, 1.333f));

            editor.OnInspectorGUI();
        }

        private static void Splitter(Color color)
        {
            var rect = GUILayoutUtility.GetRect(1f, 1f);
            rect = ToFullWidth(rect);
            EditorGUI.DrawRect(rect, color);
        }

        private static Rect ToFullWidth(Rect rect)
        {
            rect.xMin = 0f;
            rect.width += 4f;
            return rect;
        }

        public static bool AddButton(Rect timelineRect)
        {
            var buttonSize = new Vector2(28, 24);
            var position = new Vector2(timelineRect.x + (BOTTOM_HEIGHT - buttonSize.y) / 2, timelineRect.y + timelineRect.height - BOTTOM_HEIGHT + (BOTTOM_HEIGHT - buttonSize.y) / 2);
            var buttonRect = new Rect(position, buttonSize);
            var image = Resources.Load<Texture>("dotween.timeline.add.tween");
            var content = new GUIContent(image) { tooltip = "Add tween" };
            return GUI.Button(buttonRect, content);
        }

        public static bool CallbackButton(Rect timelineRect)
        {
            var buttonSize = new Vector2(22, 24);
            var position = new Vector2(timelineRect.x + (BOTTOM_HEIGHT - buttonSize.y) / 2 + 28 + 2, timelineRect.y + timelineRect.height - BOTTOM_HEIGHT + (BOTTOM_HEIGHT - buttonSize.y) / 2);
            var buttonRect = new Rect(position, buttonSize);
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor.SetAlpha(0.55f);
            var content = EditorGUIUtility.IconContent("d_Animation.AddEvent");
            content.tooltip = "Add callback";
            var result = GUI.Button(buttonRect, content);
            GUI.backgroundColor = backgroundColor;
            return result;
        }

        public static bool RemoveButton(Rect timelineRect)
        {
            var buttonSize = new Vector2(50, 24);
            var position = new Vector2(timelineRect.x + timelineRect.width - buttonSize.x - (BOTTOM_HEIGHT - buttonSize.y) / 2, timelineRect.y + timelineRect.height - BOTTOM_HEIGHT + (BOTTOM_HEIGHT - buttonSize.y) / 2);
            var buttonRect = new Rect(position, buttonSize);

            return GUI.Button(buttonRect, "Delete");
        }

        public static bool DuplicateButton(Rect rect)
        {
            var buttonSize = new Vector2(66, 24);
            var position = new Vector2(rect.x + rect.width - buttonSize.x - (BOTTOM_HEIGHT - buttonSize.y) / 2 - 50 - 2, rect.y + rect.height - BOTTOM_HEIGHT + (BOTTOM_HEIGHT - buttonSize.y) / 2);
            var buttonRect = new Rect(position, buttonSize);

            return GUI.Button(buttonRect, "Duplicate");
        }

        public static bool PlayButton(Rect rect)
        {
            var content = EditorGUIUtility.IconContent("d_PlayButton");
            var position = rect.position + new Vector2(2, (HEADER_HEIGHT - PlayButtonSize.y) / 2);
            var buttonRect = new Rect(position, PlayButtonSize);
            var contentColor = GUI.contentColor;
            GUI.contentColor = Color.cyan;
            var result = GUI.Button(buttonRect, content);
            GUI.contentColor = contentColor;
            return result;
        }

        public static bool StopButton(Rect rect)
        {
            var position = rect.position + new Vector2(2, (HEADER_HEIGHT - PlayButtonSize.y) / 2);
            var buttonRect = new Rect(position, PlayButtonSize);
            return GUI.Button(buttonRect, "■");
        }

        public static bool LoopToggle(Rect rect, bool value)
        {
            var position = rect.position + new Vector2(rect.width - LoopToggleSize.x - 2, (HEADER_HEIGHT - LoopToggleSize.y) / 2);
            var toggleRect = new Rect(position, LoopToggleSize);
            var iconContent = EditorGUIUtility.IconContent("preAudioLoopOff");
            var style = new GUIStyle(GUI.skin.button) { padding = new RectOffset(0, 0, 0, 0) };
            return GUI.Toggle(toggleRect, value, iconContent, style);
        }

        public static bool FreezeFrameToggle(Rect rect, bool value)
        {
            var position = rect.position + new Vector2(rect.width - LoopToggleSize.x - 2 - FreezeToggleSize.x - 2, (HEADER_HEIGHT - FreezeToggleSize.y) / 2);
            var toggleRect = new Rect(position, FreezeToggleSize);
            var style = new GUIStyle(GUI.skin.button) { padding = new RectOffset(0, 0, 0, 0) };
            return GUI.Toggle(toggleRect, value, IconFreezeFrame, style);
        }

        private static void RoundRect(Rect rect, Color color, float borderRadius, float borderWidth = 0)
        {
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false,
                imageAspect: 0, color, borderWidth, borderRadius);
        }

        #region Icons

        private static readonly Texture2D IconFreezeFrame = DottUtils.ImageFromString(ICON_FREEZE_FRAME, 48, 48);
        private static readonly Texture2D IconCallback = DottUtils.ImageFromString(ICON_CALLBACK, 20, 40);

        private const string ICON_FREEZE_FRAME =
            "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAN4SURBVHgB7ZhfSFNRGMDP1nK2zSZG6oyLIwV98KWsXmVQDz5YVG9RT0JMfZHAx8rSXhT7Y4gP4YtMyr3NxCRBmyyKCRpEJoqK0Jjur2PDzdx2+z679z7Y3eTOq012fvBxv51zvnP/nO9833dGCIVCoVAoFEruoiASYVm2AC43QDQphnxTKBTOFLYX4HI5hd0WiA1sw+QwgYe4y6anI43ts31s7xGJqIh0+C//HsQu0v85je0HkIBIex1IA8gpIpFMXoDnEyz3cykGMN4BF8fedvjyeGkgGaAkxxz6Av+bnHwBjPFPQL4Q+XBwc84SCiW3UKXQsx7hYWdmZnrtdvsrsT5JEx0RJ0ASIPGFhYWncA3Mzc1FVCoV63a7m/x+f1lNTc1DbixGyCSRG6hbLoE8BrkiwUwo25eXl2+1t7cre3p6GCw/Y7GYe2dnx4V6X18fg2O8Xm+DmK0swH3uc6XvAwlmytbWViMq29vb0dHRURPqXV1dpTBPAqWtra0U28bGxuoSicRudWc2m9FG3mSb4QugXWhzc/O7y+X6uLS0ZOHbp6am+qenp/v536urq0Pr6+sTkUjkB9j495v3qEqJfCil9TqdzmkwGK5VVlbeBHfRYQf4fxE8aBHqg4ODWqPReLukpORqXl7eLNicgWY1kZMDrEAAjYLB4ARs2GHUw+Fw88jISK/NZnsdCASasA1W6B3sgUnuHvIfLzPdA1ar9Vx9fb3wNQcGBs6Dm8xycyXhZX46HI4yvr+2tvbk+Pi4gezjJQcJoya4sViE+ApL/8+xUq1WswzDCGGxqqoqxt0fNyzO83ttbS2f7y8uLlZubGwoySFEoYwO9egO2OnxeCbBTd6i7vP5mtF9UEKhUDPnQsOQDya4uYJEbmDSAvz3gHMlMbkoYrb7ZePx+BvuwWLgUrubGLLwMEQhK+rd3d1a6NtKJpMsjOUjVdpNLO/ypAEezAs+7wNf/xWNRr0VFRV3sB1DKMR9hclkMuNvCLFDGo3mrFarNej1+nJwx9MkC1A2NjaWo4JJCpMV6pi8+ESGSQ3bMMlBssP9QbjklxWnRmGl+TIBywb0JSwjQNyod3Z2MlhmrKysXBezTTvxESEUaPPz8x2FhYUui8XCtLS0KJxOpweSXFF1dfWjvWOzESF8wyZ+sbi4+FKs7zhwbA80FAqF8pc/YIJhbIXLtTcAAAAASUVORK5CYII=";

        private const string ICON_CALLBACK =
            "iVBORw0KGgoAAAANSUhEUgAAABQAAAAoCAYAAAD+MdrbAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAD4SURBVHgB7ZWxCoJQFIaPkkuOQVtDLQ02tPQGDrn6CvU+tbeH4NTmCzgLRWM0NGhBuCiBBXZO3eISFXppMLgf/ChX7ufPWQ6ApHIo3HsX08ZoUI4zZodZ84c9x3GmWZYleUmSJNnTXVboiUUfckGoCDqGJFKZsKbrehME0TRNBzYqFX6MFEqhFErhnwsvcRxvQZA0TQ/4OPFnHdu2RyJrAIts8O4YXnYK0cKYvu/Pi4hoj3ieN8M7Fty35VvqmD61jaJo+UkWhuGKtRpAwbV7a+u67oQfA9fKxDSgJPRng9oGQbCgsFYGfGmlFBDTGB4zijBHkEgqzhX38zVoGGkfagAAAABJRU5ErkJggg==";

        #endregion
    }
}