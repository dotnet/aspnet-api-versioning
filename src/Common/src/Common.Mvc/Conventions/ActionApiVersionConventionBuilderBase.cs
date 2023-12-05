// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using static Asp.Versioning.ApiVersionProviderOptions;

/// <summary>
/// Represents the base implementation of a builder for API versions applied to a controller action.
/// </summary>
public abstract partial class ActionApiVersionConventionBuilderBase : ApiVersionConventionBuilderBase
{
    private HashSet<ApiVersion>? mapped;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionApiVersionConventionBuilderBase"/> class.
    /// </summary>
    protected ActionApiVersionConventionBuilderBase() => NamingConvention = ControllerNameConvention.Default;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionApiVersionConventionBuilderBase"/> class.
    /// </summary>
    /// <param name="namingConvention">The <see cref="IControllerNameConvention">controller naming convention</see>.</param>
    protected ActionApiVersionConventionBuilderBase( IControllerNameConvention namingConvention ) => NamingConvention = namingConvention;

    /// <inheritdoc />
    protected override bool IsEmpty => ( mapped is null || mapped.Count == 0 ) && base.IsEmpty;

    /// <summary>
    /// Gets the collection of API versions mapped to the current action.
    /// </summary>
    /// <value>A <see cref="ICollection{T}">collection</see> of mapped <see cref="ApiVersion">API versions</see>.</value>
    protected ICollection<ApiVersion> MappedVersions => mapped ??= [];

    /// <summary>
    /// Gets the controller naming convention associated with the builder.
    /// </summary>
    /// <value>The <see cref="IControllerNameConvention">controller naming convention</see>.</value>
    public IControllerNameConvention NamingConvention { get; }

    /// <inheritdoc />
    protected override void MergeAttributesWithConventions( IReadOnlyList<object> attributes )
    {
        ArgumentNullException.ThrowIfNull( attributes );

        base.MergeAttributesWithConventions( attributes );

        for ( var i = 0; i < attributes.Count; i++ )
        {
            if ( attributes[i] is IApiVersionProvider provider && provider.Options == Mapped )
            {
                MappedVersions.UnionWith( provider.Versions );
            }
        }
    }
}