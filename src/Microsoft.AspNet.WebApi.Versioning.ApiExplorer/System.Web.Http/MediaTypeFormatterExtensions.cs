namespace System.Web.Http
{
    using System.Net.Http.Formatting;

    static class MediaTypeFormatterExtensions
    {
        internal static MediaTypeFormatter Clone( this MediaTypeFormatter formatter )
        {
            var clone = MediaTypeFormatterAdapterFactory.GetOrCreateCloneFunction( formatter );
            return clone( formatter );
        }
    }
}