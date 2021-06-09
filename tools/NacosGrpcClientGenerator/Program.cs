using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

const string RelativePath = "grpc_temp";

var clientReplaceKeyWords = new List<KeyValuePair<string, string>>()
{
    //new KeyValuePair<string, string>("\"NacosStreamResultTransport\"","\"RequestStream\""),
    new KeyValuePair<string, string>("grpc::MethodType\\.ServerStreaming,[\\r\\n]* +__ServiceName,[\\r\\n]* +\"RequestStream\",","grpc::MethodType.ServerStreaming,\n        \"RequestStream\",\n        \"requestStream\","),
    //new KeyValuePair<string, string>("\"NacosTransport\"","\"Request\""),
    new KeyValuePair<string, string>("grpc::MethodType\\.Unary,[\\r\\n]* +__ServiceName,[\\r\\n]* +\"Request\",","grpc::MethodType.Unary,\n        \"Request\",\n        \"request\","),
    //new KeyValuePair<string, string>("\"NacosBiStreamTransport\"","\"BiRequestStream\""),
    new KeyValuePair<string, string>("grpc::MethodType\\.DuplexStreaming,[\\r\\n]* +__ServiceName,[\\r\\n]* +\"BiStreamRequest\",","grpc::MethodType.DuplexStreaming,\n        \"BiRequestStream\",\n        \"requestBiStream\","),
};

#region 路径定义

var rootPath = Path.Combine(Environment.CurrentDirectory, RelativePath);
var originDataDefineFile = Path.Combine(rootPath, "origin/NacosGrpcServiceOrigin.cs");
//var originClientFile = Path.Combine(rootPath, "origin/NacosGrpcServiceOriginGrpc.cs");

var modifiedDataDefineFile = Path.Combine(rootPath, "modified/NacosGrpcService.cs");
var modifiedClientFile = Path.Combine(rootPath, "modified/NacosGrpcServiceGrpc.cs");

var targetDirectory = Path.Combine(Environment.CurrentDirectory, "../../../../../src/Nacos.Grpc/GrpcService");

var targetDataDefineFile = Path.Combine(targetDirectory, "NacosGrpcService.cs");
var targetClientFile = Path.Combine(targetDirectory, "NacosGrpcServiceGrpc.cs");

#endregion 路径定义

#region 修改消息定义descriptorData

var originDataDefineCode = await File.ReadAllTextAsync(originDataDefineFile);
var modifiedDataDefineCode = await File.ReadAllTextAsync(modifiedDataDefineFile);

var descriptorDataRegex = "byte\\[\\] descriptorData =[\\S\\s\\r\\n]+?\\);";

var originDescriptorData = Regex.Match(originDataDefineCode, descriptorDataRegex).Value;

modifiedDataDefineCode = Regex.Replace(modifiedDataDefineCode, descriptorDataRegex, originDescriptorData);

await File.WriteAllTextAsync(modifiedDataDefineFile, modifiedDataDefineCode);

#endregion 修改消息定义descriptorData

#region 修改方法、服务名称

var modifiedClientCode = await File.ReadAllTextAsync(modifiedClientFile);

foreach (var item in clientReplaceKeyWords)
{
    modifiedClientCode = Regex.Replace(modifiedClientCode, item.Key, item.Value);
}

await File.WriteAllTextAsync(modifiedClientFile, modifiedClientCode);

#endregion 修改方法、服务名称

File.Copy(modifiedDataDefineFile, targetDataDefineFile, true);
File.Copy(modifiedClientFile, targetClientFile, true);

Console.WriteLine("Over!");