using System.Threading.Tasks;
using Nacos.Grpc.Messages;

namespace Nacos.Grpc;

internal class RequestHandler
    : IRequestHandler<ClientDetectionRequest>
{
    public Task<NacosResponse> HandleAsync(ClientDetectionRequest request)
    {
        return Task.FromResult<NacosResponse>(new ClientDetectionResponse());
    }
}
