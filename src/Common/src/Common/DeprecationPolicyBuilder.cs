// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Represents the default deprecation policy builder.
/// </summary>
public class DeprecationPolicyBuilder : PolicyBuilder<DeprecationPolicy>, IDeprecationPolicyBuilder
{
    private DateTimeOffset? date;
    private DeprecationLinkBuilder? linkBuilder;
    private Dictionary<Uri, DeprecationLinkBuilder>? linkBuilders;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeprecationPolicyBuilder"/> class.
    /// </summary>
    /// <param name="name">The name of the API the policy is for.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> the policy is for.</param>
    public DeprecationPolicyBuilder( string? name, ApiVersion? apiVersion )
        : base( name, apiVersion ) { }

    /// <inheritdoc />
    public virtual void SetEffectiveDate( DateTimeOffset effectiveDate ) => date = effectiveDate;

    /// <inheritdoc />
    public virtual ILinkBuilder Link( Uri linkTarget )
    {
        DeprecationLinkBuilder newLinkBuilder;

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
    public override DeprecationPolicy Build()
    {
        if ( Policy is not null )
        {
            return Policy;
        }

        DeprecationPolicy policy = date is null ? new() : new( date.Value );

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

    private sealed class DeprecationLinkBuilder( DeprecationPolicyBuilder policyBuilder, Uri linkTarget ) :
        LinkBuilder( linkTarget, "deprecation" ), ILinkBuilder
    {
        public override ILinkBuilder Link( Uri linkTarget ) => policyBuilder.Link( linkTarget );
    }
}