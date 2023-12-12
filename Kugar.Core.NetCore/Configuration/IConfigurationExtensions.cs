using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Kugar.Core.Configuration;

public static class IConfigurationExtensions
{
    public static T Bind<T>(this IConfiguration configuration, string? sectionKey = null)
    {
        return (T)Bind(configuration, typeof(T), sectionKey)!;
    }

    private static object? Bind(this IConfiguration configuration, Type type,
        string? sectionKey, bool isNullable = false)
    {
        var keyToUse = sectionKey ;

        var constructors = type.GetConstructors().ToList();

        if (constructors.Count < 1)
        {
            throw new ArgumentException($"Type '{type.Name}' does not have a public constructor", nameof(type));
        }

        var query = constructors.Select(x => new
        {
            Constructor = x,
            Parameters = x.GetParameters().ToList()
        }).OrderByDescending(x => x.Parameters.Count).First();

        if (isNullable && query.Parameters.Count > 0 && !configuration.GetSection(keyToUse).Exists())
        {
            return null;
        }

        var parameters = new List<object>();

        foreach (var parameter in query.Parameters)
        {
            var key = $"{keyToUse}{(!string.IsNullOrWhiteSpace(keyToUse)?":":"")}{parameter.Name}";

            parameters.Add(configuration.GetValue(parameter.ParameterType,key) );
        }

        return query.Constructor.Invoke(parameters.ToArray());
    }
    
}
