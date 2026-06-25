using System.Globalization;
using System.Text;

namespace Mutators.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// This method is intended to be used to turn mutator names into deterministic slugs.
        /// An attempt will be made to normalize the characters, removing accents etc.
        /// If the input contains non-ASCII letters or digits that cannot be normalized into a readable slug,
        /// a UTF-8 hex encoded slug will be returned instead, starting with the prefix <c>hex-</c>.
        /// </summary>
        /// <param name="input">The string to convert into a slug.</param>
        /// <param name="namespace">The namespace used as a prefix.</param>
        /// <returns>A lowercase slug containing ASCII letters, digits, and hyphens, or a deterministic hex fallback slug.</returns>
        public static string ToSlug(this string input, string @namespace)
        {
            if (string.IsNullOrEmpty(input))
                return @namespace;
            
            return @namespace.ToLowerInvariant() + ":" + (TryCreateReadableSlug(input, out string slug)
                ? slug
                : CreateEncodedSlug(input));
        }

        private static bool TryCreateReadableSlug(string input, out string slug)
        {
            string normalized = input.Normalize(NormalizationForm.FormD);
            StringBuilder builder = new(input.Length);

            foreach (char character in normalized)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);
                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (IsAsciiLetter(character))
                {
                    builder.Append(char.ToLowerInvariant(character));
                    continue;
                }

                if (IsAsciiDigit(character))
                {
                    builder.Append(character);
                    continue;
                }

                if (char.IsLetterOrDigit(character))
                {
                    slug = string.Empty;
                    return false;
                }

                if (builder.Length > 0 && builder[^1] != '-')
                {
                    builder.Append('-');
                }
            }

            if (builder.Length > 0 && builder[^1] == '-')
            {
                builder.Length--;
            }

            slug = builder.ToString();
            return slug.Length > 0;
        }

        // In case we can't make a nice slug we will just use the hexed bytes as a fallback
        // This should be deterministic across all clients
        private static string CreateEncodedSlug(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            StringBuilder builder = new(bytes.Length * 2 + 2);

            builder.Append("hex-");
            foreach (byte value in bytes)
            {
                builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static bool IsAsciiLetter(char character)
        {
            return character is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
        }

        private static bool IsAsciiDigit(char character)
        {
            return character is >= '0' and <= '9';
        }

    }
}
