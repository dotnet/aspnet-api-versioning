namespace Microsoft.AspNet.OData
{
    using FluentAssertions;
    using FluentAssertions.Common;
    using System.Runtime.Serialization;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
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

        [Fact]
        public void substituted_type_should_be_extracted_from_parent_generic()
        {
            // arrange
            var modelBuilder = new ODataConventionModelBuilder();

            modelBuilder.EntitySet<Contact>( "Contacts" );
            modelBuilder.EntityType<Address>();

            var context = NewContext( modelBuilder.GetEdmModel() );
            var originalType = typeof( Delta<Contact> );

            // act
            var substitutedType = originalType.SubstituteIfNecessary( context );

            // assert
            substitutedType.Should().Be( typeof( Contact ) );
        }

        [Fact]
        public void type_should_be_match_edm_when_extracted_and_substituted_from_parent_generic()
        {
            // arrange
            var modelBuilder = new ODataConventionModelBuilder();
            var contact = modelBuilder.EntitySet<Contact>( "Contacts" ).EntityType;

            contact.Ignore( p => p.Email );
            contact.Ignore( p => p.Phone );
            contact.Ignore( p => p.Addresses );

            var context = NewContext( modelBuilder.GetEdmModel() );
            var originalType = typeof( Delta<Contact> );

            // act
            var substitutedType = originalType.SubstituteIfNecessary( context );

            // assert
            substitutedType.Should().NotBe( originalType );
            substitutedType.Should().NotBe( typeof( Contact ) );
            substitutedType.GetRuntimeProperties().Should().HaveCount( 3 );
            substitutedType.Should().HaveProperty<int>( nameof( Contact.ContactId ) );
            substitutedType.Should().HaveProperty<string>( nameof( Contact.FirstName ) );
            substitutedType.Should().HaveProperty<string>( nameof( Contact.LastName ) );
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

        [Fact]
        public void type_should_use_self_referencing_property_substitution()
        {
            // arrange
            var modelBuilder = new ODataConventionModelBuilder();
            var company = modelBuilder.EntitySet<Company>( "Companies" ).EntityType;

            company.Ignore( c => c.DateFounded );

            var context = NewContext( modelBuilder.GetEdmModel() );
            var originalType = typeof( Company );

            //act
            var subsitutedType = originalType.SubstituteIfNecessary( context );

            // assert
            subsitutedType.GetRuntimeProperties().Should().HaveCount( 4 );
            subsitutedType.Should().HaveProperty<int>( nameof( Company.CompanyId ) );
            subsitutedType.Should().HaveProperty<string>( nameof( Company.Name ) );
            subsitutedType.Should().Be( subsitutedType.GetRuntimeProperty( nameof( Company.ParentCompany ) ).PropertyType );
            subsitutedType.Should().Be( subsitutedType.GetRuntimeProperty( nameof( Company.Subsidiaries ) ).PropertyType.GetGenericArguments()[0] );
        }

        [Fact]
        public void type_should_use_back_referencing_property_substitution()
        {
            // arrange
            var modelBuilder = new ODataConventionModelBuilder();
            var employer = modelBuilder.EntitySet<Employer>( "Employers" ).EntityType;

            employer.Ignore( e => e.Birthday );

            var context = NewContext( modelBuilder.GetEdmModel() );
            var originalType = typeof( Employer );

            // act
            var substitutedType = originalType.SubstituteIfNecessary( context );

            // assert
            substitutedType.GetRuntimeProperties().Should().HaveCount( 4 );
            substitutedType.Should().HaveProperty<int>( nameof( Employer.EmployerId ) );
            substitutedType.Should().HaveProperty<string>( nameof( Employer.FirstName ) );
            substitutedType.Should().HaveProperty<string>( nameof( Employer.LastName ) );

            var employees = substitutedType.GetProperty( nameof( Employer.Employees ) ).PropertyType.GetGenericArguments()[0];

            substitutedType.Should().Be( employees.GetProperty( nameof( Employee.Employer ) ).PropertyType );
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
            nextType.Should().HaveProperty<Email>( nameof( Contact.Email ) );
            nextType.Should().HaveProperty<string>( nameof( Contact.Phone ) );
            nextType = nextType.GetRuntimeProperty( nameof( Contact.Addresses ) ).PropertyType.GetGenericArguments()[0];
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
            var services = new ServiceCollection();

            services.AddSingleton( model );

            // act
            var substitutionType = context.ModelTypeBuilder.NewActionParameters( services.BuildServiceProvider(), operation, ApiVersion.Default, contact.Name );

            // assert
            substitutionType.GetRuntimeProperties().Should().HaveCount( 3 );
            substitutionType.Should().HaveProperty<DateTimeOffset>( "when" );
            substitutionType.Should().HaveProperty<string>( "contactedBy" );
            substitutionType.Should().HaveProperty<bool>( "callbackRequired" );
        }

        [Fact]
        public void substitute_should_generate_type_for_action_parameters_with_substituted_types()
        {
            // arrange
            var modelBuilder = new ODataConventionModelBuilder();
            var contact = modelBuilder.EntitySet<Contact>( "Contacts" ).EntityType;

            contact.Ignore( c => c.Email );

            var action = contact.Action( "PlanInterview" );

            action.Parameter<DateTime>( "when" );
            action.Parameter<Contact>( "interviewer" );
            action.Parameter<Contact>( "interviewee" );

            var context = NewContext( modelBuilder.GetEdmModel() );
            var model = context.Model;
            var qualifiedName = $"{model.EntityContainer.Namespace}.{action.Name}";
            var operation = (IEdmAction) model.FindDeclaredOperations( qualifiedName ).Single();
            var services = new ServiceCollection();

            services.AddSingleton( model );

            // act
            var substitutionType = context.ModelTypeBuilder.NewActionParameters( services.BuildServiceProvider(), operation, ApiVersion.Default, contact.Name );

            // assert
            substitutionType.GetRuntimeProperties().Should().HaveCount( 3 );
            substitutionType.Should().HaveProperty<DateTimeOffset>( "when" );

            var contactType = substitutionType.GetRuntimeProperty( "interviewer" ).PropertyType;

            contactType.Should().Be( substitutionType.GetRuntimeProperty( "interviewee" ).PropertyType );
            contactType.GetRuntimeProperties().Should().HaveCount( 5 );
            contactType.Should().HaveProperty<int>( "ContactId" );
            contactType.Should().HaveProperty<string>( "FirstName" );
            contactType.Should().HaveProperty<string>( "LastName" );
            contactType.Should().HaveProperty<string>( "Phone" );
            contactType.Should().HaveProperty<List<Address>>( "Addresses" );
        }

        [Fact]
        public void substitute_should_generate_type_for_action_parameters_with_collection_parameters()
        {
            // arrange
            var modelBuilder = new ODataConventionModelBuilder();
            var contact = modelBuilder.EntitySet<Contact>( "Contacts" ).EntityType;
            var action = contact.Action( "PlanMeeting" );

            action.Parameter<DateTime>( "when" );
            action.CollectionParameter<Contact>( "attendees" );
            action.CollectionParameter<string>( "topics" );

            var context = NewContext( modelBuilder.GetEdmModel() );
            var model = context.Model;
            var qualifiedName = $"{model.EntityContainer.Namespace}.{action.Name}";
            var operation = (IEdmAction) model.FindDeclaredOperations( qualifiedName ).Single();
            var services = new ServiceCollection();

            services.AddSingleton( model );

            // act
            var substitutionType = context.ModelTypeBuilder.NewActionParameters( services.BuildServiceProvider(), operation, ApiVersion.Default, contact.Name );

            // assert
            substitutionType.GetRuntimeProperties().Should().HaveCount( 3 );
            substitutionType.Should().HaveProperty<DateTimeOffset>( "when" );
            substitutionType.Should().HaveProperty<IEnumerable<Contact>>( "attendees" );
            substitutionType.Should().HaveProperty<IEnumerable<string>>( "topics" );
        }

        [Fact]
        public void substitute_should_generate_types_for_actions_with_the_same_name_in_different_controllers()
        {
            // arrange
            var modelBuilder = new ODataConventionModelBuilder();
            var contact = modelBuilder.EntitySet<Contact>( "Contacts" ).EntityType;
            var action = contact.Action( "PlanMeeting" );

            action.Parameter<DateTime>( "when" );
            action.CollectionParameter<Contact>( "attendees" );
            action.CollectionParameter<string>( "topics" );

            var employee = modelBuilder.EntitySet<Employee>( "Employees" ).EntityType;

            action = employee.Action( "PlanMeeting" );
            action.Parameter<DateTime>( "when" );
            action.CollectionParameter<Employee>( "attendees" );
            action.Parameter<string>( "project" );

            var context = NewContext( modelBuilder.GetEdmModel() );
            var model = context.Model;
            var qualifiedName = $"{model.EntityContainer.Namespace}.{action.Name}";
            var operations = model.FindDeclaredOperations( qualifiedName ).Select( o => (IEdmAction) o ).ToArray();
            var services = new ServiceCollection();

            services.AddSingleton( model );

            // act
            var contactActionType = context.ModelTypeBuilder.NewActionParameters( services.BuildServiceProvider(), operations[0], ApiVersion.Default, contact.Name );
            var employeesActionType = context.ModelTypeBuilder.NewActionParameters( services.BuildServiceProvider(), operations[1], ApiVersion.Default, employee.Name );

            // assert
            contactActionType.Should().NotBe( employeesActionType );
            contactActionType.GetRuntimeProperties().Should().HaveCount( 3 );
            contactActionType.Should().HaveProperty<DateTimeOffset>( "when" );
            contactActionType.Should().HaveProperty<IEnumerable<Contact>>( "attendees" );
            contactActionType.Should().HaveProperty<IEnumerable<string>>( "topics" );

            employeesActionType.GetRuntimeProperties().Should().HaveCount( 3 );
            employeesActionType.Should().HaveProperty<DateTimeOffset>( "when" );
            employeesActionType.GetRuntimeProperty( "attendees" ).Should().NotBeNull();
            employeesActionType.Should().HaveProperty<string>( "project" );
        }

        [Fact]
        public void substitute_should_get_attributes_from_property_that_has_attributes_that_takes_params()
        {
            // arrange
            var modelBuilder = new ODataConventionModelBuilder();
            var employee = modelBuilder.EntitySet<Employee>( "Employees" ).EntityType;
            var originalType = typeof( Employee );

            employee.Ignore( e => e.FirstName );

            var context = NewContext( modelBuilder.GetEdmModel() );

            // act
            var substitutionType = originalType.SubstituteIfNecessary( context );

            // assert
            var property = substitutionType.GetRuntimeProperty( "Salary" );
            var attributeWithParams = property.GetCustomAttribute<AllowedRolesAttribute>();

            attributeWithParams.AllowedRoles.Should().BeEquivalentTo( new[] { "Manager", "Employer" } );
        }

        [Fact]
        public void substitute_should_resolve_types_that_reference_a_model_that_match_the_edm()
        {
            // arrange
            var modelBuilder = new ODataConventionModelBuilder();
            var shipment = modelBuilder.EntitySet<Shipment>( "Shipments" ).EntityType;
            var originalType = typeof( Shipment );
            var addressType = typeof( Address );

            shipment.Ignore( s => s.ShippedOn );
            modelBuilder.EntitySet<Address>( "Addresses" );

            var context = NewContext( modelBuilder.GetEdmModel() );

            // act
            addressType.SubstituteIfNecessary( context );

            var substitutionType = originalType.SubstituteIfNecessary( context );

            // assert
            substitutionType.Should().NotBeOfType<TypeBuilder>();
        }

        [Fact]
        public void substituted_type_should_have_renamed_with_attribute_properties_from_original_type()
        {
            // arrange
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Contact>( "Contacts" );

            var context = NewContext( modelBuilder.GetEdmModel() );
            var originalType = typeof( Contact );

            // act
            var substitutedType = originalType.SubstituteIfNecessary( context );

            // assert
            substitutedType.Should().HaveProperty<string>( nameof( Contact.FirstName ) );
            substitutedType.GetRuntimeProperty( nameof( Contact.FirstName ) ).Should().NotBeNull();
            substitutedType.GetRuntimeProperty( nameof( Contact.FirstName ) ).IsDecoratedWith<DataMemberAttribute>();
        }

        [Fact]
        public void substituted_type_should_keep_custom_attributes_on_dependency_property()
        {
            // arrange
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Contact>( "Contacts" );

            var context = NewContext( modelBuilder.GetEdmModel() );
            var originalType = typeof( Contact );

            // act
            var substitutedType = originalType.SubstituteIfNecessary( context );

            // assert
            substitutedType.GetRuntimeProperty( nameof( Contact.Email ) ).Should().NotBeNull();
            substitutedType.GetRuntimeProperty( nameof( Contact.Email ) ).IsDecoratedWith<DataMemberAttribute>();
        }

        [Fact]
        public void substituted_type_should_keep_custom_attributes_on_collection_dependency_property()
        {
            // arrange
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Contact>( "Contacts" );

            var context = NewContext( modelBuilder.GetEdmModel() );
            var originalType = typeof( Contact );

            // act
            var substitutedType = originalType.SubstituteIfNecessary( context );

            // assert
            substitutedType.GetRuntimeProperty( nameof( Contact.Addresses ) ).Should().NotBeNull();
            substitutedType.GetRuntimeProperty( nameof( Contact.Addresses ) ).IsDecoratedWith<DataMemberAttribute>();
        }

        public static IEnumerable<object[]> SubstitutionNotRequiredData
        {
            get
            {
                yield return new object[] { typeof( IEnumerable<string> ) };
                yield return new object[] { typeof( IEnumerable<Contact> ) };
                yield return new object[] { typeof( ODataValue<IEnumerable<Contact>> ) };
            }
        }

        public static IEnumerable<object[]> SubstitutionData
        {
            get
            {
                yield return new object[] { typeof( IEnumerable<Contact> ) };
                yield return new object[] { typeof( ODataValue<Contact> ) };
            }
        }

        static TypeSubstitutionContext NewContext( IEdmModel model )
        {
            return new TypeSubstitutionContext( model, new DefaultModelTypeBuilder() );
        }
    }
}