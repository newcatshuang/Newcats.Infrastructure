using System.Collections.Generic;
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
    /// Generates the source code for the dependency injection
    /// </summary>
    [Generator(LanguageNames.CSharp)]
    public class DependencyInjectionGenerator : IIncrementalGenerator
    {
        private const string ATTRIBUTENAME = "GeneratorApp.DependencyInjectionAttribute";//特性名

        /// <summary>
        /// 跳过的程序集
        /// </summary>
        private const string SKIPASSEMBLIES = "^System|^Mscorlib|^Netstandard|^Microsoft|^Autofac|^AutoMapper|^EntityFramework|^Newtonsoft|^Castle|^NLog|^Pomelo|^AspectCore|^Xunit|^Nito|^Npgsql|^Exceptionless|^MySqlConnector|^Anonymously Hosted|^libuv|^api-ms|^clrcompression|^clretwrc|^clrjit|^coreclr|^dbgshim|^e_sqlite3|^hostfxr|^hostpolicy|^MessagePack|^mscordaccore|^mscordbi|^mscorrc|sni|sos|SOS.NETCore|^sos_amd64|^SQLitePCLRaw|^StackExchange|^Swashbuckle|WindowsBase|ucrtbase|^log4net|^Dapper|^IdentityServer|^IdentityModel|^ICSharpCode|^NPOI";

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
                ATTRIBUTENAME,
                (node, _) => node is ClassDeclarationSyntax,
                (syntaxContext, _) => (ClassDeclarationSyntax)syntaxContext.TargetNode
                )
                .Where(f => f != null);

            var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

            context.RegisterSourceOutput(
                compilationAndClasses,
                (spc, source) => Execute(source.Left, source.Right, spc));
        }

        /// <summary>
        /// 查找带指定特性的类
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="classes"></param>
        /// <param name="context"></param>
        private static void Execute(Compilation compilation, IEnumerable<ClassDeclarationSyntax> classes, SourceProductionContext context)
        {
            var classesToRegister = new List<RegisterDependencyModel>();//要注册的类
            foreach (var classSyntax in classes)
            {
                var semanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
                var classSymbol = semanticModel.GetDeclaredSymbol(classSyntax);
                if (classSymbol == null)
                    continue;

                var dependencyAttribute = classSymbol.GetAttributes().FirstOrDefault(f => f.AttributeClass?.ToDisplayString() == ATTRIBUTENAME);
                if (dependencyAttribute == null)
                    continue;

                var lifetime = (LifetimeEnum)dependencyAttribute.ConstructorArguments[0].Value;//特性标记的生命周期
                var interfaces = classSymbol.Interfaces;//实现类继承的所有接口(AllInterfaces会往上查询接口的父级接口，这里只注册直接实现的接口)

                if (interfaces.Any())
                {
                    foreach (var iface in interfaces)
                    {
                        if (!Regex.IsMatch(iface.ToDisplayString(), SKIPASSEMBLIES, RegexOptions.IgnoreCase | RegexOptions.Compiled))//跳过系统和常用开源程序集的接口，不生成注册代码
                            classesToRegister.Add(new RegisterDependencyModel(iface, classSymbol, lifetime));//有接口，就注册接口和实现类
                    }
                }
                else
                {
                    classesToRegister.Add(new RegisterDependencyModel(null, classSymbol, lifetime));//没有接口，就注册实现类自己
                }
            }

            if (classesToRegister.Count > 0)
            {
                GenerateSource(classesToRegister, context);//生成源代码
            }
        }

        /// <summary>
        /// 生成源代码
        /// </summary>
        /// <param name="classesToRegister"></param>
        /// <param name="context"></param>
        private static void GenerateSource(List<RegisterDependencyModel> models, SourceProductionContext context)
        {
            var sourceBuilder = new StringBuilder(@"
using Microsoft.Extensions.DependencyInjection;

public static class GeneratorDependencyInjectionExtensions
{
    public static IServiceCollection AddGeneratedDependencies(this IServiceCollection services)
    {
");

            foreach (var classToRegister in models)
            {
                if (classToRegister.InterfaceType != null)
                {
                    sourceBuilder.AppendLine($"        services.Add{classToRegister.Lifetime}<{classToRegister.InterfaceType}, {classToRegister.ImplementationType}>();");
                }
                else
                {
                    sourceBuilder.AppendLine($"        services.Add{classToRegister.Lifetime}<{classToRegister.ImplementationType}>();");
                }
            }

            sourceBuilder.Append(@"
        return services;
    }
}");

            context.AddSource("NewcatsDependencyInjectionExtensions.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
    }
}