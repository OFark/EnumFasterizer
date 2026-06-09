using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EnumFasterizer;

[Generator]
public class MakeEnumsGoFaster : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Create a provider for enum declarations
        var enumDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is EnumDeclarationSyntax,
                transform: static (ctx, cancellation) => GetEnumReceiver(ctx, cancellation))
            .Where(static m => m is not null);

        // Register source output
        context.RegisterSourceOutput(enumDeclarations, static (spc, enumReceiver) =>
        {
            if (enumReceiver is null)
                return;

            StringBuilder source = new($$"""
using System;
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace {{enumReceiver.Namespace}};
#pragma warning restore IDE0130
#pragma warning restore IDE0079

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0036:Modifiers are not ordered", Justification = "Different Orders for different accessibilities intruduces too much complexity")]
{{enumReceiver.Accessibility}} static class {{enumReceiver.EnumClass}}
{
    {{enumReceiver.Accessibility}} static string FastToString(this {{enumReceiver.EnumName}} e)
    {
        return e switch
        {

""");
            foreach (var member in enumReceiver.Members)
            {
                if (member.IsObsolete)
                {
                    source.AppendLine($"#pragma warning disable {member.ObsoleteLevel} // Obsolete member");
                }
                source.Append(
$"""
            {enumReceiver.EnumName}.{member.Name} => nameof({enumReceiver.EnumName}.{member.Name}),

""");
                if (member.IsObsolete)
                {
                    source.AppendLine($"#pragma warning restore {member.ObsoleteLevel} // Obsolete member");
                }
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

    private static EnumReceiver? GetEnumReceiver(GeneratorSyntaxContext context, CancellationToken cancellation = default)
    {
        var enumDeclarationSyntax = (EnumDeclarationSyntax)context.Node;

        var enumSymbol = context.SemanticModel.GetDeclaredSymbol(enumDeclarationSyntax, cancellation);

        if (enumSymbol is null)
        {
            return null;
        }

        if (enumSymbol.DeclaredAccessibility == Accessibility.Private)
        {
            return null;
        }

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

                var obsoleteAttribute = x.AttributeLists.SelectMany(a => a.Attributes).FirstOrDefault(a => a.Name.ToString().Contains("Obsolete"));
                if (obsoleteAttribute is not null)
                {
                    if (obsoleteAttribute.ArgumentList?.Arguments.Count > 0 && obsoleteAttribute.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
                        Members.Add(new Member(true, "CS0618", x.Identifier.Text));
                    else
                        Members.Add(new Member(true, "CS0612", x.Identifier.Text));

                }
                else
                {
                    Members.Add(new Member(false, null, x.Identifier.Text));
                }
            }
        }

        public string Accessibility { get; private set; }
        public List<Member> Members { get; } = [];
        public string EnumName { get; private set; }
        public string Namespace { get; private set; }
        public string EnumClass { get; private set; }
    }
}

internal readonly struct Member(bool isObsolete, string? obsoleteLevel, string name)
{
    public bool IsObsolete { get; } = isObsolete;
    public string? ObsoleteLevel { get; } = obsoleteLevel;
    public string Name { get; } = name;
}