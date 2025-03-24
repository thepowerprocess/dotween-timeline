using System;
using UnityEngine;

namespace Dott
{
    public static class DottUtils
    {
        public static Texture2D ImageFromString(string source, int width, int height)
        {
            var bytes = Convert.FromBase64String(source);
            var texture = new Texture2D(width, height);
            texture.LoadImage(bytes);
            return texture;
        }
    }
}