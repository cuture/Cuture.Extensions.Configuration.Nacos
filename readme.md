# Cuture.Extensions.Configuration.Nacos

## Intro

一个轻量的，基于 `Nacos` 的 `Microsoft.Extensions.Configuration.IConfigurationSource` 配置源库;

## Background
项目考虑接入Nacos作为配置中心，然后发现Nacos官方有SDK - [nacos-sdk-csharp](https://github.com/nacos-group/nacos-sdk-csharp)，但是在尝试过程中遇到一些问题（比如Grpc端口号固定为http端口号+1000等）。。。本来想去修改官方SDK，结果完全理不清代码逻辑。。。那就参照着重写一个理想的针对当前使用场景的版本，顺便试着领悟一下官方SDK的设计思路。。。

## Features
- 没有历史包袱 (当前仅支持`.net5`);
- 便捷的Nacos地址配置，支持自定义Grpc端口号;
- 更轻便直观的配置 (个人觉得);
- 较少的包依赖 (可以在不引用Grpc包的情况下使用Http协议接入);
- 更方便调试 (没有异步Timer...支持SourceLink);
- 支持`阿里云ACM` (支持KMS加密配置的自动解密，不依赖阿里云SDK包);
- 支持添加配置获取处理中间件，自定义配置解密等;

## Note
- 当前仅实现了`Nacos`的`配置读取`功能，`不支持服务发现` (暂时没有需求...)
- 没有实现任何`failover`机制 (基于文件缓存的`failover`对于容器化运行环境不是很有必要...)

** 在`Nacos2.0.1 with docker`与`阿里云ACM`环境下开发，其它环境未测试。

** 单元测试代码暂时还没写...

------------------

## 如何使用

### 1. 安装包

```powershell
# 使用Http客户端
Install-Package Cuture.Extensions.Configuration.Nacos -IncludePrerelease

# 使用Grpc客户端
Install-Package Cuture.Extensions.Configuration.Nacos.Grpc -IncludePrerelease
```

### 2. 配置

------------------

#### 2.1 使用拓展方法进行配置

```C#
Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddNacos(options =>
        {
            options.AddServerAddress("http://127.0.0.1:18848")  //添加Nacos地址
                   .AddServerAddress("http://127.0.0.1:8848")   //添加Nacos地址
                   //.UseGrpcClient()   //使用Grpc客户端（需要安装Grpc包）
                   .UseHttpClient()     //使用Http客户端
                   .WithUser("username", "password")    //使用的用户
                   .SubscribeConfiguration("test1", "Cfg1") //订阅配置
                   .SubscribeConfiguration("test1", "Cfg2"); //订阅配置
        })
    });
```

------------------

#### 2.2 使用配置文件进行配置

- 配置文件示例

```json
{
  "ClientType": "Grpc", //可选，客户端类型 - Http 或 Grpc（需要安装Grpc包，并使用对应的配置方法）默认为Http（阿里云ACM暂时不支持GRPC）
  "Servers": [ //Nacos服务器地址列表（支持特殊定义的地址）
    "http://127.0.0.1:18848/",
    "http://127.0.0.1:8848/"
  ],
  "Auth": { //认证信息节点
    "User": { //用于Nacos登陆的用户信息
      "Account": "username",
      "Password": "password"
    },
    "ACS": { //用于阿里云ACS认证信息
      "RegionId": "",
      "AccessKeyId": "",
      "AccessKeySecret": ""
    }
  },
  "Configuration": { //Configuration配置
    "DefaultNamespace": "test1", //默认的命名空间，当订阅项不指定 Namespace 时，使用此值
    "DefaultGroup": "DEFAULT_GROUP", //默认组，当订阅项不指定 Group 时，使用此值。不设置时，默认为 DEFAULT_GROUP
    "Subscriptions": [ //Configuration订阅列表
      {
        "Optional": false, //可选，是否为可选配置，默认为false
        "ReloadOnChange": true, //可选，是否监听配置变更，默认为true
        "DataId": "Cfg1", //配置的DataId
        "Group": "DEFAULT_GROUP", //配置所在Group，当不设定时，使用 DefaultGroup
        "Namespace": "test1" //配置所在命名空间，当不设定时，使用 DefaultNamespace
      },
      {
        "DataId": "Cfg2"
      }
    ]
  },
  "HealthCheckInterval": "00:00:05", //可选，客户端健康检查间隔
  "ClientIPSubnet": "192.168.1.1/24", //可选，获取客户端IP时，将获取此值所在子网IP
  "SpecifyClientIP": "127.0.0.1" //可选，指定客户端IP，优先级高于ClientIPSubnet
}
```

- 使用配置文件

```C#
Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder =>
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("nacos.json").Build();

        //builder.AddNacos(configuration);  //仅使用Http
        builder.AddNacosWithGrpcClientAllowed(configuration);    //允许使用Grpc
    });
```

## Nacos服务地址定义格式

- 标准的Uri格式
- 使用`Scheme`和`Fragment`进行自定义端口号

eg:
```
http://127.0.0.1:8848/                          //解析结果 - HttpPort: 8848 , GrpcPort: 9848    （GrpcPort为HttpPort+1000）
http://127.0.0.1:8848/#GrpcPort=8849            //解析结果 - HttpPort: 8848 , GrpcPort: 8849
http-grpc://127.0.0.1:9848/                     //解析结果 - HttpPort: 8848 , GrpcPort: 9848    （HttpPort为GrpcPort-1000）
http-grpc://127.0.0.1:8849/#HttpPort=8848       //解析结果 - HttpPort: 8848 , GrpcPort: 8849

http-endpoint-acm://acm.aliyun.com/             //解析结果 - 使用阿里云acm
http-endpoint://127.0.0.1/list                  //解析结果 - 使用endpoint模式，内容仍然遵循此解析模式
```