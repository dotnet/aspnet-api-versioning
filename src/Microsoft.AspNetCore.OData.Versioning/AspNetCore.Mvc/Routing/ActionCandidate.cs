namespace Microsoft.AspNetCore.Mvc.Routing
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using System.Collections.Generic;
    using System.Diagnostics;
    using static Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource;

    [DebuggerDisplay( "{Action.DisplayName,nq}" )]
    sealed class ActionCandidate
    {
        internal ActionCandidate( ActionDescriptor action )
        {
            TotalParameterCount = action.Parameters.Count;

            var filteredParameters = new List<string>( TotalParameterCount );

            for ( var i = 0; i < TotalParameterCount; i++ )
            {
                var parameter = action.Parameters[i];

                if ( parameter.ParameterType.IsModelBound() )
                {
                    continue;
                }

                var bindingSource = parameter.BindingInfo?.BindingSource;

                if ( bindingSource != Custom && bindingSource != Path )
                {
                    continue;
                }

                filteredParameters.Add( parameter.Name );
            }

            Action = action;
            FilteredParameters = filteredParameters;
        }

        internal ActionDescriptor Action { get; }

        internal int TotalParameterCount { get; }

        internal IReadOnlyList<string> FilteredParameters { get; }
    }
}