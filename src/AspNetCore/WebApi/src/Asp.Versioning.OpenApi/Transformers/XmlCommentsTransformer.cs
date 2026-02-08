// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Transformers;

using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using System.IO;
using System.Reflection;
using System.Threading;
using static System.Reflection.BindingFlags;

/// <summary>
/// Represents a <see cref="IOpenApiDocumentTransformer">transformer</see> used to apply XML comments to an
/// OpenAPI document.
/// </summary>
[CLSCompliant( false )]
public class XmlCommentsTransformer : IOpenApiSchemaTransformer, IOpenApiOperationTransformer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlCommentsTransformer"/> class.
    /// </summary>
    /// <param name="environment">The current <see cref="IHostEnvironment">hosting environment</see>.</param>
    public XmlCommentsTransformer( IHostEnvironment environment ) :
        this( ResolvePath( environment ) )
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlCommentsTransformer"/> class.
    /// </summary>
    /// <param name="path">The file path of the XML commands file.</param>
    public XmlCommentsTransformer( string path ) => Documentation = XmlDocumentation.FromFile( path );

    internal bool IsEmpty => Documentation.IsEmpty;

    /// <summary>
    /// Gets the documentation associated with the transformer.
    /// </summary>
    protected XmlDocumentation Documentation { get; }

    /// <inheritdoc />
    public virtual Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken )
    {
        ArgumentNullException.ThrowIfNull( schema );
        ArgumentNullException.ThrowIfNull( context );

        if ( schema.Properties is not { } properties
            || context.JsonTypeInfo?.Type is not Type type )
        {
            return Task.CompletedTask;
        }

        if ( string.IsNullOrEmpty( schema.Description ) )
        {
            schema.Description = Documentation.GetSummary( type );
        }

        foreach ( var (name, prop) in properties )
        {
            if ( prop is not null
                && string.IsNullOrEmpty( prop.Description )
                && type.GetProperty( name, IgnoreCase | Instance | Public ) is { } property )
            {
                prop.Description = Documentation.GetSummary( property );
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken )
    {
        ArgumentNullException.ThrowIfNull( operation );
        ArgumentNullException.ThrowIfNull( context );

        if ( context.Description.ActionDescriptor is not ControllerActionDescriptor controller )
        {
            return Task.CompletedTask;
        }

        var method = controller.MethodInfo;

        if ( string.IsNullOrEmpty( operation.Summary ) )
        {
            operation.Summary = Documentation.GetSummary( method );
        }

        if ( string.IsNullOrEmpty( operation.Description ) )
        {
            operation.Description = Documentation.GetRemarks( method );
        }

        if ( operation.Responses is { } responses )
        {
            foreach ( var (statusCode, response) in responses )
            {
                var description = Documentation.GetResponseDescription( method, statusCode );

                if ( !string.IsNullOrEmpty( description ) )
                {
                    response.Description = description;
                }
            }
        }

        var parameters = operation.Parameters;
        var args = context.Description.ParameterDescriptions;

        if ( parameters is null || parameters.Count == 0 || args.Count == 0 )
        {
            return Task.CompletedTask;
        }

        for ( var i = 0; i < parameters.Count; i++ )
        {
            var parameter = parameters[i];

            if ( !string.IsNullOrEmpty( parameter.Name ) && string.IsNullOrEmpty( parameter.Description ) )
            {
                for ( var j = 0; j < args.Count; j++ )
                {
                    var arg = args[i];

                    if ( arg.Name == parameter.Name )
                    {
                        var name = arg.ParameterDescriptor.Name;
                        parameter.Description = Documentation.GetParameterDescription( method, name );
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    private static string ResolvePath( IHostEnvironment environment )
    {
        ArgumentNullException.ThrowIfNull( environment );

        if ( Assembly.GetEntryAssembly() is { } assembly )
        {
            var fileName = $"{assembly.GetName().Name}.xml";
            var paths = new[] { environment.ContentRootPath, AppContext.BaseDirectory };

            for ( var i = 0; i < paths.Length; i++ )
            {
                var path = Path.Combine( paths[i], fileName );

                if ( File.Exists( path ) )
                {
                    return path;
                }
            }
        }

        return string.Empty;
    }
}