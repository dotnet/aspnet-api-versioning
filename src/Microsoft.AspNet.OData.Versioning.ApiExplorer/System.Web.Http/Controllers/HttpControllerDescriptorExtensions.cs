namespace System.Web.Http.Controllers
{
    using System;
    using System.Collections.Generic;

    static class HttpControllerDescriptorExtensions
    {
        const string RelatedControllerCandidatesKey = "MS_RelatedControllerCandidates";

        internal static IEnumerable<HttpControllerDescriptor> AsEnumerable( this HttpControllerDescriptor controllerDescriptor )
        {
            if ( controllerDescriptor.Properties.TryGetValue( RelatedControllerCandidatesKey, out object value ) )
            {
                if ( value is IEnumerable<HttpControllerDescriptor> relatedCandidates )
                {
                    using ( var relatedControllerDescriptors = relatedCandidates.GetEnumerator() )
                    {
                        if ( relatedControllerDescriptors.MoveNext() )
                        {
                            yield return controllerDescriptor;

                            do
                            {
                                if ( relatedControllerDescriptors.Current != controllerDescriptor )
                                {
                                    yield return relatedControllerDescriptors.Current;
                                }
                            }
                            while ( relatedControllerDescriptors.MoveNext() );

                            yield break;
                        }
                    }
                }
            }

            if ( controllerDescriptor is IEnumerable<HttpControllerDescriptor> groupedControllerDescriptors )
            {
                foreach ( var groupedControllerDescriptor in groupedControllerDescriptors )
                {
                    yield return groupedControllerDescriptor;
                }
            }
            else
            {
                yield return controllerDescriptor;
            }
        }
    }
}