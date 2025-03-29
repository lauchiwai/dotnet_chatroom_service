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



# docker command

## 清理构建缓存

docker-compose down -v --remove-orphans
docker builder prune -af

## 重新构建

docker-compose up --build -d

## 查看依赖安装情况

docker exec -it py_chat_service-chat-service-1 pip list

## 注意 ：

需要連接到同一個網絡才能呼叫

斷開 sql1 的預設網路 (sql1 是你的 docker container)
docker network disconnect bridge sql1

加入自定義網路
docker network connect app-network sql1
