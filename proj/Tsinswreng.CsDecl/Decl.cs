//新
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

		// Convert back to string and format with tabs
		// var result = newRoot.ToFullString();
		// return ConvertToTabs(result);
		return newRoot.ToFullString();
	}

	private static string ConvertToTabs(string code) {
		// Replace leading spaces with tabs (assuming 4 spaces per indentation level)
		var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		for (int i = 0; i < lines.Length; i++) {
			if (string.IsNullOrWhiteSpace(lines[i])) {
				continue;
			}

			// Count leading spaces and replace with tabs
			int spaceCount = 0;
			while (spaceCount < lines[i].Length && lines[i][spaceCount] == ' ') {
				spaceCount++;
			}

			if (spaceCount > 0) {
				// Replace 4 spaces with 1 tab (standard indentation)
				int tabs = spaceCount / 4;
				lines[i] = new string('\t', tabs) + lines[i].Substring(spaceCount);
			}
		}

		return string.Join("\r\n", lines);
	}

	private class DeclRewriter : CSharpSyntaxRewriter {
		public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) {
			// Always ensure namespace uses block-scoped syntax with braces
			// If it's already a block-scoped namespace, just return it as-is
			if (node.OpenBraceToken.IsKind(SyntaxKind.OpenBraceToken) && node.CloseBraceToken.IsKind(SyntaxKind.CloseBraceToken)) {
				var newNamespace = node
					.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
					.WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
					.WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken));

				return newNamespace;
			}
			return node;
		}

		public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node) {
			// Convert file-scoped namespace (namespace X;) to block-scoped namespace (namespace X { ... })
			var openBraceToken = SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
				.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed); // Add newline after opening brace

			var newNamespace = SyntaxFactory.NamespaceDeclaration(
				node.AttributeLists,
				node.Modifiers,
				node.NamespaceKeyword,
				node.Name,
				openBraceToken,
				node.Externs,
				node.Usings,
				node.Members,
				SyntaxFactory.Token(SyntaxKind.CloseBraceToken),
				default // No semicolon token for block-scoped namespace
			);

			// Process each member individually to ensure method bodies are cleared
			var processedMembers = new List<MemberDeclarationSyntax>();
			foreach (var member in newNamespace.Members) {
				var processedMember = (MemberDeclarationSyntax)Visit(member);
				processedMembers.Add(processedMember);
			}

			return newNamespace.WithMembers(SyntaxFactory.List(processedMembers));
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
