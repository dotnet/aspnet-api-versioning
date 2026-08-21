// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable SA1629

namespace Asp.Versioning.OpenApi.Simulators;

/// <summary>
/// Represents a model whose members exercise the documentation tags that convey structure.
/// </summary>
public class Documented
{
    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    /// <value>Defaults to <c>Pending</c>.</value>
    public string Status { get; set; }

    // deliberately documented with <value> and no <summary>
#pragma warning disable SA1604
    /// <value>The unique identifier.</value>
    public int Kind { get; set; }
#pragma warning restore SA1604

    /// <summary>
    /// Gets or sets the tags.
    /// <list type="bullet">
    /// <item>First</item>
    /// <item>Second</item>
    /// </list>
    /// </summary>
    public string Tags { get; set; }

    /// <summary>
    /// Gets or sets the steps.
    /// <list type="number">
    /// <item>First</item>
    /// <item>Second</item>
    /// </list>
    /// </summary>
    public string Steps { get; set; }

    /// <summary>
    /// Gets or sets the format, which is <c>yyyy-MM-dd</c>.
    /// </summary>
    public string Format { get; set; }

    /// <summary>
    /// Gets or sets the sample.
    /// <code>
    ///     var value = 42;
    ///     Use( value );
    /// </code>
    /// </summary>
    public string Sample { get; set; }

    /// <summary>
    /// Gets or sets the snippet.
    /// <code>
    ///     The fence is ``` here.
    /// </code>
    /// </summary>
    public string Snippet { get; set; }

    /// <summary>
    /// Gets or sets the definitions.
    /// <list type="bullet">
    /// <item><term>Foo</term><description>Does foo</description></item>
    /// <item><term>Bar</term></item>
    /// <item><description>Just a description</description></item>
    /// <item>Plain text</item>
    /// </list>
    /// </summary>
    public string Definitions { get; set; }

    /// <summary>
    /// Gets or sets the matrix.
    /// <list type="table">
    /// <listheader><term>Name</term><description>Meaning</description></listheader>
    /// <item><term>A</term><description>First</description></item>
    /// <item><term>B</term><description>Second</description></item>
    /// </list>
    /// </summary>
    public string Matrix { get; set; }

    /// <summary>
    /// Gets or sets the headerless.
    /// <list type="table">
    /// <item><term>A</term><description>Uses a | pipe</description></item>
    /// </list>
    /// </summary>
    public string Headerless { get; set; }

    /// <summary>
    /// Gets or sets the narrow.
    /// <list type="table">
    /// <item><description>Only</description></item>
    /// <item><description>One</description></item>
    /// </list>
    /// </summary>
    public string Narrow { get; set; }

    /// <summary>
    /// Gets or sets the notes.
    /// <para>The first note.</para>
    /// <para>The second note, which mentions <c>Status</c>.</para>
    /// </summary>
    public string Notes { get; set; }

    /// <summary>
    /// Gets or sets the highlights, which are <b>important</b> and <i>subtle</i>.
    /// </summary>
    public string Highlights { get; set; }

    /// <summary>
    /// Gets or sets the emphasis, which is <b>very <i>strongly</i> worded</b>.
    /// </summary>
    public string Emphasis { get; set; }

    /// <summary>
    /// Gets or sets the sibling, which is <u>underlined</u> in place.
    /// <para>A note.</para>
    /// </summary>
    public string Sibling { get; set; }

    /// <summary>
    /// Gets or sets the adjacent.
    /// <para>
    ///     <b>Remark <i>of</i></b> <u>GetToDo</u>
    /// </para>
    /// <para>
    ///     <b>Remark</b>   <i>of</i>   <u>GetToDo</u>
    /// </para>
    /// </summary>
    public string Adjacent { get; set; }

    /// <summary>
    /// Gets or sets the intraword.
    /// <para>
    ///     Very<b><i>long</i></b>word
    /// </para>
    /// <para>
    ///     Very<i><b>long</b></i>word
    /// </para>
    /// </summary>
    public string Intraword { get; set; }

    /// <summary>
    /// Gets or sets the reference, which is described by <a href="https://example.com">the
    /// specification</a>.
    /// </summary>
    public string Reference { get; set; }

    /// <summary>
    /// Gets or sets the site, which is <a href="https://example.com" />.
    /// </summary>
    public string Site { get; set; }

    /// <summary>
    /// Gets or sets the spec, which is described by <a href="https://example.com/spec"></a>.
    /// </summary>
    public string Spec { get; set; }

    /// <summary>
    /// Gets or sets the anonymous, which is described by <a>the specification</a>.
    /// </summary>
    public string Anonymous { get; set; }

    /// <summary>
    /// Gets or sets the manual, which is described by <see href="https://example.com">the
    /// specification</see>.
    /// </summary>
    public string Manual { get; set; }

    /// <summary>
    /// Gets or sets the guide, which is described by <see href="https://example.com/guide" />.
    /// </summary>
    public string Guide { get; set; }

    /// <summary>
    /// Gets or sets the cited. <see cref="Status" />
    /// </summary>
    public string Cited { get; set; }

    /// <summary>
    /// Gets or sets the related, which is described by <seealso href="https://example.com/related">the related
    /// specification</seealso>.
    /// </summary>
    public string Related { get; set; }

    /// <summary>
    /// Gets or sets the referred. <seealso cref="Status" />
    /// </summary>
    public string Referred { get; set; }

    /// <summary>
    /// Gets or sets the outline.
    /// <list type="number">
    /// <item><description>First step</description></item>
    /// <item><description>Second step</description></item>
    /// </list>
    /// Text after list
    /// </summary>
    public string Outline { get; set; }
}