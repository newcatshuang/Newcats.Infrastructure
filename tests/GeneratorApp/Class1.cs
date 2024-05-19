using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GeneratorApp
{
    /// <summary>
    /// 依赖注入特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public class DIAttribute : Attribute
    {

    }

    [Generator]
    public class DISourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // new NotImplementedException();
            const string attributeName = "GeneratorApp.DIAttribute";



            var nodesAutoInject = context.SyntaxProvider.ForAttributeWithMetadataName(
     attributeName,
    (node, _) => true,
     (syntaxContext, _) => syntaxContext.TargetNode).Collect();

            IncrementalValueProvider<(Compilation, ImmutableArray<SyntaxNode>)> compilationAndTypesInject =
                context.CompilationProvider.Combine(nodesAutoInject);
        }
    }
}