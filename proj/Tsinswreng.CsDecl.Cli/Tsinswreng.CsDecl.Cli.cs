//dotnet run -- E:\_code\CsNgaq\Tsinswreng.CsDecl\proj\TestCsprojCases\Test1\Test1.csproj out
using Tsinswreng.CsDecl;

if (args.Length < 2) {
	Console.WriteLine("用法: Tsinswreng.CsDecl.Cli <project.csproj> <outputDir>");
	return 1;
}

var csprojPath = Path.GetFullPath(args[0]);
var outputDir = Path.GetFullPath(args[1]);

var svc = new DeclService();
svc.ProcessProject(csprojPath, outputDir);

return 0;
