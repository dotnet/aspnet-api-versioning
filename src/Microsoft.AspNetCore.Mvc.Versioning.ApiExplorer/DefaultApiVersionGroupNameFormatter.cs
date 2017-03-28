namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using System.Text;
    using static System.Globalization.CultureInfo;
    using static System.String;

    /// <summary>
    /// Represents the default implementation used to format group names for API versions.
    /// </summary>
    public sealed class DefaultApiVersionGroupNameFormatter : IApiVersionGroupNameFormatter
    {
        /// <summary>
        /// Returns the group name for the specified API version.
        /// </summary>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to retrieve a group name for.</param>
        /// <returns>The group name for the specified <paramref name="apiVersion">API version</paramref>.</returns>
        public string GetGroupName( ApiVersion apiVersion )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );

            var format = new StringBuilder();
            var formatProvider = InvariantCulture;

            if ( apiVersion.GroupVersion == null )
            {
                format.Append( 'v' );
                format.Append( apiVersion.MajorVersion ?? 0 );

                if ( apiVersion.MinorVersion != null && apiVersion.MinorVersion.Value > 0 )
                {
                    format.Append( '.' );
                    format.Append( apiVersion.MinorVersion );
                }
            }
            else
            {
                format.Append( apiVersion.GroupVersion.Value.ToString( "yyyy-MM-dd", formatProvider ) );

                if ( apiVersion.MajorVersion == null )
                {
                    if ( apiVersion.MinorVersion != null )
                    {
                        format.Append( "-0." );
                        format.Append( apiVersion.MinorVersion.Value );
                    }
                }
                else
                {
                    format.Append( '-' );
                    format.Append( apiVersion.MajorVersion.Value );

                    if ( apiVersion.MinorVersion != null && apiVersion.MinorVersion.Value > 0 )
                    {
                        format.Append( '.' );
                        format.Append( apiVersion.MinorVersion.Value );
                    }
                }
            }

            if ( !IsNullOrEmpty( apiVersion.Status ) )
            {
                format.Append( '-' );
                format.Append( apiVersion.Status );
            }

            return format.ToString();
        }
    }
}