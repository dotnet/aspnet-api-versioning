namespace System.Web.Http
{
    using System.Diagnostics.Contracts;
    using System.Net.Http.Formatting;

    static class MediaTypeFormatterExtensions
    {
        internal static MediaTypeFormatter Clone( this MediaTypeFormatter formatter )
        {
            Contract.Requires( formatter != null );
            Contract.Ensures( Contract.Result<MediaTypeFormatter>() != null );

            var clone = MediaTypeFormatterAdapterFactory.GetOrCreateCloneFunction( formatter );
            return clone( formatter );
        }
    }
}