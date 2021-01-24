using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OMyEF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace OMyEFTests
{
    public class GeneratorTests
    {
        [Fact]
        public void InitialTest()
        {
            Compilation inputCompilation = CreateCompilation(@"
                using System;
                using OMyEF.Db;
                using System.Collections.Generic;
                using Microsoft.EntityFrameworkCore;
                using System.ComponentModel.DataAnnotations;
                namespace GeneratorTests
                {
                    public class Program
                    {
                        public static void Main(string[] args)
                        {
                        }
                    }
                    public class Authentication
                    {
                        [Key]
                        public short AuthenticationId { get; set; }
                        public string Name { get; set; }
                    }
                    public partial class GeneratorFakeContext 
                    {
                        [GenerateODataController(KeyName = ""AuthenticationId"", KeyType = ""int"")]
                        public DbSet<Authentication> Authentication { get; set; }

                    }
                }
            ");
            
            ODataGenerator generator = new ODataGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            var refAss = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.True(diagnostics.IsEmpty);
            Assert.Equal(2, outputCompilation.SyntaxTrees.Count());
            var outputDiagnostics = outputCompilation.GetDiagnostics();
            List<Diagnostic> unexpectedDiagnostics = new List<Diagnostic>();
            foreach(var diagnostic in diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case "CS1701": // wrong dll version warning
                    case "CS8019": // unnecessary using warning
                        break;
                    default:
                        unexpectedDiagnostics.Add(diagnostic);
                        break;
                }
            }
            Assert.True(unexpectedDiagnostics.Count == 0);

            var runResult = driver.GetRunResult();
            Assert.Single(runResult.GeneratedTrees);
            Assert.Empty(runResult.Diagnostics);
        }

        private IEnumerable<MetadataReference> GetMetadataReferences()
        {
            List<MetadataReference> references = new List<MetadataReference>();
            references.Add(MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location));
            foreach(var ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!ass.IsDynamic)
                {
                    string codeBase = ass.Location;
                    UriBuilder uri = new UriBuilder(codeBase);
                    string path = Uri.UnescapeDataString(uri.Path);
                    references.Add(MetadataReference.CreateFromFile(path));
                }
            }
            return references;
        }
        private Compilation CreateCompilation(string source)
        {
            return CSharpCompilation.Create("compilation",
                           new[] { CSharpSyntaxTree.ParseText(source) },
                           GetMetadataReferences(),
                           new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        }
    }
}
