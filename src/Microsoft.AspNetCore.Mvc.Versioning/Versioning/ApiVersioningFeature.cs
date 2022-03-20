namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents the API versioning feature.
    /// </summary>
    [CLSCompliant( false )]
    public sealed class ApiVersioningFeature : IApiVersioningFeature
    {
        readonly HttpContext context;
        IReadOnlyList<string>? rawApiVersions;
        ApiVersion? apiVersion;
        ActionSelectionResult? selectionResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersioningFeature"/> class.
        /// </summary>
        /// <param name="context">The current <see cref="HttpContext">HTTP context</see>.</param>
        [CLSCompliant( false )]
        public ApiVersioningFeature( HttpContext context ) => this.context = context;

        /// <inheritdoc />
        public string? RouteParameter { get; set; }

        /// <inheritdoc />
        public IReadOnlyList<string> RawRequestedApiVersions
        {
            get => rawApiVersions ??= ReadApiVersions();
            set => rawApiVersions = value;
        }

        /// <inheritdoc />
        public string? RawRequestedApiVersion
        {
            get
            {
                var values = RawRequestedApiVersions;

                return values.Count switch
                {
                    0 => default,
                    1 => values[0],
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations; existing behavior via IApiVersionReader.Read
                    _ => throw new AmbiguousApiVersionException(
                            SR.MultipleDifferentApiVersionsRequested.FormatDefault( string.Join( ", ", values ) ),
                            values ),
#pragma warning restore CA1065
                };
            }
            set
            {
                if ( string.IsNullOrEmpty( value ) )
                {
                    rawApiVersions = default;
                }
                else
                {
                    rawApiVersions = new[] { value };
                }
            }
        }

        /// <inheritdoc />
        public ApiVersion? RequestedApiVersion
        {
            get
            {
                if ( apiVersion is not null )
                {
                    return apiVersion;
                }

                var value = RawRequestedApiVersion;

                if ( string.IsNullOrEmpty( value ) )
                {
                    return apiVersion;
                }

                try
                {
                    apiVersion = ApiVersion.Parse( value );
                }
                catch ( FormatException )
                {
                    apiVersion = default;
                }

                return apiVersion;
            }
            set
            {
                apiVersion = value;

                if ( apiVersion is not null && ( rawApiVersions is null || rawApiVersions.Count == 0 ) )
                {
                    rawApiVersions = new[] { apiVersion.ToString() };
                }
            }
        }

        /// <inheritdoc />
        public ActionSelectionResult SelectionResult => selectionResult ??= new();

        string[] ReadApiVersions()
        {
            var reader =
                context.RequestServices.GetService<IApiVersionReader>() ??
                ApiVersionReader.Combine(
                    new QueryStringApiVersionReader(),
                    new UrlSegmentApiVersionReader() );
            HashSet<string>? values;
            string? value;

            switch ( reader )
            {
                case IReadOnlyList<IApiVersionReader> readers:
                    values = new( capacity: readers.Count, StringComparer.OrdinalIgnoreCase );

                    for ( var i = 0; i < readers.Count; i++ )
                    {
                        value = readers[i].Read( context.Request );

                        if ( value is not null )
                        {
                            values.Add( value );
                        }
                    }

                    return values.ToArray();
                case IEnumerable<IApiVersionReader> readers:
                    values = new( StringComparer.OrdinalIgnoreCase );

                    foreach ( var innerReader in readers )
                    {
                        value = innerReader.Read( context.Request );

                        if ( value is not null )
                        {
                            values.Add( value );
                        }
                    }

                    return values.ToArray();
            }

            value = reader.Read( context.Request );

            return value is null ? Array.Empty<string>() : new[] { value };
        }
    }
}