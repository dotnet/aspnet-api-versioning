// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Transformers;

using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using static System.Globalization.CultureInfo;
using static System.Reflection.BindingFlags;

/// <summary>
/// Provides access to XML documentation comments, which enables the retrieval of summaries, remarks, return values,
/// examples, and parameter descriptions.
/// </summary>
public class XmlComments
{
    private const int MaxInheritDocDepth = 8;
    private static readonly CompositeFormat ItemOfFormat = CompositeFormat.Parse( "**{0}**: {1}" );
    private static readonly CompositeFormat LinkFormat = CompositeFormat.Parse( "[{0}]({1})" );
    private static readonly CompositeFormat BlockFormat = CompositeFormat.Parse( "\n{0}\n" );
    private static readonly CompositeFormat FenceBlockFormat = CompositeFormat.Parse( "\n{0}\n{1}\n{0}\n" );
    private readonly ConcurrentDictionary<string, XElement?> members = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlComments"/> class.
    /// </summary>
    /// <param name="path">The file path of the XML comments to read.</param>
    /// <remarks>The whitespace of the source file is preserved. A reader discards a text node that is only
    /// whitespace by default, which is not insignificant here: it is the indentation the file is written with,
    /// and it is the only way to know the margin that has to be removed before the text reads as Markdown. It is
    /// also the space between two adjacent tags, which is the space between the words they wrap.</remarks>
    protected XmlComments( string path ) =>
        Xml = File.Exists( path ) ? XDocument.Load( path, LoadOptions.PreserveWhitespace ) : new();

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
    /// <returns>The corresponding <c>&lt;summary&gt;</c> or an empty string.</returns>
    public virtual string GetSummary( MemberInfo member ) => Text( GetMember( member )?.Element( "summary" ) );

    /// <summary>
    /// Gets the <c>description</c> from the specified member, if any.
    /// </summary>
    /// <param name="member">The member to get the description from.</param>
    /// <returns>The corresponding <c>&lt;description&gt;</c> or an empty string.</returns>
    public virtual string GetDescription( MemberInfo member ) => Text( GetMember( member )?.Element( "description" ) );

    /// <summary>
    /// Gets the <c>remarks</c> from the specified member, if any.
    /// </summary>
    /// <param name="member">The member to get the remarks from.</param>
    /// <returns>The corresponding <c>&lt;remarks&gt;</c> or an empty string.</returns>
    public virtual string GetRemarks( MemberInfo member ) => Text( GetMember( member )?.Element( "remarks" ) );

    /// <summary>
    /// Gets the <c>returns</c> from the specified member, if any.
    /// </summary>
    /// <param name="member">The member to get the returns from.</param>
    /// <returns>The corresponding <c>&lt;returns&gt;</c> or an empty string.</returns>
    public virtual string GetReturns( MemberInfo member ) => Text( GetMember( member )?.Element( "returns" ) );

    /// <summary>
    /// Gets the <c>value</c> from the specified member, if any.
    /// </summary>
    /// <param name="member">The member to get the value from.</param>
    /// <returns>The corresponding <c>&lt;value&gt;</c> or an empty string.</returns>
    /// <remarks>The <c>&lt;value&gt;</c> tag documents what a property represents and is complementary to
    /// <c>&lt;summary&gt;</c> rather than a replacement for it. A property can have both.</remarks>
    public virtual string GetValue( MemberInfo member ) => Text( GetMember( member )?.Element( "value" ) );

    /// <summary>
    /// Gets the <c>example</c> from the specified member, if any.
    /// </summary>
    /// <param name="member">The member to get the example from.</param>
    /// <returns>The corresponding <c>&lt;example&gt;</c> or an empty string.</returns>
    public virtual string GetExample( MemberInfo member ) => Text( GetMember( member )?.Element( "example" ) );

