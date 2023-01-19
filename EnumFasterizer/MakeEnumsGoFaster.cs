using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EnumFasterizer
{
    [Generator]
    public class MakeEnumsGoFaster : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
                return;

            foreach (var r in receiver.EnumRecievers)
            {

                StringBuilder source = new($@"using System;
namespace {r.Namespace}
{{
    {r.Accessibility} static class {r.EnumClass}
    {{
        #pragma warning disable CS0618
        public static string FastToString(this {r.EnumName} e)
        {{
            switch(e)
            {{");

                foreach (var member in r.Members)
                {
                    source.Append($@"
                case {r.EnumName}.{member}:
                    return nameof({r.EnumName}.{member});");
                }
                source.Append($@"
                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }}
        }}
        #pragma warning restore CS0618
    }}
}}");
                context.AddSource($"{r.EnumName}_fasterizer.cs", SourceText.From(source.ToString(), Encoding.UTF8));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        class SyntaxReceiver : ISyntaxContextReceiver
        {
            public List<EnumReceiver> EnumRecievers { get; } = new List<EnumReceiver>();

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is EnumDeclarationSyntax enumDeclarationSyntax)
                {
                    EnumRecievers.Add(new EnumReceiver(enumDeclarationSyntax, context.SemanticModel));
                }
            }
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
            public List<string> Members { get; } = new List<string>();
            public string EnumName { get; private set; }
            public string Namespace { get; private set; }
            public string EnumClass { get; private set; } 
        }
    }
}
