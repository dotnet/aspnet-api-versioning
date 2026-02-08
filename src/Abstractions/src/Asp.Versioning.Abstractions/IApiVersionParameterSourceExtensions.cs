// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using static ApiVersionParameterLocation;

/// <summary>
/// Provides extension methods for the <see cref="IApiVersionParameterSource"/> interface.
/// </summary>
public static class IApiVersionParameterSourceExtensions
{
    /// <param name="source">The extended parameter source.</param>
    extension( IApiVersionParameterSource source )
    {
        /// <summary>
        /// Determines whether the specified parameter source versions by query string.
        /// </summary>
        /// <param name="allowMultipleLocations">True if multiple API version locations are allowed.
        /// False if the API version can only appear in a query string parameter. The default value is true.</param>
        /// <returns>True if the parameter source versions by query string; otherwise, false.</returns>
        public bool VersionsByQueryString( bool allowMultipleLocations = true )
        {
            ArgumentNullException.ThrowIfNull( source );

            var context = new DescriptionContext( Query );

            source.AddParameters( context );

            return context.IsMatch && ( allowMultipleLocations || context.Locations == 1 );
        }

        /// <summary>
        /// Determines whether the specified parameter source versions by HTTP header.
        /// </summary>
        /// <param name="allowMultipleLocations">True if multiple API version locations are allowed.
        /// False if the API version can only appear in a HTTP header. The default value is true.</param>
        /// <returns>True if the parameter source versions by HTTP header; otherwise, false.</returns>
        public bool VersionsByHeader( bool allowMultipleLocations = true )
        {
            ArgumentNullException.ThrowIfNull( source );

            var context = new DescriptionContext( Header );

            source.AddParameters( context );

            return context.IsMatch && ( allowMultipleLocations || context.Locations == 1 );
        }

        /// <summary>
        /// Determines whether the specified parameter source versions by URL path segment.
        /// </summary>
        /// <param name="allowMultipleLocations">True if multiple API version locations are allowed.
        /// False if the API version can only appear in a URL path segment. The default value is true.</param>
        /// <returns>True if the parameter source versions by URL path segment; otherwise, false.</returns>
        public bool VersionsByUrl( bool allowMultipleLocations = true )
        {
            ArgumentNullException.ThrowIfNull( source );

            var context = new DescriptionContext( Path );

            source.AddParameters( context );

            return context.IsMatch && ( allowMultipleLocations || context.Locations == 1 );
        }

        /// <summary>
        /// Determines whether the specified parameter source versions by media type.
        /// </summary>
        /// <param name="allowMultipleLocations">True if multiple API version locations are allowed.
        /// False if the API version can only appear as a media type. The default value is true.</param>
        /// <returns>True if the parameter source versions by media type; otherwise, false.</returns>
        public bool VersionsByMediaType( bool allowMultipleLocations = true )
        {
            ArgumentNullException.ThrowIfNull( source );

            var context = new DescriptionContext( MediaTypeParameter );

            source.AddParameters( context );

            return context.IsMatch && ( allowMultipleLocations || context.Locations == 1 );
        }

        /// <summary>
        /// Gets the name of the parameter associated with the parameter source, if any.
        /// </summary>
        /// <param name="location">The location to get the parameter name for.</param>
        /// <returns>The name of the first parameter defined by the parameter source for the specified
        /// <paramref name="location"/> or <c>null</c>.</returns>
        public string GetParameterName( ApiVersionParameterLocation location )
        {
            ArgumentNullException.ThrowIfNull( source );

            var context = new DescriptionContext( location );

            source.AddParameters( context );

            return context.ParameterName;
        }

        /// <summary>
        /// Gets the name of the parameters associated with the parameter source.
        /// </summary>
        /// <param name="location">The location to get the parameter names for.</param>
        /// <returns>The names of the parameters defined by the parameter source for the specified <paramref name="location"/>.</returns>
        public IReadOnlyList<string> GetParameterNames( ApiVersionParameterLocation location )
        {
            ArgumentNullException.ThrowIfNull( source );

            var context = new DescriptionContext( location );

            source.AddParameters( context );

            return context.ParameterNames;
        }
    }

    private sealed class DescriptionContext : IApiVersionParameterDescriptionContext
    {
        private readonly ApiVersionParameterLocation expectedLocation;
        private List<string>? parameterNames;
        private int matches;

        internal DescriptionContext( ApiVersionParameterLocation expectedLocation ) => this.expectedLocation = expectedLocation;

        internal bool IsMatch { get; private set; }

        internal int Locations { get; private set; }

        internal string ParameterName { get; private set; } = string.Empty;

        internal IReadOnlyList<string> ParameterNames
        {
            get
            {
                if ( parameterNames == null )
                {
                    return [];
                }

                return parameterNames;
            }
        }

        public void AddParameter( string name, ApiVersionParameterLocation location )
        {
            Locations++;

            var match = expectedLocation == location;

            IsMatch |= match;

            if ( !match )
            {
                return;
            }

            if ( ++matches == 1 )
            {
                ParameterName = name;
            }
            else if ( !string.IsNullOrEmpty( name ) )
            {
                if ( parameterNames == null )
                {
                    parameterNames = new( capacity: 4 );

                    var first = ParameterName;

                    if ( !string.IsNullOrEmpty( first ) )
                    {
                        parameterNames.Add( first! );
                    }
                }

                if ( !parameterNames.Contains( name, StringComparer.OrdinalIgnoreCase ) )
                {
                    parameterNames.Add( name );
                }
            }
        }
    }
}