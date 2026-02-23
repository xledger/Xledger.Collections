#if NETFRAMEWORK
namespace System.Runtime.CompilerServices {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
    public sealed class CollectionBuilderAttribute : Attribute {
        //
        // Summary:
        //     Initializes a new instance of System.Runtime.CompilerServices.CollectionBuilderAttribute
        //     that refers to the methodName method on the builderType type.
        //
        // Parameters:
        //   builderType:
        //     The type of the builder to use to construct the collection.
        //
        //   methodName:
        //     The name of the method on the builder to use to construct the collection.
        public CollectionBuilderAttribute(Type builderType, string methodName) {
            this.BuilderType = builderType;
            this.MethodName = methodName;
        }

        //
        // Summary:
        //     Gets the type of the builder to use to construct the collection.
        public Type BuilderType { get; }
        //
        // Summary:
        //     Gets the name of the method on the builder to use to construct the collection.
        public string MethodName { get; }
    }
}
#endif
