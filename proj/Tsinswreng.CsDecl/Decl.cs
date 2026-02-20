//新
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tsinswreng.CsDecl;

//配置類
public class Opt{
	/// 如 using System;
	public bool RetainUsingNamespace = false;
	/// 如 using i32 = System.Int32;
	public bool RetainUsingTypeAlias = true;
}

public class DeclProcessor {
	public Opt Opt { get; set; }

	public DeclProcessor(Opt opt = null) {
		Opt = opt ?? new Opt();
	}

	public string ProcessSourceCode(string sourceCode) {
		// Parse the source code into a syntax tree
		var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
		var root = syntaxTree.GetRoot();

		// Create a rewriter to modify the code
		var rewriter = new DeclRewriter(this);
		var newRoot = rewriter.Visit(root);

		// Convert back to string and format with tabs
		// var result = newRoot.ToFullString();
		// return ConvertToTabs(result);
		return newRoot.ToFullString();
	}

	private string ConvertToTabs(string code) {
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
}

internal class DeclRewriter : CSharpSyntaxRewriter {
	private readonly DeclProcessor _processor;

	public DeclRewriter(DeclProcessor processor) {
		_processor = processor;
	}

	public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) {
		// Always ensure namespace uses block-scoped syntax with braces
		// If it's already a block-scoped namespace, just return it as-is
		if (node.OpenBraceToken.IsKind(SyntaxKind.OpenBraceToken) && node.CloseBraceToken.IsKind(SyntaxKind.CloseBraceToken)) {
			// Filter usings inside the namespace based on RetainUsingNamespace setting
			var filteredUsings = FilterUsings(node.Usings);

			// Process members inside the namespace
			var processedMembers = new List<MemberDeclarationSyntax>();
			foreach (var member in node.Members) {
				var processedMember = (MemberDeclarationSyntax)Visit(member);
				processedMembers.Add(processedMember);
			}

			var newNamespace = node
				.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
				.WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
				.WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken))
				.WithUsings(filteredUsings)
				.WithMembers(SyntaxFactory.List(processedMembers));

			return newNamespace;
		}
		return node;
	}

	public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node) {
		// Convert file-scoped namespace (namespace X;) to block-scoped namespace (namespace X { ... })
		var openBraceToken = SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
			.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed); // Add newline after opening brace

		// Filter usings based on RetainUsingNamespace setting
		var filteredUsings = FilterUsings(node.Usings);

		var newNamespace = SyntaxFactory.NamespaceDeclaration(
			node.AttributeLists,
			node.Modifiers,
			node.NamespaceKeyword,
			node.Name,
			openBraceToken,
			node.Externs,
			filteredUsings,
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

	// Add a helper method to filter using directives
	private SyntaxList<UsingDirectiveSyntax> FilterUsings(SyntaxList<UsingDirectiveSyntax> usings) {
		var filteredList = new List<UsingDirectiveSyntax>();

		foreach (var usingDirective in usings) {
			// Check if this is a type alias (has an alias)
			if (usingDirective.Alias != null) {
				// This is a type alias like "using i32 = System.Int32;"
				if (_processor.Opt.RetainUsingTypeAlias) {
					filteredList.Add(usingDirective);
				}
			}
			else {
				// This is a regular using directive like "using System;"
				if (_processor.Opt.RetainUsingNamespace) {
					filteredList.Add(usingDirective);
				}
			}
		}

		return SyntaxFactory.List(filteredList);
	}

	public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node) {
		// Remove method body, keep only the declaration
		if (node.Body != null) {
			return node.WithBody(null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
		}
		// Also handle expression body (=> ...)
		if (node.ExpressionBody != null) {
			return node.WithExpressionBody(null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
		}
		return node;
	}
	//2026_0220_162937

	public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) {
		// Process class members - extension blocks will be handled via DefaultVisit
		var processedMembers = new List<MemberDeclarationSyntax>();
		foreach (var member in node.Members) {
			var processedMember = Visit(member);
			if (processedMember is MemberDeclarationSyntax memberDecl) {
				processedMembers.Add(memberDecl);
			}
		}
		return node.WithMembers(SyntaxFactory.List(processedMembers));
	}

	public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node) {
		// Remove property body (get/set accessors) and expression body, keep only the declaration
		var newNode = node;

		// Save trailing trivia from accessor list or expression body before removing them
		// This preserves #if, #endif and other preprocessor directives
		SyntaxTriviaList trailingTrivia = default;

		// Remove accessor list (get { } set { })
		if (newNode.AccessorList != null) {
			// Get trailing trivia from the close brace of accessor list
			// This includes directives like #if Impl that appear after the closing brace
			trailingTrivia = newNode.AccessorList.CloseBraceToken.TrailingTrivia;
			newNode = newNode.WithAccessorList(null);
		}

		// Remove expression body (=> ...)
		if (newNode.ExpressionBody != null) {
			// If there's an expression body, get its trailing trivia instead
			var exprTrailing = newNode.ExpressionBody.GetTrailingTrivia();
			if (exprTrailing.Count > 0) {
				trailingTrivia = exprTrailing;
			}
			newNode = newNode.WithExpressionBody(null);
		}

		// Add semicolon if not present, preserving trailing trivia
		if (!newNode.SemicolonToken.IsKind(SyntaxKind.SemicolonToken)) {
			var semicolonToken = SyntaxFactory.Token(SyntaxKind.SemicolonToken);

			// If we have trailing trivia (like #if directives), append it to the semicolon
			if (trailingTrivia.Count > 0) {
				semicolonToken = semicolonToken.WithTrailingTrivia(trailingTrivia);
			}

			newNode = newNode.WithSemicolonToken(semicolonToken);
		}

		return newNode;
	}

	public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node) {
		// Remove constructor body, keep only the declaration
		if (node.Body != null) {
			return node.WithBody(null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
		}
		return node;
	}

	public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax node) {
		// Check if this is a type alias (has an alias)
		if (node.Alias != null) {
			// This is a type alias like "using i32 = System.Int32;"
			return _processor.Opt.RetainUsingTypeAlias ? node : null;
		}
		else {
			// This is a regular using directive like "using System;"
			return _processor.Opt.RetainUsingNamespace ? node : null;
		}
	}
		// Handle extension blocks (C# extension types) - extension(ITable z) { ... }
	public override SyntaxNode? VisitExtensionBlockDeclaration(ExtensionBlockDeclarationSyntax node) {
		// Process members inside the extension block
		var processedMembers = new List<MemberDeclarationSyntax>();
		foreach (var member in node.Members) {
			var processedMember = (MemberDeclarationSyntax)Visit(member);
			if (processedMember != null) {
				processedMembers.Add(processedMember);
			}
		}
		return node.WithMembers(SyntaxFactory.List(processedMembers));
	}


}

public class DeclService {
	private readonly DeclProcessor _processor;

	public DeclService(DeclProcessor processor = null) {
		_processor = processor ?? new DeclProcessor();
	}

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
			var processedCode = _processor.ProcessSourceCode(sourceCode);

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
