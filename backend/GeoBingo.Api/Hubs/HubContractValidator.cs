using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace GeoBingo.Api.Hubs;

internal sealed class HubContractValidator
{
    public IReadOnlyDictionary<string, string[]> Validate(
        IReadOnlyList<object?> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        var errors = new SortedDictionary<
            string,
            SortedSet<string>>(StringComparer.Ordinal);
        var visited = new HashSet<object>(
            ReferenceEqualityComparer.Instance);

        foreach (var argument in arguments)
        {
            ValidateValue(
                argument,
                path: string.Empty,
                visited,
                errors);
        }

        return errors.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.ToArray(),
            StringComparer.Ordinal);
    }

    private static void ValidateValue(
        object? value,
        string path,
        ISet<object> visited,
        IDictionary<string, SortedSet<string>> errors)
    {
        if (value is null || IsLeaf(value.GetType()))
        {
            return;
        }

        if (!value.GetType().IsValueType
            && !visited.Add(value))
        {
            return;
        }

        var validationResults = new List<ValidationResult>();
        _ = Validator.TryValidateObject(
            value,
            new ValidationContext(value),
            validationResults,
            validateAllProperties: true);
        foreach (var result in validationResults)
        {
            var memberNames = result.MemberNames.Any()
                ? result.MemberNames
                : [string.Empty];
            foreach (var memberName in memberNames)
            {
                AddError(
                    errors,
                    Combine(
                        path,
                        NormalizeMemberName(
                            value.GetType(),
                            memberName)),
                    result.ErrorMessage
                    ?? "The supplied value is invalid.");
            }
        }

        if (value is IEnumerable enumerable
            && value is not string)
        {
            var index = 0;
            foreach (var item in enumerable)
            {
                ValidateValue(
                    item,
                    $"{path}[{index}]",
                    visited,
                    errors);
                index++;
            }

            return;
        }

        foreach (var property in value.GetType()
                     .GetProperties(
                         BindingFlags.Instance
                         | BindingFlags.Public)
                     .Where(property =>
                         property.CanRead
                         && property.GetIndexParameters().Length == 0))
        {
            var propertyName =
                property.GetCustomAttribute<
                        JsonPropertyNameAttribute>()
                    ?.Name
                ?? ToCamelCase(property.Name);
            ValidateValue(
                property.GetValue(value),
                Combine(path, propertyName),
                visited,
                errors);
        }
    }

    private static string NormalizeMemberName(
        Type declaringType,
        string memberName)
    {
        if (string.IsNullOrWhiteSpace(memberName))
        {
            return string.Empty;
        }

        var segments = memberName.Split('.');
        for (var index = 0; index < segments.Length; index++)
        {
            var property = declaringType.GetProperty(
                segments[index],
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.IgnoreCase);
            segments[index] =
                property?.GetCustomAttribute<
                        JsonPropertyNameAttribute>()
                    ?.Name
                ?? ToCamelCase(segments[index]);
            declaringType = property?.PropertyType
                ?? declaringType;
        }

        return string.Join('.', segments);
    }

    private static string Combine(
        string prefix,
        string member) =>
        string.IsNullOrEmpty(prefix)
            ? member
            : string.IsNullOrEmpty(member)
                ? prefix
                : $"{prefix}.{member}";

    private static void AddError(
        IDictionary<string, SortedSet<string>> errors,
        string path,
        string message)
    {
        if (!errors.TryGetValue(path, out var messages))
        {
            messages = new SortedSet<string>(
                StringComparer.Ordinal);
            errors.Add(path, messages);
        }

        messages.Add(message);
    }

    private static string ToCamelCase(string value) =>
        string.IsNullOrEmpty(value)
            ? value
            : char.ToLowerInvariant(value[0])
              + value[1..];

    private static bool IsLeaf(Type type) =>
        type.IsPrimitive
        || type.IsEnum
        || type == typeof(string)
        || type == typeof(decimal)
        || type == typeof(Guid)
        || type == typeof(DateTime)
        || type == typeof(DateTimeOffset)
        || type == typeof(TimeSpan)
        || Nullable.GetUnderlyingType(type) is Type inner
            && IsLeaf(inner);

    private sealed class ReferenceEqualityComparer
        : IEqualityComparer<object>
    {
        public static ReferenceEqualityComparer Instance { get; } =
            new();

        public new bool Equals(object? x, object? y) =>
            ReferenceEquals(x, y);

        public int GetHashCode(object obj) =>
            RuntimeHelpers.GetHashCode(obj);
    }
}
