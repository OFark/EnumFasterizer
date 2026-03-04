using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Text;

namespace EnumFasterizer
{
    [Generator]
    public class MakeEnumsGoFaster : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Create a provider for enum declarations
            var enumDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is EnumDeclarationSyntax,
                    transform: static (ctx, _) => GetEnumReceiver(ctx))
                .Where(static m => m is not null);

            // Register source output
            context.RegisterSourceOutput(enumDeclarations, static (spc, enumReceiver) =>
            {
                if (enumReceiver is null)
                    return;

                StringBuilder source = new($$"""
using System;
#pragma warning disable IDE0130
namespace {{enumReceiver.Namespace}};
#pragma warning restore IDE0130

{{enumReceiver.Accessibility}} static class {{enumReceiver.EnumClass}}
{
    public static string FastToString(this {{enumReceiver.EnumName}} e)
    {
        return e switch
        {

""");
            foreach (var member in enumReceiver.Members)
            {
                source.Append(
$"""
            {enumReceiver.EnumName}.{member} => nameof({enumReceiver.EnumName}.{member}),

""");
            }
            
            source.Append("""
            _ => throw new ArgumentOutOfRangeException(nameof(e), e, null)
        };
    }
}
""");
                spc.AddSource($"{enumReceiver.EnumName}_fasterizer.cs", SourceText.From(source.ToString(), Encoding.UTF8));
            });
        }

        private static EnumReceiver? GetEnumReceiver(GeneratorSyntaxContext context)
        {
            var enumDeclarationSyntax = (EnumDeclarationSyntax)context.Node;
            return new EnumReceiver(enumDeclarationSyntax, context.SemanticModel);
        }

        class EnumReceiver
        {
            public EnumReceiver(EnumDeclarationSyntax enumDeclarationSyntax, SemanticModel semanticModel)
            {
                EnumName = enumDeclarationSyntax.Identifier.Text;
                EnumClass = $"{EnumName}_Extension";
                var parent = enumDeclarationSyntax.Parent;

                Accessibility = semanticModel.GetDeclaredSymbol(enumDeclarationSyntax)?.DeclaredAccessibility.ToString().ToLower() ?? "";

                while (parent is ClassDeclarationSyntax classDeclaration)
                {
                    EnumName = $"{classDeclaration.Identifier.Text}.{EnumName}";
                    parent = classDeclaration.Parent;
                    Accessibility = semanticModel.GetDeclaredSymbol(classDeclaration)?.DeclaredAccessibility.ToString().ToLower() ?? "";
                }

                if (parent is NamespaceDeclarationSyntax namespaceDeclaration)
                {
                    Namespace = namespaceDeclaration.Name.ToString();
                }
                else if (parent is FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax)
                {
                    Namespace = fileScopedNamespaceDeclarationSyntax.Name.ToString();
                }
                else
                {
                    Namespace = "EnumFasterizer";
                }

                foreach (var x in enumDeclarationSyntax.Members)
                {
                    Members.Add(x.Identifier.Text);
                }
            }

            public string Accessibility { get; private set; }
            public List<string> Members { get; } = [];
            public string EnumName { get; private set; }
            public string Namespace { get; private set; }
            public string EnumClass { get; private set; } 
        }
    }
}
