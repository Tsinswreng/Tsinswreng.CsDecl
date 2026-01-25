using System;
using Microsoft.CodeAnalysis.CSharp;

class TestNamespaceHandling {
    static void Main() {
        // Test case 1: Code with both outer and inner using directives
        string testCode = @"
using System;
using System.Collections.Generic;

namespace OuterNamespace {
    using System.Linq;
    using System.Text;

    namespace InnerNamespace {
        using System.IO;

        public class TestClass {
            public void TestMethod() {
                Console.WriteLine(""Hello World"");
            }
        }
    }
}
";

        // Test with RetainUsingNamespace = false (should remove all using directives)
        var processor = new Tsinswreng.CsDecl.DeclProcessor(new Tsinswreng.CsDecl.Opt {
            RetainUsingNamespace = false,
            RetainUsingTypeAlias = true
        });

        string result = processor.ProcessSourceCode(testCode);
        Console.WriteLine("=== Test with RetainUsingNamespace = false ===");
        Console.WriteLine(result);
        Console.WriteLine();

        // Test with RetainUsingNamespace = true (should keep all using directives)
        processor = new Tsinswreng.CsDecl.DeclProcessor(new Tsinswreng.CsDecl.Opt {
            RetainUsingNamespace = true,
            RetainUsingTypeAlias = true
        });

        result = processor.ProcessSourceCode(testCode);
        Console.WriteLine("=== Test with RetainUsingNamespace = true ===");
        Console.WriteLine(result);
    }
}