using System.Runtime.CompilerServices;

namespace Engine.Core.Exceptions
{
    public class ArgumentEmptyException : ArgumentException
    {
        public ArgumentEmptyException(string? message) : base(message)
        {
        }

        public ArgumentEmptyException(string? message, string? paramName) : base(message, paramName)
        {
        }

        /// <summary>Throws a <see cref="ArgumentEmptyException"/> when the collection is null or empty</summary>
        /// <typeparam name="T">The element type</typeparam>
        /// <param name="value">The collection to validate</param>
        /// <param name="paramName">The name of the argument being validated</param>
        /// <exception cref="ArgumentEmptyException"/>
        public static void ThrowIfNullOrEmpty<T>(IEnumerable<T>? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value is null || !value.Any())
            {
                throw new ArgumentEmptyException($"Argument '{paramName}' cannot be null or empty.", paramName);
            }
        }

        /// <summary>Throws a <seealso cref="ArgumentEmptyException"/> when the collection is empty</summary>
        /// <typeparam name="T">The element type</typeparam>
        /// <param name="value">The collection to validate</param>
        /// <param name="paramName">The name of the argument being validated</param>
        /// <exception cref="ArgumentEmptyException"/>
        public static void ThrowIfEmpty<T>(IEnumerable<T>? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (!value?.Any() ?? false)
            {
                throw new ArgumentEmptyException($"Argument '{paramName}' cannot be empty.", paramName);
            }
        }
    }
}
