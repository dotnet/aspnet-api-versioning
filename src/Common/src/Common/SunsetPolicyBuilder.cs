// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Globalization;

/// <summary>
/// Represents the default sunset policy builder.
/// </summary>
public class SunsetPolicyBuilder : ISunsetPolicyBuilder
{
    private SunsetPolicy? sunsetPolicy;
    private DateTimeOffset? date;
    private SunsetLinkBuilder? linkBuilder;
    private Dictionary<Uri, SunsetLinkBuilder>? linkBuilders;

    /// <summary>
    /// Initializes a new instance of the <see cref="SunsetPolicyBuilder"/> class.
    /// </summary>
    /// <param name="name">The name of the API the policy is for.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> the policy is for.</param>
    public SunsetPolicyBuilder( string? name, ApiVersion? apiVersion )
    {
        if ( string.IsNullOrEmpty( name ) && apiVersion == null )
        {
            var message = string.Format( CultureInfo.CurrentCulture, Format.InvalidPolicyKey, nameof( name ), nameof( apiVersion ) );
            throw new System.ArgumentException( message );
        }

        Name = name;
        ApiVersion = apiVersion;
    }

    /// <inheritdoc />
    public string? Name { get; }

    /// <inheritdoc />
    public ApiVersion? ApiVersion { get; }

    /// <inheritdoc />
    public virtual void Per( SunsetPolicy policy ) =>
        sunsetPolicy = policy ?? throw new System.ArgumentNullException( nameof( policy ) );

    /// <inheritdoc />
    public virtual ISunsetPolicyBuilder Effective( DateTimeOffset sunsetDate )
    {
        date = sunsetDate;
        return this;
    }

    /// <inheritdoc />
    public virtual ILinkBuilder Link( Uri linkTarget )
    {
        SunsetLinkBuilder newLinkBuilder;

        if ( linkBuilder == null )
        {
            linkBuilder = newLinkBuilder = new( this, linkTarget );
        }
        else if ( linkBuilder.LinkTarget.Equals( linkTarget ) )
        {
            return linkBuilder;
        }
        else if ( linkBuilders == null )
        {
            linkBuilders = new()
            {
                [linkBuilder.LinkTarget] = linkBuilder,
                [linkTarget] = newLinkBuilder = new( this, linkTarget ),
            };
        }
        else if ( !linkBuilders.TryGetValue( linkTarget, out newLinkBuilder! ) )
        {
            linkBuilders.Add( linkTarget, newLinkBuilder = new( this, linkTarget ) );
        }

        return newLinkBuilder;
    }

    /// <inheritdoc />
    public virtual SunsetPolicy Build()
    {
        if ( sunsetPolicy is not null )
        {
            return sunsetPolicy;
        }

        SunsetPolicy policy = date is null ? new() : new( date.Value );

        if ( linkBuilders == null )
        {
            if ( linkBuilder != null )
            {
                policy.Links.Add( linkBuilder.Build() );
            }
        }
        else
        {
            foreach ( var builder in linkBuilders.Values )
            {
                policy.Links.Add( builder.Build() );
            }
        }

        return policy;
    }
}