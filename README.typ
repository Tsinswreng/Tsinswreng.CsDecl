#let H(t,d)={}

/*
#H[標題][
	標題內容
	#H[子標題][
		子標題內容
	]
]
 */

這是命令行工具
傳入csproj文件路徑
生成項目的符號聲明、用作面向大模型的API文檔。

符號聲明中包含類型(類,結構體,接口,枚舉及其成員), 函數聲明, 訪問器聲明與實現、
但不包含函數體。

生成的代碼中:
- 移除所有函數實現
	- 即 `void fn(){}` 變成 `void fn();`
- 移除所有`using <namespace>`語句
- 命名空間統一用 `namespace MyNs{}`寫法、不使用`namespace MyNs;`語法
- 保留Attribute及上方的註釋


#H[構建][

非AOT構建
```bash
cd proj/Tsinswreng.CsDecl.Cli
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

]


#H[使用][
```bash
mkdir -p MyOutDir/
CsDecl MyProj.csproj MyOutDir/
```
]


#H[簡單測試][
```bash
# cwd=<git root>/proj/Tsinswreng.CsDecl.Cli/
dotnet run -- ../TestCsprojCases/Test1/Test1.csproj bin
```
]

