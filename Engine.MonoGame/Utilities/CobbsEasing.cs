using System;

using Microsoft.Xna.Framework;

namespace Engine.MonoGame.Utilities
{
    public static class CobbsEasing
    {
        public static float Linear(float t) => t;

        public static float InSine(float t) => 1f - MathF.Cos(t * MathF.PI / 2f);

        public static float OutSine(float t) => MathF.Sin(t * MathF.PI / 2f);

        public static float InOutSine(float t) => -(MathF.Cos(MathF.PI * t) - 1f) / 2f;

        public static float InQuad(float t) => t * t;

        public static float OutQuad(float t) => 1f - (1f - t) * (1f - t);

        public static float InOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f;

        public static float InCubic(float t) => t * t * t;

        public static float OutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);

        public static float InOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f;

        public static float InQuart(float t) => t * t * t * t;

        public static float OutQuart(float t) => 1f - MathF.Pow(1f - t, 4f);

        public static float InOutQuart(float t) => t < 0.5f ? 8f * t * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 4f) / 2f;

        public static float InQuint(float t) => t * t * t * t * t;

        public static float OutQuint(float t) => 1f - MathF.Pow(1f - t, 5f);

        public static float InOutQuint(float t) => t < 0.5f ? 16f * t * t * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 5f) / 2f;

        public static float InExpo(float t) => t == 0f ? 0f : MathF.Pow(2f, 10f * t - 10f);

        public static float OutExpo(float t) => t == 1f ? 1f : 1f - MathF.Pow(2f, -10f * t);

        public static float InOutExpo(float t) => t == 0f ? 0f : t == 1f ? 1f : 
            t < 0.5f
                ? MathF.Pow(2f, 20f * t - 10f) / 2f
                : (2f - MathF.Pow(2f, -20f * t + 10f)) / 2f;

        public static float InCirc(float t) => 1f - MathF.Sqrt(1f - t * t);

        public static float OutCirc(float t) => MathF.Sqrt(1f - (t - 1f) * (t - 1f));

        public static float InOutCirc(float t) => t < 0.5f ? (1f - MathF.Sqrt(1f - 4f * t * t)) / 2f : (MathF.Sqrt(1f - MathF.Pow(-2f * t + 2f, 2f)) + 1f) / 2f;

        private const float BackC1 = 1.70158f;
        private const float BackC3 = BackC1 + 1f;

        public static float InBack(float t) => BackC3 * t * t * t - BackC1 * t * t;

        public static float OutBack(float t) => 1f + BackC3 * MathF.Pow(t - 1f, 3f) + BackC1 * MathF.Pow(t - 1f, 2f);

        public static float InOutBack(float t)
        {
            const float c2 = BackC1 * 1.525f;

            return t < 0.5f
                ? MathF.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2) / 2f
                : (MathF.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (2f * t - 2f) + c2) + 2f) / 2f;
        }

        public static float OutBounce(float t)
        {
            const float n = 7.5625f;
            const float d = 2.75f;

            if (t < 1f / d)
            {
                return n * t * t;
            }

            if (t < 2f / d)
            {
                t -= 1.5f / d;
                return n * t * t + 0.75f;
            }

            if (t < 2.5f / d)
            {
                t -= 2.25f / d;
                return n * t * t + 0.9375f;
            }

            t -= 2.625f / d;
            return n * t * t + 0.984375f;
        }

        public static float InBounce(float t) => 1f - OutBounce(1f - t);

        public static float InOutBounce(float t) => t < 0.5f
                ? (1f - OutBounce(1f - 2f * t)) / 2f
                : (1f + OutBounce(2f * t - 1f)) / 2f;

        public static float OutElastic(float t)
        {
            if (t == 0f || t == 1f)
            {
                return t;
            }

            const float c4 = 2f * MathF.PI / 3f;

            return MathF.Pow(2f, -10f * t) * MathF.Sin((t * 10f - 0.75f) * c4) + 1f;
        }

        public static float InElastic(float t) => 1f - OutElastic(1f - t);

        public static float InOutElastic(float t)
        {
            if (t == 0f || t == 1f)
            {
                return t;
            }

            const float c5 = 2f * MathF.PI / 4.5f;

            return t < 0.5f
                ? -(MathF.Pow(2f, 20f * t - 10f) * MathF.Sin((20f * t - 11.125f) * c5)) / 2f
                : MathF.Pow(2f, -20f * t + 10f) * MathF.Sin((20f * t - 11.125f) * c5) / 2f + 1f;
        }

        public static float Lerp(float start, float end, float t) => MathHelper.Lerp(start, end, t);

        public static Vector2 Lerp(Vector2 start, Vector2 end, float t) => Vector2.Lerp(start, end, t);

        public static Vector3 Lerp(Vector3 start, Vector3 end, float t) => Vector3.Lerp(start, end, t);

        public static Vector2 Ease(Vector2 start, Vector2 end, float t, Func<float, float> easing) => Vector2.Lerp(start, end, easing(t));

        public static Vector3 Ease(Vector3 start, Vector3 end, float t, Func<float, float> easing) => Vector3.Lerp(start, end, easing(t));
    }
}
