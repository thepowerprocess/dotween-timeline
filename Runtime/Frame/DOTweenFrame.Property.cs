using System;
using UnityEngine;

namespace Dott
{
    public partial class DOTweenFrame
    {
        [Serializable]
        public class FrameProperty
        {
            public enum PropertyType
            {
                None,
                Position, LocalPosition,
                Scale,
                Fade
            }

            public GameObject TargetGameObject;
            public Component Target;
            public PropertyType Property;
            public bool IsRelative;

            public Vector3 EndValueVector3;
            public float EndValueFloat;

            public bool OptionalBool;
        }
    }
}