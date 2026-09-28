namespace Generator.Wasm
{
    using HexaGen;
    using HexaGen.Core.CSharp;
    using HexaGen.CppAst.Model.Declarations;
    using HexaGen.CppAst.Model.Types;
    using HexaGen.GenerationSteps;
    using HexaGen.Metadata;
    using System.Text;

    /// <summary>
    /// Function step that also records, per exported C function, the Mono browser-wasm signature cookie of its native call.
    /// </summary>
    /// <remarks>
    /// The cookies land in the <see cref="MetadataKey"/> metadata entry, which merges with the rest of the metadata
    /// (internals, manual functions). <see cref="Targets.WasmTarget"/> turns them into InterpToNativeSignatures.cs.
    /// </remarks>
    public class WasmSignatureGenerationStep : FunctionGenerationStep
    {
        public const string MetadataKey = "WasmSignatures";

        private readonly Dictionary<string, string> signatures = [];

        public WasmSignatureGenerationStep(CsCodeGenerator generator, CsCodeGeneratorConfig config) : base(generator, config)
        {
        }

        protected override void GenerateVariations(CppFunction cppFunction, CsFunctionOverload overload)
        {
            signatures[cppFunction.Name] = WasmSignature.GetCookie(cppFunction);
            base.GenerateVariations(cppFunction, overload);
        }

        public override void CopyToMetadata(CsCodeGeneratorMetadata metadata)
        {
            base.CopyToMetadata(metadata);
            metadata.GetOrCreate<MetadataDictionaryEntry<string, string>>(MetadataKey).CopyFrom(signatures);
        }

        public override void Reset()
        {
            base.Reset();
            signatures.Clear();
        }
    }

    /// <summary>
    /// Maps C function types to the signature cookies of Mono's WasmAppBuilder (src/tasks/WasmAppBuilder/SignatureMapper.cs).
    /// </summary>
    public static class WasmSignature
    {
        /// <summary>
        /// Returns the cookie: return char then one char per parameter; I = 32-bit int or pointer, L = 64-bit int, F = float,
        /// D = double, V = void. A struct returned by value travels through a pointer in slot 0 ("VI" prefix) and a struct
        /// argument through a pointer ("I"); a struct with a single field is passed as that field.
        /// </summary>
        public static string GetCookie(CppFunction function)
        {
            StringBuilder cookie = new();
            char result = Classify(function.ReturnType);
            cookie.Append(result == 'S' ? "VI" : result.ToString());
            foreach (CppParameter parameter in function.Parameters)
            {
                char c = Classify(parameter.Type);
                cookie.Append(c == 'S' ? 'I' : c);
            }
            return cookie.ToString();
        }

        private static char Classify(CppType type)
        {
            switch (type)
            {
                case CppQualifiedType qualified:
                    return Classify(qualified.ElementType);

                case CppTypedef typedef:
                    // Pointer-sized on wasm32, whatever the host the headers were parsed on uses.
                    return typedef.Name is "size_t" or "ptrdiff_t" or "intptr_t" or "uintptr_t" ? 'I' : Classify(typedef.ElementType);

                case CppPointerType or CppReferenceType or CppArrayType or CppFunctionType:
                    return 'I';

                case CppEnum @enum:
                    return Classify(@enum.IntegerType);

                case CppPrimitiveType primitive:
                    return primitive.Kind switch
                    {
                        CppPrimitiveKind.Void => 'V',
                        CppPrimitiveKind.Float => 'F',
                        CppPrimitiveKind.Double => 'D',
                        CppPrimitiveKind.LongLong or CppPrimitiveKind.UnsignedLongLong => 'L',
                        CppPrimitiveKind.LongDouble => throw new NotSupportedException("long double has no wasm signature char"),
                        _ => 'I', // bool, chars, shorts, ints; long is 32-bit on wasm32.
                    };

                case CppClass @class:
                    var fields = @class.Fields.Where(f => f.StorageQualifier != CppStorageQualifier.Static).ToList();
                    return fields.Count == 1 ? Classify(fields[0].Type) : 'S';

                default:
                    throw new NotSupportedException($"type '{type}' ({type.GetType().Name}) in a native signature");
            }
        }
    }
}