    /// <summary>
    /// Gets the <c>param</c> description from the specified member, if any.
    /// </summary>
    /// <param name="member">The member to get the parameter from.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <returns>The corresponding description or an empty string.</returns>
    public virtual string GetParameterDescription( MemberInfo member, string name )
    {
        if ( GetMember( member ) is { } element )
        {
            return Text( element.Elements( "param" )
                                .FirstOrDefault( x => x.Attribute( "name" )?.Value == name ) );
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets the description for a parameter.
    /// </summary>
    /// <param name="parameter">The parameter to get the description for.</param>
    /// <returns>The corresponding description or an empty string.</returns>
    /// <remarks><see cref="GetParameterDescription(MemberInfo, string)"/> is typically used to get the description of
    /// a parameter, which come from the <c>&lt;param/&gt;</c> tag for a parameter on a method. Some API styles, such as
    /// gRPC, use request/reply messages that graft its own message semantics over the HTTP semantics. In these
    /// scenarios, what the API Explorer considers a parameter is actually a property on a request message. When this
    /// happens, we need to get description from the source property. <see cref="ParameterInfo"/> does not derive from
    /// <see cref="MemberInfo"/> so that path doesn't work. If parameter-to-property mapping has been established, then
    /// the expectation is that <see cref="ParameterInfo.Member"/> is the property and the associated
    /// <c>&lt;summary/&gt;</c> tag provides the description. This approach is always considered a fallback mechanism.
    /// </remarks>
    internal string GetParameterDescription( ParameterInfo parameter ) => GetSummary( parameter.Member );

    /// <summary>
    /// Gets the parameter <c>example</c> from the specified member, if any.
    /// </summary>
    /// <param name="member">The member to get the parameter from.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <returns>The corresponding <c>&lt;example&gt;</c> or an empty string.</returns>
    public virtual string GetParameterExample( MemberInfo member, string name )
    {
        if ( GetMember( member ) is { } element )
        {
            return Text( element.Elements( "param" )
                                .FirstOrDefault( x => x.Attribute( "name" )?.Value == name )?
                                .Attribute( "example" ) );
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets the <c>deprecated</c> attribute from the specified member, if any.
    /// </summary>
    /// <param name="member">The member to get the parameter from.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <returns><c>true</c> if the <c>deprecated</c> attribute is present with a value of <c>"true"</c>;
    /// otherwise <c>false</c>.</returns>
    public virtual bool IsParameterDeprecated( MemberInfo member, string name )
    {
        if ( GetMember( member ) is { } element )
        {
            var deprecated = element.Elements( "param" )
                                    .FirstOrDefault( x => x.Attribute( "name" )?.Value == name )?
                                    .Attribute( "deprecated" )?.Value;

            if ( deprecated is { } value )
            {
                return StringComparer.OrdinalIgnoreCase.Equals( value, bool.TrueString );
            }
        }

        return false;
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
    public virtual string GetResponseDescription( MemberInfo member, int statusCode )
        => GetResponseDescription( member, statusCode.ToString( InvariantCulture ) );

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
    public virtual string GetResponseDescription( MemberInfo member, string statusCode )
    {
        if ( GetMember( member ) is { } element )
        {
            return Text( element.Elements( "response" )
                                .FirstOrDefault( x => x.Attribute( "code" )?.Value == statusCode ) );
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets the <c>member</c> documentation for the specified type member.
    /// </summary>
    /// <param name="member">The member to get the information for.</param>
    /// <returns>The <see cref="XElement"/> representing the matching <c>member</c> element or <c>null</c>.</returns>
    protected virtual XElement? GetMember( MemberInfo member ) => ResolveMember( member, depth: 0 );

    private XElement? ResolveMember( MemberInfo member, int depth )
    {
        var element = GetMemberById( XmlCommentsProvider.GetDocumentationMemberId( member ) );

        // The compiler writes <inheritdoc /> verbatim into the XML file; following it is the consumer's
        // responsibility. Resolve it so members documented on a base type or an implemented interface still
        // surface their summary, remarks, parameters, and so on.
        if ( depth < MaxInheritDocDepth && element?.Element( "inheritdoc" ) is { } inheritdoc )
        {
            if ( ResolveInheritDoc( member, inheritdoc, depth ) is { } inherited )
            {
                return inherited;
            }
        }

        return element;
    }

    private XElement? ResolveInheritDoc( MemberInfo member, XElement inheritdoc, int depth )
    {
        // Explicit source: <inheritdoc cref="..." />
        if ( inheritdoc.Attribute( "cref" )?.Value is { Length: > 0 } cref
             && GetMemberById( cref ) is { } referenced
             && referenced.Element( "inheritdoc" ) is null )
        {
            return referenced;
        }

        // Implicit source: the same member on a base type, then on an implemented interface
        foreach ( var inheritedMember in GetInheritedMembers( member ) )
        {
            if ( ResolveMember( inheritedMember, depth + 1 ) is { } resolved
                 && resolved.Element( "inheritdoc" ) is null )
            {
                return resolved;
            }
        }

        return null;
    }

    [UnconditionalSuppressMessage( "ILLink", "IL2070" )]
    private static IEnumerable<MemberInfo> GetInheritedMembers( MemberInfo member )
    {
        // <inheritdoc /> on a type inherits from its base type and then its implemented interfaces
        if ( member is Type type )
        {
            if ( type.BaseType is { } baseType && baseType != typeof( object ) )
            {
                yield return baseType;
            }

            foreach ( var contract in type.GetInterfaces() )
            {
                yield return contract;
            }

            yield break;
        }

        if ( member.DeclaringType is not { } declaringType )
        {
            yield break;
        }

        // Base class first (overrides and hidden members), then implemented interfaces
        if ( declaringType.BaseType is { } baseDeclaringType
             && baseDeclaringType != typeof( object )
             && FindMatchingMember( baseDeclaringType, member ) is { } baseMember )
        {
            yield return baseMember;
        }

        foreach ( var contract in declaringType.GetInterfaces() )
        {
            if ( FindMatchingMember( contract, member ) is { } interfaceMember )
            {
                yield return interfaceMember;
            }
        }
    }

    [UnconditionalSuppressMessage( "ILLink", "IL2070" )]
    private static MemberInfo? FindMatchingMember( Type type, MemberInfo member )
    {
        const BindingFlags Flags = Public | NonPublic | Instance | Static;

        switch ( member )
        {
            case PropertyInfo property:
                return type.GetProperty( property.Name, Flags );
            case MethodInfo method:
                var parameterTypes = method.GetParameters().Select( parameter => parameter.ParameterType ).ToArray();
                return type.GetMethod( method.Name, Flags, binder: null, parameterTypes, modifiers: null )
                       ?? type.GetMethods( Flags ).FirstOrDefault(
                              candidate => candidate.Name == method.Name
                                  && candidate.GetParameters().Length == parameterTypes.Length );
            default:
                return type.GetMember( member.Name, Flags ).FirstOrDefault();
        }
    }

    private static XElement? FindMember( XDocument xml, string key ) =>
        xml.Descendants( "member" ).FirstOrDefault( member => member.Attribute( "name" )?.Value == key );

    private XElement? GetMemberById( string id ) => members.GetOrAdd( id, key => Normalize( FindMember( Xml, key ) ) );

    // The compiler copies the documentation tags into the XML file verbatim, so reading the text of an element
    // yields the concatenated text of everything inside it. Tags that convey structure are lost; the items of a
    // list run together into a single sentence and code samples are indistinguishable from prose. Rewrite those
    // tags into the Markdown that OpenAPI descriptions are rendered as before the text is read.
    //
    // The result is cached, so this is done once per member. A copy is normalized to leave the source document
    // untouched; the same element can be reached again through <inheritdoc />.
    private static XElement? Normalize( XElement? element )
    {
        if ( element is null )
        {
            return null;
        }

        var copy = new XElement( element );

        // strip the indentation the XML file is written with before anything is substituted. doing it up front
        // means replacement text needs no indentation of its own, which cannot be derived reliably anyway: XLinq
        // merges the text nodes around a replaced element, so the text preceding a sibling tag changes as the
        // pass proceeds
        Dedent( copy );

        // order matters. <code> is resolved first so an inline tag nested in a block stays literal, then the
        // tags that produce inline text so they survive being flattened into a list item, a table cell, or a
        // paragraph. the tags that absorb their content are resolved last for the same reason.
        //
        // every pass below snapshots what it walks with ToArray() before it rewrites anything. Descendants() is
        // a lazy walk over the live tree, so replacing an element mid-enumeration detaches the node the iterator
        // is standing on and the walk faults. the Parent check that follows the snapshot is a separate concern:
        // a node the snapshot captured may have since been absorbed by the replacement of an ancestor.
        ResolveCodeBlocks( copy );
        ResolveInlineCode( copy );
        ResolveParamRefTags( copy );
        ResolveInlineTags( copy );
        ResolveListTags( copy );
        ResolveParaTags( copy );

        return copy;
    }

    private static string Text( XElement? element ) => element is null ? string.Empty : TrimEachLine( element.Value );

    private static string Text( XAttribute? attribute ) => attribute is null ? string.Empty : TrimEachLine( attribute.Value );

    // <paramref /> refers to a parameter of the operation, which is part of the API over the wire, so its name
    // is meaningful in a description. <typeparamref /> is not; a type parameter is a C# concept with no
    // representation in a request or a response, and neither is <see langword="" /> or <see cref="" />.
    //
    // The tag names a code element rather than describing one, so the name is rendered as inline code. It reads
    // as the identifier it is instead of running together with the prose around it.
    private static void ResolveParamRefTags( XElement element )
    {
        foreach ( var paramRef in element.Descendants( "paramref" ).ToArray().Where( e => e.Parent is not null ) )
        {
            if ( paramRef.Attribute( "name" )?.Value is { Length: > 0 } name )
            {
                paramRef.ReplaceWith( new XText( Delimit( name, "`" ) ) );
            }
        }
    }

    // A blank line is what separates paragraphs in the Markdown a description is rendered as; a single line break
    // is only a soft break and renders as a space, which would leave the tag with no effect. The content of the
    // containing element is rebuilt rather than each <para /> being replaced in turn, because the whitespace
    // between two of them is not a reliable separator: XLinq merges the text nodes around a replaced element.
    //
    // Only a <para /> starts a paragraph. Everything between two of them belongs to the same one, including a tag
    // left in the tree because it has no Markdown of its own, such as <see /> or <u />. Reading one of those as a
    // block of its own would break the sentence around it into a paragraph per fragment.
    //
    // This runs last, so every other tag has already been resolved to text and <para /> is the only element left
    // that carries structure.
    private static void ResolveParaTags( XElement element )
    {
        foreach ( var parent in element.DescendantsAndSelf().ToArray() )
        {
            if ( !parent.Elements( "para" ).Any() )
            {
                continue;
            }

            var blocks = new List<string>();
            var paragraph = new StringBuilder();

            foreach ( var node in parent.Nodes() )
            {
                if ( node is XElement para && para.Name == "para" )
                {
                    AddBlock( blocks, paragraph.ToString() );
                    paragraph.Clear();
                    AddBlock( blocks, para.Value );
                }
                else if ( node is XText content )
                {
                    paragraph.Append( content.Value );
                }
                else if ( node is XElement other )
                {
                    paragraph.Append( other.Value );
                }
            }

            AddBlock( blocks, paragraph.ToString() );
            parent.ReplaceNodes( new XText( string.Join( "\n\n", blocks ) ) );
        }
    }

    private static void AddBlock( List<string> blocks, string text )
    {
        var block = TrimEachLine( text );

        if ( block.Length > 0 )
        {
            blocks.Add( block );
        }
    }

    private static void ResolveListTags( XElement element )
    {
        foreach ( var list in element.Descendants( "list" ).ToArray() )
        {
            if ( list.Parent is null )
            {
                continue;
            }

            var type = list.Attribute( "type" )?.Value;

            if ( StringComparer.Ordinal.Equals( type, "table" )
                 && TryResolveTable( list, out var table ) )
            {
                list.ReplaceWith( new XText( table ) );
                continue;
            }

            var bullets = type switch
            {
                "number" => BulletedList.Numbered(),
                _ => BulletedList.Bullets(),
            };
            var items = list.Elements( "item" ).Select( item => bullets.Next() + ItemOf( item ) );
            var block = string.Format( InvariantCulture, BlockFormat, string.Join( '\n', items ) );

            // a list is a block, so like a table it is surrounded by blank lines. the line that follows the last
            // item would otherwise be a lazy continuation of it and the text written after the list would be
            // pulled into it; the line that precedes the first item would be the paragraph the list interrupts.
            list.ReplaceWith( new XText( block ) );
        }
    }

    // an item can be written as plain text or as a definition of a <term /> by a <description />. markdown has no
    // definition list, so a definition renders as a bolded term followed by its description. reading the text of
    // the item would run the two together because the tags are adjacent.
    //
    // this is deliberately not the cell handling used for a table. outside of a table a pipe is an ordinary
    // character and the line structure of a description is worth keeping.
    private static string ItemOf( XElement item )
    {
        var term = item.Element( "term" );
        var description = item.Element( "description" );

        if ( term is null && description is null )
        {
            return TrimEachLine( item.Value );
        }

        var name = term is null ? string.Empty : TrimEachLine( term.Value );
        var text = description is null ? string.Empty : TrimEachLine( description.Value );

        if ( name.Length == 0 )
        {
            return text;
        }

        return text.Length == 0 ? name : string.Format( InvariantCulture, ItemOfFormat, name, text );
    }

    // a table is the one list type with a markdown equivalent that is not a list. it only resolves to a table when
    // there is more than one column; a table of one column conveys nothing a bulleted list does not, so the caller
    // falls back to bullets. markdown requires a header row, so a list with no <listheader /> gets a blank one.
    private static bool TryResolveTable( XElement list, [NotNullWhen( true )] out string? table )
    {
        var header = CellsOf( list.Element( "listheader" ) );
        var rows = list.Elements( "item" ).Select( CellsOf ).ToList();
        var columns = header.Count;

        for ( var i = 0; i < rows.Count; i++ )
        {
            columns = Math.Max( columns, rows[i].Count );
        }

        if ( columns < 2 )
        {
            table = default;
            return false;
        }

        // a table cannot interrupt a paragraph, so it is surrounded by blank lines
        var markdown = new StringBuilder( "\n" );

        AppendRow( markdown, header, columns );
        AppendRow( markdown, [.. Enumerable.Repeat( "---", columns )], columns );

        for ( var i = 0; i < rows.Count; i++ )
        {
            AppendRow( markdown, rows[i], columns );
        }

        table = markdown.ToString();
        return true;
    }

    private static List<string> CellsOf( XElement? row )
    {
        if ( row is null )
        {
            return [];
        }

        var cells = row.Elements()
                       .Where( cell => cell.Name == "term" || cell.Name == "description" )
                       .Select( cell => CellOf( cell.Value ) )
                       .ToList();

        // an item written without <term /> or <description /> is a single cell of text
        if ( cells.Count == 0 )
        {
            cells.Add( CellOf( row.Value ) );
        }

        return cells;
    }

    // a row occupies a single line and is delimited by pipes, so the whitespace in a cell is collapsed and any
    // pipe of its own is escaped. neither can be represented in a cell otherwise.
    private static string CellOf( string text ) => Flatten( text ).Replace( "|", "\\|", StringComparison.Ordinal );

    private static void AppendRow( StringBuilder table, List<string> cells, int columns )
    {
        table.Append( '|' );

        for ( var i = 0; i < columns; i++ )
        {
            var cell = i < cells.Count ? cells[i] : string.Empty;

            table.Append( ' ' );

            if ( cell.Length > 0 )
            {
                table.Append( cell ).Append( ' ' );
            }

            table.Append( '|' );
        }

        table.Append( '\n' );
    }

    // a fence only opens and closes a code block when it sits on a line of its own, so the block is surrounded
    // by line breaks instead of being wrapped in place; a fence written inline is literal text and the block
    // would never be closed. a code block cannot interrupt a paragraph either, which the leading break also
    // takes care of.
    private static void ResolveCodeBlocks( XElement element )
    {
        foreach ( var code in element.Descendants( "code" ).ToArray().Where( e => e.Parent is not null ) )
        {
            var content = TrimEachLine( code.Value );
            var fence = FenceFor( content );
            var block = new XText( string.Format( InvariantCulture, FenceBlockFormat, fence, content ) );

            code.ReplaceWith( block );
        }
    }

    // a fence has to be longer than any run of backticks in the block it delimits, three being the customary
    // minimum. a sample that contains a fence of its own would otherwise close the block early and leave the
    // remainder of the sample to be read as prose.
    private static string FenceFor( string code )
    {
        const int MinimumFence = 3;
        var longest = 0;
        var run = 0;

        for ( var i = 0; i < code.Length; i++ )
        {
            run = code[i] == '`' ? run + 1 : 0;
            longest = Math.Max( longest, run );
        }

        return new string( '`', Math.Max( MinimumFence, longest + 1 ) );
    }

    private static void ResolveInlineCode( XElement element )
    {
        foreach ( var code in element.Descendants( "c" ).ToArray().Where( e => e.Parent is not null ) )
        {
            code.ReplaceWith( new XText( Delimit( code.Value, "`" ) ) );
        }
    }

    // <b>, <i>, and <a> are the html tags a documentation comment carries inline, and each has a direct markdown
    // equivalent. <see /> and <seealso /> join them in the one form that is a link rather than a reference to a
    // code element. rewriting them keeps the meaning that reading the text of the enclosing element would drop.
    // the tags are visited from the inside out so that one nested in another is rewritten before it is absorbed.
    //
    // emphasis is delimited by an asterisk rather than an underscore. the two are interchangeable on their own,
    // but an underscore only opens or closes emphasis at a word boundary, so it is literal text in the middle of
    // a word and it does not pair with the asterisk of a <b> nested the other way around; <b><i>x</i></b> and
    // <i><b>x</b></i> then render differently despite meaning the same thing.
    private static void ResolveInlineTags( XElement element )
    {
        foreach ( var inline in element.Descendants().Reverse().ToArray().Where( e => e.Parent is not null ) )
        {
            var text = inline.Name.LocalName switch
            {
                "b" => Delimit( inline.Value, "**" ),
                "i" => Delimit( inline.Value, "*" ),
                "a" => LinkOf( inline ),
                "see" or "seealso" => SeeOf( inline ),
                _ => default,
            };

            if ( text is not null )
            {
                inline.ReplaceWith( new XText( text ) );
            }
        }
    }

    // <see href="" /> and <seealso href="" /> are links written with the tags a documentation comment already uses
    // for a reference; they are the form the documentation generators accept as an alternative to <a />, so they
    // resolve the same way. both are inline where they are written, so the address belongs in the sentence around
    // it; only the tooling that renders a separate "see also" section treats <seealso /> as a block of its own.
    //
    // every other form names a code element instead of an address - <see cref="" /> and <see langword="" /> have
    // no markdown of their own - and is left in the tree for the text it wraps, if any, to be read in place.
    private static string? SeeOf( XElement see ) => see.Attribute( "href" ) is null ? default : LinkOf( see );

    // a link with no text has nothing to show but its own address, so the address becomes the text. writing the
    // address on its own would rely on the renderer turning it into a link, which is an extension that not every
    // renderer implements; Scalar does and Swagger UI does not. a link with no address is not a link at all, so
    // only the text it wrapped is kept.
    private static string LinkOf( XElement anchor )
    {
        var text = Flatten( anchor.Value );
        var href = Flatten( anchor.Attribute( "href" )?.Value ?? string.Empty );

        if ( href.Length == 0 )
        {
            return text;
        }

        if ( string.IsNullOrWhiteSpace( text ) )
        {
            text = href;
        }

        return string.Format( InvariantCulture, LinkFormat, text, href );
    }

    // a span occupies a single line and its delimiters cannot be padded by whitespace, so the content is
    // flattened. an empty tag is dropped; a pair of delimiters with nothing between them is literal text.
    private static string Delimit( string value, string delimiter )
    {
        var text = Flatten( value );

        return text.Length == 0 ? string.Empty : delimiter + text + delimiter;
    }

    // collapses a run of text onto a single line. the markdown constructs that occupy exactly one line - a table
    // row, a span - cannot carry the line breaks and indentation the xml file is written with.
    private static string Flatten( string text )
    {
        var flattened = new StringBuilder( text.Length );
        var space = false;

        for ( var i = 0; i < text.Length; i++ )
        {
            var ch = text[i];

            if ( char.IsWhiteSpace( ch ) )
            {
                space = flattened.Length > 0;
                continue;
            }

            if ( space )
            {
                flattened.Append( ' ' );
                space = false;
            }

            flattened.Append( ch );
        }

        return flattened.ToString();
    }

    // the xml file is written with its own indentation, which becomes part of the text of every element inside a
    // member. remove it from each text node so the member reads as it was written and substituted text does not
    // have to reproduce it. an xml processor normalizes line endings, so only '\n' occurs here.
    private static void Dedent( XElement element )
    {
        var margin = MarginOf( element.Value );

        if ( margin == 0 )
        {
            return;
        }

        foreach ( var text in element.DescendantNodes().OfType<XText>() )
        {
            text.Value = Unindent( text.Value, margin );
        }
    }

    private static string Unindent( string text, int margin )
    {
        var unindented = new StringBuilder( text.Length );
        var i = 0;

        while ( i < text.Length )
        {
            var ch = text[i++];

            unindented.Append( ch );

            if ( ch != '\n' )
            {
                continue;
            }

            for ( var j = 0; j < margin && i < text.Length && ( text[i] == ' ' || text[i] == '\t' ); j++ )
            {
                i++;
            }
        }

        return unindented.ToString();
    }

    private static int MarginOf( string text )
    {
        var lines = text.Split( '\n' );
        var margin = int.MaxValue;

        // the text of a member begins immediately after its opening tag rather than at the start of a line, so
        // whatever precedes the first break carries none of the indentation the margin is measured from. counting
        // it would report a margin of zero and leave the indentation on every line that does start one.
        for ( var i = 1; i < lines.Length; i++ )
        {
            var line = lines[i];

            if ( line.Trim().Length == 0 )
            {
                continue;
            }

            var indent = 0;

            while ( indent < line.Length && char.IsWhiteSpace( line[indent] ) )
            {
                indent++;
            }

            margin = Math.Min( margin, indent );
        }

        return margin == int.MaxValue ? 0 : margin;
    }

    // trims each line while preserving relative indentation. a code sample is indented to match the source it
    // was written in; removing only the common indentation keeps the sample readable without the leading noise.
    private static string TrimEachLine( string text )
    {
        var lines = text.Split( '\n' );
        var margin = int.MaxValue;

        for ( var i = 0; i < lines.Length; i++ )
        {
            var line = lines[i].TrimEnd();

            if ( line.Length == 0 )
            {
                continue;
            }

            var indent = 0;

            while ( indent < line.Length && char.IsWhiteSpace( line[indent] ) )
            {
                indent++;
            }

            margin = Math.Min( margin, indent );
        }

        if ( margin == int.MaxValue )
        {
            margin = 0;
        }

        var trimmed = new StringBuilder();
        var empty = true;

        for ( var i = 0; i < lines.Length; i++ )
        {
            var line = lines[i].TrimEnd();

            // skip leading blank lines; the tag is almost always opened on its own line
            if ( empty && line.Length == 0 )
            {
                continue;
            }

            if ( !empty )
            {
                trimmed.Append( '\n' );
            }

            empty = false;
            trimmed.Append( line.Length > margin ? line[margin..] : string.Empty );
        }

        return trimmed.ToString().TrimEnd();
    }

    private sealed class BulletedList
    {
        private readonly Func<int, string> generate;
        private int count;

        private BulletedList( Func<int, string> generate ) => this.generate = generate;

        private static string Increment( int number ) => number.ToString( InvariantCulture ) + ". ";

        private static string Bullet( int number ) => "* ";

        public static BulletedList Numbered() => new( Increment );

        public static BulletedList Bullets() => new( Bullet );

        public string Next() => generate( ++count );
    }
}