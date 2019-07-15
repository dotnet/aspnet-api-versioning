using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Examples.Models;

namespace Microsoft.Examples.Configuration
{
    public class Ten99ModelConfiguration : IModelConfiguration
    {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion )
        {
            if ( apiVersion < ApiVersions.V4 )
            {
                return;
            }

            var trust = builder.EntityType<TrustModel>();//.HasKey( e => e.TrustKey );
            trust.HasKey( e => new { e.TrustKey, e.YrSeq, e.PermSeq } );
            //trust.Name = "Account";

            var trustPerm = builder.EntityType<AccountDetails>();//.HasKey( e => e.TrustKey );
            trustPerm.HasKey( e => new { e.TrustKey, e.PermSeq } );
            //trustPerm.Name = "AccountDetails";

            trust.ContainsOptional( t => t.Details );//.Name = "account_details";
            //trust.Ignore( t => t.TrustKey );
            //trust.Ignore( t => t.PermSeq );
            //trust.Property( t => t.YrSeq ).Name = "yr_seq";


            //trustPerm.Ignore( t => t.TrustKey );
            //trustPerm.Ignore( t => t.PermSeq );
            //trustPerm.Filter()
            //    .Count()
            //    .Expand()
            //    .OrderBy()
            //    .Page()
            //    .Select();

            var ten99b = builder.EntitySet<Ten99BModel>( "Ten99B" ).EntityType;
            ten99b.HasKey( o => new { o.YrSeq, o.Pan } );
            //ten99b.HasKey( o => o.YrSeq );
            //ten99b.HasKey( o => o.Pan );

            ten99b.ContainsOptional( t => t.Account );
            //ten99b.Name = "account";
                                                      //ten99b.Ignore( t => t.TrustKey );
                                                      //ten99b.Property( t => t.YrSeq ).Name = "yr_seq";
            //ten99b.Filter()
            //    .Count()
            //    .Expand()
            //    .OrderBy()
            //    .Page()
            //    .Select();

            var ten99div = builder.EntitySet<Ten99DivModel>( "Ten99Div" ).EntityType;
            ten99div.HasKey( o => new { o.YrSeq, o.Pan } );
            //ten99div.HasKey( o => o.YrSeq );
            //ten99div.HasKey( o => o.Pan );


            //ten99div.Ignore( t => t.TrustKey );
            //ten99div.Property( t => t.YrSeq ).Name = "yr_seq";
            ten99div.ContainsOptional( t => t.Account );//.Name = "account";
            //ten99div.Filter()
            //    .Count()
            //    .Expand()
            //    .OrderBy()
            //    .Page()
            //    .Select();

        }
    }
}