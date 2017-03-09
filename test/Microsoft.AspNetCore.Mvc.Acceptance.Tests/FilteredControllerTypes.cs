namespace Microsoft.AspNetCore.Mvc
{
    using Controllers;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    sealed class FilteredControllerTypes : ControllerFeatureProvider, ICollection<TypeInfo>
    {
        readonly HashSet<TypeInfo> controllerTypes = new HashSet<TypeInfo>();

        protected override bool IsController( TypeInfo typeInfo ) => base.IsController( typeInfo ) && controllerTypes.Contains( typeInfo );

        public int Count => controllerTypes.Count;

        public bool IsReadOnly => false;

        public void Add( TypeInfo item ) => controllerTypes.Add( item );

        public void Clear() => controllerTypes.Clear();

        public bool Contains( TypeInfo item ) => controllerTypes.Contains( item );

        public void CopyTo( TypeInfo[] array, int arrayIndex ) => controllerTypes.CopyTo( array, arrayIndex );

        public IEnumerator<TypeInfo> GetEnumerator() => controllerTypes.GetEnumerator();

        public bool Remove( TypeInfo item ) => controllerTypes.Remove( item );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}