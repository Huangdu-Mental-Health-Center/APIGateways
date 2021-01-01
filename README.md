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

- Schedule  
  路由规则

  - `/api/{query}` => `/api/{query}`

- [MockHospitalData](https://github.com/Huangdu-Mental-Health-Center/MockHospitalData)  
  路由规则  
  
  - `/{query}` => `/api/hospital_data/{query}`

- [MockMedicalRecords](https://github.com/Huangdu-Mental-Health-Center/MockMedicalRecords)  
  路由规则
  - `/{query}` => `/api/medical_records/{query}`

- [RegisterAndLoginServices](https://github.com/Huangdu-Mental-Health-Center/RegisterAndLoginServices)  
  路由规则  
  - `/api/{loginOrRegister}` => `/api/{loginOrRegister}`
  - `/api/register/admin` => `/api/register/admin`
  - `/api/userinfo` => `/api/userinfo`
  - `/api/userinfo/` => `/api/userinfo`
  - `/api/userinfo/{id}` => `/api/userinfo/{id}`
  - `/api/changepassword` => `/api/changepassword`

具体 API 的实现与使用您可以参考对应 API 的文档。

### 2. 身份验证与鉴权功能

#### 身份验证功能

为 API 添加身份验证（非业务逻辑层面）的安全策略。  
需要登录身份验证的 API，请在 `ocelot.json` 中对应 API 代码段添加以下示例项：

```json
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "AuthKey",
        "AllowedScopes": []
      }
```

`AuthenticationProviderKey` 指定使用的验证 Key，目前只有 `AuthKey`，对所有登录用户进行验证。  
`AllowedScopes` 由于使用 JWT 验证，请**不要**修改这一项。

用户未登录，或凭证过期会返回 `401 Unauthorized`。

#### 用户鉴权功能

目前已实现的鉴权方案：

- 仅登录用户访问。（仅启用验证而不添加鉴权时默认）
- 仅管理员可访问。
- 仅超级管理员可访问。

要在网关层面实现鉴权，请在 `ocelot.json` 中对应 API 代码段添加以下示例项：

```json
      "RouteClaimsRequirement": {
        "aud": "admin",
        "http///schemas.microsoft.com/ws/2008/06/identity/claims/role": "suadmin"
      }
```

`aud` 字段可选 `admin` `user` ，用于区分管理员和一般用户。  
`http///schemas.microsoft.com/ws/2008/06/identity/claims/role` 字段可选 `admin` `suadmin` ，用于区分一般管理员和超级管理员，注意在这里请使用 `http///` 代替 `http://` ，以规避 Ocelot 框架中 json 解析的问题。

用户调用无权访问的 API 会返回 `403 Forbidden`。

### 3. 日志记录功能

Ocelot 日志记录使用 .NET 框架原生配置，因此考虑使用 `nlog` ，产生的日志记录在 `./log` 文件夹下。  
相关配置请参考 [nlog.config](./nlog.config)。

### 4. 缓存功能

使用 Ocelot 框架的原生缓存管理 `Ocelot.Cache.CacheManager`。  
可以在内存中缓存请求结果，降低服务的调用频率，适用于如一些非敏感信息的查询。  
要启用缓存，请在 `ocelot.json` 中对应 API 代码段添加以下示例项：

```json
      "FileCacheOptions": {
        "TtlSeconds": 15,
        "Region": "somename"
      }
```

`TtlSeconds` 项配置缓存失效时间，单位为秒。

### 5. 负载均衡功能

使用 Ocelot 框架的负载均衡功能。  
要启用负载均衡，请在 `ocelot.json` 中对应 API 代码段添加以下示例项：

```json
      "LoadBalancerOptions": {
        "Type": "RoundRobin"
      }
```

`Type` 指定负载均衡方案，详见 [Ocelot Doc](https://ocelot.readthedocs.io/en/latest/features/loadbalancer.html)。

### 6. 限流功能

使用 Ocelot 框架的限流功能。  
要启用限流功能，请在 `ocelot.json` 中对应 API 代码段添加以下示例项：

```json
      "RateLimitOptions": {
        "ClientWhitelist": [],
        "EnableRateLimiting": true,
        "Period": "1s",
        "PeriodTimespan": 1,
        "Limit": 3
      }
```

`ClientWhitelist` 白名单，列表中的客户端将不受限流策略影响。  
`EnableRateLimiting` 是否启用限流。  
`Period` 与 `Limit` 表示在 `Period` 时间内，API 最多可以被调用 `Limit` 次。  
`PeriodTimespan` 限流触发后，再次调用 API 需要等待的时间，单位为秒。

限流被触发时，调用 API 会返回 `429 Too Many Requests`。

### 7. 熔断机制

使用  `Ocelot.Provider.Polly` 实现。  
当某个服务多次无响应后可以启用熔断，熔断时间内再次调用此 API 会快速返回错误信息，避免阻塞。  
要启用熔断机制，请在 `ocelot.json` 中对应 API 代码段添加以下示例项：

```json
      "QoSOptions": {
        "ExceptionsAllowedBeforeBreaking": 3,
        "DurationOfBreak": 30000,
        "TimeoutValue": 2000
      }
```

`ExceptionsAllowedBeforeBreaking` 熔断前的尝试次数。  
`DurationOfBreak` 熔断时间，单位为毫秒。  
`TimeoutValue` 响应超时时间，若服务超过这个设定的时间未响应，则返回错误信息。

如果不想启用熔断机制，可以单独设置响应超时时间。

在服务超时未响应或熔断的情况下调用，会返回 `503 Service Unavailable`。

### 8. 其他功能

*Todo：监控，服务发现等*

## 3. 部署文档

### **1. 安装 .NET 5.0 SDK** 

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
    <add key="CorsDomains" value="http://localhost,你的服务地址（可以添加多个，使用逗号间隔）"/>
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
        "AuthenticationProviderKey": "AuthKey",
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
        "AuthenticationProviderKey": "AuthKey",
        "AllowedScopes": []
      },
      "RouteClaimsRequirement": {
        "aud": "admin"
      }
    },
    ... // 其他服务的内容
```

**注意，您可以在 API 网关服务运行时修改配置文件以实现热更新。**
