# DotNetCoreApi.Template
1. TargetFramework : netcoreapp3.1
2. ORM : EntityFrameworkCore
3. Ioc/DI : Autofac
4. Mapper : AutoMapper
5. Open Api : Swashbuckle.AspNetCore
6. Logger : Seq

# 專案rename
1. 編輯 rename.ps1
2. 修改 newCompanyName、newProjectName
3. 使用powershell執行該ps1檔

# Customer Helper
1. HttpClientHelper : 用於呼叫他方API使用，同時會搭配Seq紀錄API Request、Response
2. TimestampHelper : 產生時間戳記
3. EnumHelper : 處理Enum值轉換
4. EntityHelper : 協助新增Entity Index

# Customer Action Filter
1. ActionLogAttribute : 搭配Seq紀錄他方呼叫我方API Request、Response等紀錄
2. ModelStateValidationAttribute : 客製化API Request Model 驗證

# Customer Middleware
1. ExceptionMiddleware : 捕捉系統未預期錯誤，並且搭配Seq記錄錯誤原因

