﻿using System.Globalization;

namespace BricksHoarder.Helpers;

public static class BoolExtensions
{
    public static bool IsFalse(this bool source)
    {
        return source == false;
    }
}

public static class StringExtensions
{
    public static T To<T>(this string source)
    {
        return (T)Convert.ChangeType(source, typeof(T), CultureInfo.InvariantCulture);
    }

    public static T? ToEnum<T>(this string source) where T : struct
    {
        if (Enum.TryParse(source, out T value))
        {
            return value;
        }

        return null;
    }

    public static T? ToN<T>(this string? source, CultureInfo cultureInfo) where T : struct
    {
        if (!string.IsNullOrWhiteSpace(source))
        {
            return (T)Convert.ChangeType(source, typeof(T), cultureInfo);
        }

        return null;
    }

    public static Guid? ToGuid(this string? source)
    {
        if (Guid.TryParse(source, out Guid value))
        {
            return value;
        }

        return null;
    }

    public static bool IsNullOrWhiteSpace(this string source)
    {
        return string.IsNullOrWhiteSpace(source);
    }
}