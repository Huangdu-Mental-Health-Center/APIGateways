# API Gateways
Huangdu Mental Health Center API 网关。  
![build_status](https://github.com/Huangdu-Mental-Health-Center/APIGateways/workflows/.NET%20Core/badge.svg)
![language](https://img.shields.io/badge/language-C%23%209.0-blue.svg)
![env](https://img.shields.io/badge/-.NET%20Core%205.0-blueviolet)

## 1. 开发环境

- OS: Debian GNU/Linux 10 (buster) x86_64
- Kernel: 4.19.0-12-amd64
- Microsoft Visual Studio Professional 2019 v16.8.1

## 2. 功能和使用

若您想将您的微服务添加到网关控制，请参考部署文档部分。

### 1. 路由功能

根据上下文或消息内容将请求发送到不同的目标。  
目前已部署的微服务：

- [MockHospitalData](https://github.com/Huangdu-Mental-Health-Center/MockHospitalData)  
  路由规则  
  - `/{query}` => `/api/{loginOrRegister}`
- [RegisterAndLoginServices](https://github.com/Huangdu-Mental-Health-Center/RegisterAndLoginServices)  
  路由规则  
  - `/api/{loginOrRegister}` => `/api/{loginOrRegister}`

具体 API 的实现与使用您可以参考对应 API 的文档。

### 2. 鉴权功能

为 API 添加身份验证（非业务逻辑层面）的安全策略。  
目前已实现的鉴权方案：

- 允许所有人访问（默认）。
- 仅登录用户访问。
- 仅管理员可访问。

### 3. 日志记录功能

*Todo*

### 4. 其他功能

*Todo：监控，均衡负载，缓存等*

## 3. 部署文档

### **1.安装 .NET 5.0 SDK** 

0. 安装 .NET 之前，请根据自身系统环境将 Microsoft 包签名密钥添加到受信任密钥列表，并添加包存储库。  
[MS-Document](https://docs.microsoft.com/en-us/windows-server/administration/linux-package-repository-for-microsoft-software)

1. 安装 .Net 5.0。  
    [MS-Document](https://docs.microsoft.com/en-us/dotnet/core/install/linux)

2. clone 本项目。

```shell
git clone https://github.com/Huangdu-Mental-Health-Center/APIGateways.git
cd ./APIGateways
```

### **2. 手动添加配置文件**

- 在项目根目录下新建 app.config，添加以下内容，并手动替换其中值字段。

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <appSettings>
    <add key="Secret" value="你的 JWT Secret"/>
    <add key="Domain" value="你的 JWT Domain"/>
    <add key="ListenPort" value="服务所使用端口"/>
  </appSettings>
</configuration>
```

- 编译并运行。

```shell
dotnet run -c release
```

### 3. 添加或删除微服务

如果要添加新的或删除现有的微服务，请编辑 `ocelot.json`。  
例如，以下配置文件中有两个微服务：登录注册服务和医生查询服务在运行中：

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/{loginOrRegister}", // 下游服务路由，可以使用 {} 作为通配符
      "DownstreamScheme": "http", // 指定下游 Scheme
      "DownstreamHostAndPorts": [ // 下游服务地址和端口，可以有多个下游进行均衡负载
        {
          "Host": "localhost",
          "Port": 7777
        }
      ],
      "UpstreamPathTemplate": "/api/{loginOrRegister}", // 外部访问的服务路由
      "UpstreamHttpMethod": [ "Post" ], // HTTP 请求方法
    },
    {
      "DownstreamPathTemplate": "/{query}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 14000
        }
      ],
      "UpstreamPathTemplate": "/api/hospital_data/{query}",
      "UpstreamHttpMethod": [ "Get" ],
      "AuthenticationOptions": { // 若服务需要认证，则添加该字段
        "AuthenticationProviderKey": "UserKey", // 指定使用的 ProviderKey，目前有两种
                                                // - UserKey 所有登录用户
                                                // - AdminKey 仅管理员可访问
        "AllowedScopes": []
      }
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "https://localhost:5200"
  }
}
```

若要添加一个新的服务：“测试”：

- 服务地址 `http://localhost:5000/api/test` 方法 `GET`。
- 需要管理员权限才能访问。

那么应该在 `"Routes"` 字段添加：

```json
"Routes": [
    {
      "DownstreamPathTemplate": "/api/test",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5000
        }
      ],
      "UpstreamPathTemplate": "/api/test",
      "UpstreamHttpMethod": [ "Get" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "AdminKey",
        "AllowedScopes": []
      }
    },
    ... // 其他服务的内容
```

**注意，您可以在 API 网关服务运行时修改配置文件以实现热更新。**



