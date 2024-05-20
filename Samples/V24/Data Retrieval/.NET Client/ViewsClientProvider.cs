using Canary.Utility.CanaryClientHelper;
using Canary.Utility.GrpcHelper.Token;
using Canary.Views.Grpc.Api;
using Grpc.Core;
using System;
using System.Threading.Tasks;
using static Canary.Views.Grpc.Api.CanaryViewsApiService;

namespace ReadData
{
    public class ViewsClientConnectionContext : IGrpcClientContext<CanaryViewsApiServiceClient>
    {
        public ViewsClientConnectionContext(CanaryViewsApiServiceClient client, int cci)
        {
            Client = client;
            Cci = cci;
        }

        public int Cci { get; }
        public CanaryViewsApiServiceClient Client { get; }
    }

    public class ViewsClientProvider : GrpcClientProviderBase<CanaryViewsApiServiceClient, ViewsClientConnectionContext>
    {
        private const string APP_ID = "Views API Example";
        private readonly string _identityApiToken;
        private readonly string _viewsEndpoint;

        internal ViewsClientProvider(string identityApiToken, string viewsEndpoint)
        {
            _identityApiToken = identityApiToken;
            _viewsEndpoint = viewsEndpoint;
        }

        public async Task<T> MakeRequestAsync<T>(Func<CanaryViewsApiServiceClient, int, Task<T>> requestAction)
        {
            return await MakeRequestAsync(async (ViewsClientConnectionContext context) =>
            {
                return await requestAction(context.Client, context.Cci);
            });
        }

        public async Task ReleaseCciAsync()
        {
            try
            {
                await MakeRequestAsync(async (ViewsClientConnectionContext context) =>
                {
                    ReleaseClientConnectionIdRequest request = new()
                    {
                        Cci = context.Cci
                    };

                    ReleaseClientConnectionIdResponse response = new ReleaseClientConnectionIdResponse() { Status = new() };

                    try
                    {
                        response = await context.Client.ReleaseClientConnectionIdAsync(request, deadline: DateTime.UtcNow.AddSeconds(10));
                    }
                    catch (Exception ex)
                    {
                        response.Status.StatusType = Canary.Views.Grpc.Common.ApiCallStatusType.CheckExtendedStatus;
                        response.Status.StatusErrorMessage = ex.Message;
                    }
                    finally
                    {
                        ResetClientContext(context);
                    }

                    return response;
                });
            }
            catch { }
        }

        protected override async Task<ViewsClientConnectionContext> CreateClientContext()
        {
            CanaryViewsApiServiceClient client = await GetViewsClient();
            int cci = await GetCciAsync(client);
            var clientConnection = new ViewsClientConnectionContext(client, cci);
            return clientConnection;
        }

        protected override async Task<T> MakeRequestAsync<T>(Func<ViewsClientConnectionContext, Task<T>> requestAction)
        {
            try
            {
                return await base.MakeRequestAsync(requestAction);
            }
            catch (GrpcFailedRequestException ex)
            {
                throw new Exception("Views Service request failed.", ex);
            }
        }

        private static async Task<int> GetCciAsync(CanaryViewsApiServiceClient client)
        {
            GetClientConnectionIdResponse getCciResponse;
            try
            {
                getCciResponse = await client.GetClientConnectionIdAsync(
                    new GetClientConnectionIdRequest()
                    {
                        App = APP_ID,
                        UserId = System.Net.Dns.GetHostName() + ":" + System.Environment.UserName
                    });
            }
            catch (RpcException ex)
            {
                throw new Exception("Views Service request failed.", ex);
            }

            if (getCciResponse.Status.StatusType == Canary.Views.Grpc.Common.ApiCallStatusType.NoLicense)
                throw new Exception("Views License is not available at this time.");
            else if (getCciResponse.Status.StatusType != Canary.Views.Grpc.Common.ApiCallStatusType.Success)
                throw new Exception($"Error getting Views client connection id: {getCciResponse.Status.StatusErrorMessage}");

            return getCciResponse.Cci;
        }

        private async Task<CanaryViewsApiServiceClient> GetViewsClient()
        {
            return await GrpcClientConnectionHelper.Instance.GetViewsClientAsync(
                _viewsEndpoint,
                (callOptions) =>
                {
                    var clientToken = CanaryClientToken.CreateApiToken(_identityApiToken);
                    if (clientToken is null)
                        return callOptions;

                    return callOptions.WithHeaders(new Metadata
                    {
                        new Metadata.Entry(
                            key: clientToken.GetMetadataKey(),
                            value: clientToken.TokenValue)
                    });
                });
        }
    }
}