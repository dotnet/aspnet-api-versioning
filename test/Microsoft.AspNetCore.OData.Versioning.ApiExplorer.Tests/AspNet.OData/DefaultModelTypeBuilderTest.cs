namespace Microsoft.AspNet.OData
{
    using FluentAssertions;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class DefaultModelTypeBuilderTest
    {
        [Theory]
        [MemberData( nameof( SubstitutionNotRequiredData ) )]
        public void substituted_type_should_be_same_as_original_type( Type originalType )
        {
            // arrange
            var modelBuilder = new ODataConventionModelBuilder();

            modelBuilder.EntitySet<Contact>( "Contacts" );
            modelBuilder.EntityType<Address>();

            var context = NewContext( modelBuilder.GetEdmModel() );

            // act
            var substitutedType = originalType.SubstituteIfNecessary( context );

            // assert
            substitutedType.Should().Be( originalType );
        }

        [Theory]
        [MemberData( nameof( SubstitutionData ) )]
        public void type_should_match_edm_with_top_entity_substitution( Type originalType )
        {
            // arrange
            var modelBuilder = new ODataConventionModelBuilder();
            var contact = modelBuilder.EntitySet<Contact>( "Contacts" ).EntityType;

            contact.Ignore( p => p.Email );
            contact.Ignore( p => p.Phone );
            contact.Ignore( p => p.Addresses );

            var context = NewContext( modelBuilder.GetEdmModel() );

            // act
            var substitutedType = originalType.SubstituteIfNecessary( context );

            // assert
            substitutedType.Should().NotBe( originalType );

            var innerType = substitutedType.GetGenericArguments()[0];

            innerType.GetRuntimeProperties().Should().HaveCount( 3 );
            innerType.Should().HaveProperty<int>( nameof( Contact.ContactId ) );
            innerType.Should().HaveProperty<string>( nameof( Contact.FirstName ) );
            innerType.Should().HaveProperty<string>( nameof( Contact.LastName ) );
        }

        [Fact]
        public void type_should_match_edm_with_nested_entity_substitution()
        {
            // arrange
            var modelBuilder = new ODataConventionModelBuilder();
            var contact = modelBuilder.EntitySet<Contact>( "Contacts" ).EntityType;

            contact.Ignore( p => p.Email );
            contact.Ignore( p => p.Phone );
            contact.Ignore( p => p.Addresses );

            var context = NewContext( modelBuilder.GetEdmModel() );
            var originalType = typeof( ODataValue<IEnumerable<Contact>> );

            // act
            var substitutedType = originalType.SubstituteIfNecessary( context );

            // assert
            substitutedType.Should().NotBe( originalType );

            var innerType = substitutedType.GetGenericArguments()[0].GetGenericArguments()[0];

            innerType.GetRuntimeProperties().Should().HaveCount( 3 );
            innerType.Should().HaveProperty<int>( nameof( Contact.ContactId ) );
            innerType.Should().HaveProperty<string>( nameof( Contact.FirstName ) );
            innerType.Should().HaveProperty<string>( nameof( Contact.LastName ) );
        }

        [Theory]
        [MemberData( nameof( SubstitutionData ) )]
        public void type_should_match_edm_with_child_entity_substitution( Type originalType )
        {
            // arrange
            var modelBuilder = new ODataConventionModelBuilder();

            modelBuilder.EntitySet<Contact>( "Contacts" );
            modelBuilder.EntityType<Address>().Ignore( a => a.IsoCode );

            var context = NewContext( modelBuilder.GetEdmModel() );

            // act
            var substitutedType = originalType.SubstituteIfNecessary( context );

            // assert
            substitutedType.Should().NotBe( originalType );

            var nextType = substitutedType.GetGenericArguments()[0];

            nextType.GetRuntimeProperties().Should().HaveCount( 6 );
            nextType.Should().HaveProperty<int>( nameof( Contact.ContactId ) );
            nextType.Should().HaveProperty<string>( nameof( Contact.FirstName ) );
            nextType.Should().HaveProperty<string>( nameof( Contact.LastName ) );
            nextType.Should().HaveProperty<string>( nameof( Contact.Email ) );
            nextType.Should().HaveProperty<string>( nameof( Contact.Phone ) );
            nextType = nextType.GetRuntimeProperties().Single( p => p.Name == nameof( Contact.Addresses ) ).PropertyType.GetGenericArguments()[0];
            nextType.GetRuntimeProperties().Should().HaveCount( 5 );
            nextType.Should().HaveProperty<int>( nameof( Address.AddressId ) );
            nextType.Should().HaveProperty<string>( nameof( Address.Street ) );
            nextType.Should().HaveProperty<string>( nameof( Address.City ) );
            nextType.Should().HaveProperty<string>( nameof( Address.State ) );
            nextType.Should().HaveProperty<string>( nameof( Address.Zip ) );
        }

        [Fact]
        public void substitute_should_generate_type_for_action_parameters()
        {
            // arrange
            var modelBuilder = new ODataConventionModelBuilder();
            var contact = modelBuilder.EntitySet<Contact>( "Contacts" ).EntityType;
            var action = contact.Action( "MarkContacted" );

            action.Parameter<DateTime>( "when" );
            action.Parameter<string>( "contactedBy" );
            action.Parameter<bool>( "callbackRequired" );

            var context = NewContext( modelBuilder.GetEdmModel() );
            var model = context.Model;
            var qualifiedName = $"{model.EntityContainer.Namespace}.{action.Name}";
            var operation = (IEdmAction) model.FindDeclaredOperations( qualifiedName ).Single();

            // act
            var substitutionType = context.ModelTypeBuilder.NewActionParameters( operation, ApiVersion.Default );

            // assert
            substitutionType.GetRuntimeProperties().Should().HaveCount( 3 );
            substitutionType.Should().HaveProperty<DateTimeOffset>( "when" );
            substitutionType.Should().HaveProperty<string>( "contactedBy" );
            substitutionType.Should().HaveProperty<bool>( "callbackRequired" );
        }

        public static IEnumerable<object[]> SubstitutionNotRequiredData
        {
            get
            {
                yield return new object[] { typeof( IEnumerable<string> ) };
                yield return new object[] { typeof( IEnumerable<Contact> ) };
                yield return new object[] { typeof( ODataValue<IEnumerable<Contact>> ) };
                yield return new object[] { typeof( Delta<Contact> ) };
            }
        }

        public static IEnumerable<object[]> SubstitutionData
        {
            get
            {
                yield return new object[] { typeof( IEnumerable<Contact> ) };
                yield return new object[] { typeof( ODataValue<Contact> ) };
                yield return new object[] { typeof( Delta<Contact> ) };
            }
        }

        static TypeSubstitutionContext NewContext( IEdmModel model )
        {
            var assemblies = new[] { typeof( DefaultModelTypeBuilderTest ).Assembly };
            return new TypeSubstitutionContext( model, assemblies, new DefaultModelTypeBuilder( assemblies ) );
        }
    }
}