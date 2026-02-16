// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Represents the default sunset policy builder.
/// </summary>
public class SunsetPolicyBuilder : PolicyBuilder<SunsetPolicy>, ISunsetPolicyBuilder
{
    private DateTimeOffset? date;
    private SunsetLinkBuilder? linkBuilder;
    private Dictionary<Uri, SunsetLinkBuilder>? linkBuilders;

    /// <summary>
    /// Initializes a new instance of the <see cref="SunsetPolicyBuilder"/> class.
    /// </summary>
    /// <param name="name">The name of the API the policy is for.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> the policy is for.</param>
    public SunsetPolicyBuilder( string? name, ApiVersion? apiVersion )
        : base( name, apiVersion ) { }

    /// <inheritdoc />
    public virtual void SetEffectiveDate( DateTimeOffset effectiveDate ) => date = effectiveDate;

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
    public override SunsetPolicy Build()
    {
        if ( Policy is not null )
        {
            return Policy;
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

    private sealed class SunsetLinkBuilder( SunsetPolicyBuilder policyBuilder, Uri linkTarget ) :
        LinkBuilder( linkTarget, "sunset" ), ILinkBuilder
    {
        public override ILinkBuilder Link( Uri linkTarget ) => policyBuilder.Link( linkTarget );
    }
}