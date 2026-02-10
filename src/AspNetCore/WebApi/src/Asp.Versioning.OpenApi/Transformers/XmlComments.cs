// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Transformers;

using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

/// <summary>
/// Provides access to XML documentation comments, which enables the retrieval of summaries, remarks, return values,
/// examples, and parameter descriptions.
/// </summary>
public class XmlComments
{
    private readonly ConcurrentDictionary<string, XElement?> members = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlComments"/> class.
    /// </summary>
    /// <param name="path">The file path of the XML comments to read.</param>
    protected XmlComments( string path )
    {
        if ( File.Exists( path ) )
        {
            Xml = XDocument.Load( path );
        }
        else
        {
            Xml = new XDocument();
        }
    }

    /// <summary>
    /// Creates and returns new <see cref="XmlComments"/> from the specified file.
    /// </summary>
    /// <param name="path">The file path of the XML comments to read.</param>
    /// <returns>New <see cref="XmlComments"/>.</returns>
    public static XmlComments FromFile( string path ) => new( path );

    internal bool IsEmpty => Xml.Root is null;

    /// <summary>
    /// Gets the underlying <see cref="XDocument">XML document</see>.
    /// </summary>
    /// <value>The <see cref="XDocument"/> for the source XML comments file.</value>
    protected XDocument Xml { get; }

    /// <summary>
    /// Gets the <c>summary</c> from the specified member, if any.
    /// </summary>
    /// <param name="member">The member to get the summary from.</param>
    /// <returns>The corresponding summary or an empty string.</returns>
    public string GetSummary( MemberInfo member )
        => GetMember( member )?.Element( "summary" )?.Value.Trim() ?? string.Empty;

    /// <summary>
    /// Gets the <c>description</c> from the specified member, if any.
    /// </summary>
    /// <param name="member">The member to get the description from.</param>
    /// <returns>The corresponding description or an empty string.</returns>
    public string GetDescription( MemberInfo member )
        => GetMember( member )?.Element( "description" )?.Value.Trim() ?? string.Empty;

    /// <summary>
    /// Gets the <c>remarks</c> from the specified member, if any.
    /// </summary>
    /// <param name="member">The member to get the remarks from.</param>
    /// <returns>The corresponding remarks or an empty string.</returns>
    public string GetRemarks( MemberInfo member )
        => GetMember( member )?.Element( "remarks" )?.Value.Trim() ?? string.Empty;

    /// <summary>
    /// Gets the <c>returns</c> from the specified member, if any.
    /// </summary>
    /// <param name="member">The member to get the returns from.</param>
    /// <returns>The corresponding returns or an empty string.</returns>
    public string GetReturns( MemberInfo member )
        => GetMember( member )?.Element( "returns" )?.Value.Trim() ?? string.Empty;

    /// <summary>
    /// Gets the <c>param</c> description from the specified member, if any.
    /// </summary>
    /// <param name="member">The member to get the parameter from.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <returns>The corresponding returns or an empty string.</returns>
    public string GetParameterDescription( MemberInfo member, string name )
    {
        if ( GetMember( member ) is { } element )
        {
            return element.Elements( "param" )
                          .FirstOrDefault( x => x.Attribute( "name" )?.Value == name )?
                          .Value
                          .Trim() ?? string.Empty;
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets the <c>response</c> description from the specified member, if any.
    /// </summary>
    /// <param name="member">The member to get the parameter from.</param>
    /// <param name="statusCode">The status code to get the description for.</param>
    /// <returns>The corresponding response description or an empty string.</returns>
    /// <remarks>This method is based on the custom extension element that was introduced and popularized by
    /// Swashbuckle; for example, <c>&lt;response code="200"&gt;The operation was successful&lt;/response&gt;</c>. See the
    /// <a href="https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle">tutorial</a>
    /// for more information.</remarks>
    public string GetResponseDescription( MemberInfo member, int statusCode )
        => GetResponseDescription( member, statusCode.ToString( CultureInfo.InvariantCulture ) );

    /// <summary>
    /// Gets the <c>response</c> description from the specified member, if any.
    /// </summary>
    /// <param name="member">The member to get the parameter from.</param>
    /// <param name="statusCode">The status code to get the description for.</param>
    /// <returns>The corresponding response description or an empty string.</returns>
    /// <remarks>This method is based on the custom extension element that was introduced and popularized by
    /// Swashbuckle; for example, <c>&lt;response code="200"&gt;The operation was successful&lt;/response&gt;</c>. See the
    /// <a href="https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle">tutorial</a>
    /// for more information.</remarks>
    public string GetResponseDescription( MemberInfo member, string statusCode )
    {
        if ( GetMember( member ) is { } element )
        {
            return element.Elements( "response" )
                          .FirstOrDefault( x => x.Attribute( "code" )?.Value == statusCode )?
                          .Value
                          .Trim() ?? string.Empty;
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets the <c>member</c> documentation for the specified type member.
    /// </summary>
    /// <param name="member">The member to get the information for.</param>
    /// <returns>The <see cref="XElement"/> representing the matching <c>member</c> element or <c>null</c>.</returns>
    protected XElement? GetMember( MemberInfo member ) =>
        GetMemberById( XmlCommentsProvider.GetDocumentationMemberId( member ) );

    private static XElement? FindMember( XDocument xml, string key ) =>
        xml.Descendants( "member" ).FirstOrDefault( member => member.Attribute( "name" )?.Value == key );

    private XElement? GetMemberById( string id ) => members.GetOrAdd( id, key => FindMember( Xml, key ) );
}