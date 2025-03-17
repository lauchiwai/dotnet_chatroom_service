## 在本地初始化 Git 倉庫

1. cd /path/to/your/dotnet-project
2. git init

## 配置 .gitignore 文件

dotnet new gitignore

## 將文件加入本地倉庫

git add .
git commit -m "Initial commit for .NET 8 project"

## 關聯到遠端 github

git remote add origin https://github.com/yourusername/my-dotnet-project.git
