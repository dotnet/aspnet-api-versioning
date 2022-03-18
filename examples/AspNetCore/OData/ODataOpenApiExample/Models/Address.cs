namespace ApiVersioning.Examples.Models;

using System.Runtime.Serialization;

/// <summary>
/// Represents an address.
/// </summary>
[DataContract]
public class Address
{
    /// <summary>
    /// Gets or sets the address identifier.
    /// </summary>
    [IgnoreDataMember]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the street address.
    /// </summary>
    /// <value>The street address.</value>
    [DataMember]
    public string Street { get; set; }

    /// <summary>
    /// Gets or sets the address city.
    /// </summary>
    /// <value>The address city.</value>
    [DataMember]
    public string City { get; set; }

    /// <summary>
    /// Gets or sets the address state.
    /// </summary>
    /// <value>The address state.</value>
    [DataMember]
    public string State { get; set; }

    /// <summary>
    /// Gets or sets the address zip code.
    /// </summary>
    /// <value>The address zip code.</value>
    [DataMember( Name = "zip" )]
    public string ZipCode { get; set; }
}