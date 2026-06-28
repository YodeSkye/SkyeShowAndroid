
using Android.Graphics;

namespace SkyeShowAndroid
{
    public static class TextHelpers
    {
        public static string TrimLeftToFit(string text, Label label)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Create Android Paint for measuring text
            var paint = new Android.Graphics.Paint
            {
                TextSize = (float)label.FontSize
            };

            // If the label has a font family, apply it
            if (!string.IsNullOrEmpty(label.FontFamily))
                paint.SetTypeface(Typeface.Create(label.FontFamily, TypefaceStyle.Normal));

            float fullWidth = paint.MeasureText(text);

            if (fullWidth <= label.Width)
                return text;

            string trimmed = text;

            while (trimmed.Length > 0)
            {
                trimmed = trimmed.Substring(1);

                string candidate = "…" + trimmed;
                float width = paint.MeasureText(candidate);

                if (width <= label.Width)
                    return candidate;
            }

            return text;
        }
    }
}
