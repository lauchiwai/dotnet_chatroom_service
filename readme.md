# .net 微服務 - chatService

基於.NET 8的現代化微服務架構，整合雙Token驗證、消息佇列與雲原生部署方案。

## 📋 功能列表

- **認證授權**
  - JWT + Refresh Token 雙Token機制
- **資料層**
  - MSSQL 資料庫整合
  - Entity Framework Core 8
  - Repository Pattern 實現
- **異步通訊**
  - RabbitMQ 消息生產
  - HTTP Client 服務間通訊
- **即時推送**
  - Server-Sent Events (SSE) 中轉層
- **基礎設施**
  - Docker 容器化
  - CI/CD 自動化流水線
  - Swagger API 文檔

## 🛠️ 環境要求

- .NET 8 SDK
- Docker 20.10+
- MSSQL 2022 / Docker版
- RabbitMQ 3.12+
- Git 2.40+

推薦IDE：

- VS 2022
