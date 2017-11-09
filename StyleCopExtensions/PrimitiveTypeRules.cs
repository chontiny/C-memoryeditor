namespace StyleCopExtensions
{
    using StyleCop;
    using StyleCop.CSharp;
    using System;
    using System.Diagnostics;

    /// <summary>
    /// A custom stylecop rule to enforce use of explicit type names.
    /// </summary>
    [SourceAnalyzer(typeof(CsParser))]
    public partial class PrimitiveTypeRules : SourceAnalyzer
    {
        /// <summary>
        /// The built-in type aliases for C#.
        /// </summary>
        private static readonly String[][] BuiltInTypes = new String[][]
        {
            new[] { "Boolean", "System.Boolean", "bool" },
            new[] { "Object", "System.Object", "object" },
            new[] { "String", "System.String", "string" },
            new[] { "Int16", "System.Int16", "short" },
            new[] { "UInt16", "System.UInt16", "ushort" },
            new[] { "Int32", "System.Int32", "int" },
            new[] { "UInt32", "System.UInt32", "uint" },
            new[] { "Int64", "System.Int64", "long" },
            new[] { "UInt64", "System.UInt64", "ulong" },
            new[] { "Double", "System.Double", "double" },
            new[] { "Single", "System.Single", "float" },
            new[] { "Byte", "System.Byte", "byte" },
            new[] { "SByte", "System.SByte", "sbyte" },
            new[] { "Char", "System.Char", "char" },
            new[] { "Decimal", "System.Decimal", "decimal" }
        };

        /// <summary>
        /// Checks the methods within the given document.
        /// </summary>
        /// <param name="document">
        /// The document to check.
        /// </param>
        public override void AnalyzeDocument(CodeDocument document)
        {
            Param.RequireNotNull(document, "document");

            CsDocument csdocument = (CsDocument)document;

            if (csdocument.RootElement != null && !csdocument.RootElement.Generated)
            {
                // Checks the usage of the built-in types and empty strings.
                this.IterateTokenList(csdocument);
            }
        }

        /// <summary>
        /// Checks the built-in types and empty strings within a document.
        /// </summary>
        /// <param name="document">The document containing the tokens.</param>
        private void IterateTokenList(CsDocument document)
        {
            Param.AssertNotNull(document, "document");

            for (Node<CsToken> tokenNode = document.Tokens.First; tokenNode != null; tokenNode = tokenNode.Next)
            {
                CsToken token = tokenNode.Value;

                if (token.CsTokenClass == CsTokenClass.Type || token.CsTokenClass == CsTokenClass.GenericType)
                {
                    // Check that the type is using the built-in types, if applicable.
                    this.CheckBuiltInType(tokenNode, document);
                }
            }
        }

        /// <summary>
        /// Checks a type to determine whether it should use one of the built-in types.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="document">The parent document.</param>
        private void CheckBuiltInType(Node<CsToken> type, CsDocument document)
        {
            Param.AssertNotNull(type, "type");
            Param.AssertNotNull(document, "document");

            Debug.Assert(type.Value is TypeToken, "The type must be a TypeToken");
            TypeToken typeToken = (TypeToken)type.Value;

            if (type.Value.CsTokenClass != CsTokenClass.GenericType)
            {
                for (Int32 i = 0; i < PrimitiveTypeRules.BuiltInTypes.Length; ++i)
                {
                    String[] builtInType = PrimitiveTypeRules.BuiltInTypes[i];

                    if (CsTokenList.MatchTokens(typeToken.ChildTokens.First, builtInType[2]))
                    {
                        this.AddViolation(typeToken.FindParentElement(), typeToken.LineNumber, nameof(PrimitiveTypeRules), builtInType[0], builtInType[1], builtInType[2]);

                        break;
                    }
                }
            }

            for (Node<CsToken> childToken = typeToken.ChildTokens.First; childToken != null; childToken = childToken.Next)
            {
                if (childToken.Value.CsTokenClass == CsTokenClass.Type || childToken.Value.CsTokenClass == CsTokenClass.GenericType)
                {
                    this.CheckBuiltInType(childToken, document);
                }
            }
        }
    }
}
