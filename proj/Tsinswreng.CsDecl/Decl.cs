using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tsinswreng.CsDecl;

public static class DeclProcessor {
	public static string ProcessSourceCode(string sourceCode) {
		// Parse the source code into a syntax tree
		var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
		var root = syntaxTree.GetRoot();

		// Create a rewriter to modify the code
		var rewriter = new DeclRewriter();
		var newRoot = rewriter.Visit(root);

		// Convert back to string
		return newRoot.ToFullString();
	}

	private class DeclRewriter : CSharpSyntaxRewriter {
		public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) {
			// Change namespace A; to namespace A{ ... }
			if (node.SemicolonToken.IsKind(SyntaxKind.SemicolonToken)) {
				// File-scoped namespace: replace semicolon with opening brace and add closing brace
				var newNamespace = node
					.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
					.WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
					.WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken));

				return newNamespace;
			}
			return node;
		}

		public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node) {
			// Remove method body, keep only the declaration
			if (node.Body != null) {
				return node.WithBody(null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
			}
			return node;
		}

		public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node) {
			// Remove constructor body, keep only the declaration
			if (node.Body != null) {
				return node.WithBody(null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
			}
			return node;
		}
	}
}

public class DeclService {
	public void ProcessProject(string csprojPath, string outputDir) {
		// Initialize MSBuild
		Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();

		using var workspace = Microsoft.CodeAnalysis.MSBuild.MSBuildWorkspace.Create();
		var project = workspace.OpenProjectAsync(csprojPath).Result;
		var compilation = project.GetCompilationAsync().Result;

		if (compilation == null) {
			throw new Exception("Failed to get compilation");
		}

		var projectDir = Path.GetDirectoryName(csprojPath);
		if (projectDir == null) {
			throw new Exception("Invalid project path");
		}

		// Process each document
		foreach (var document in project.Documents) {
			var sourceCode = document.GetTextAsync().Result.ToString();
			var processedCode = DeclProcessor.ProcessSourceCode(sourceCode);

			// Calculate relative path from project directory
			var documentPath = document.FilePath;
			var relativePath = Path.GetRelativePath(projectDir, documentPath);

			// Create output path
			var outputPath = Path.Combine(outputDir, relativePath);
			var outputFileDir = Path.GetDirectoryName(outputPath);

			if (outputFileDir != null && !Directory.Exists(outputFileDir)) {
				Directory.CreateDirectory(outputFileDir);
			}

			File.WriteAllText(outputPath, processedCode);
		}
	}
}
