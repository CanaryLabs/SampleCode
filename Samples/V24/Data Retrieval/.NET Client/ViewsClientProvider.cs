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

        // Takes a function that has the CanaryViewsApiServiceClient and cci as parameters
        public async Task<T> MakeRequestAsync<T>(Func<CanaryViewsApiServiceClient, int, Task<T>> requestAction)
        {
            return await MakeRequestAsync(async (ViewsClientConnectionContext context) =>
            {
                return await requestAction(context.Client, context.Cci);
            });
        }

        // Releases the Client Connection Id in the Views Service
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
                        // Reset the client context so that CreateClientContext is called again on the next MakeRequestAsync call
                        ResetClientContext(context);
                    }

                    return response;
                });
            }
            catch { }
        }

        public static async Task<int> GetCciAsync(CanaryViewsApiServiceClient client)
        {
            GetClientConnectionIdResponse getCciResponse = await client.GetClientConnectionIdAsync(
                new GetClientConnectionIdRequest()
                {
                    App = APP_ID,
                    UserId = System.Net.Dns.GetHostName() + ":" + System.Environment.UserName
                });


            if (getCciResponse.Status.StatusType == Canary.Views.Grpc.Common.ApiCallStatusType.NoLicense)
                throw new Exception("Views License is not available at this time.");
            else if (getCciResponse.Status.StatusType != Canary.Views.Grpc.Common.ApiCallStatusType.Success)
                throw new Exception($"Error getting Views client connection id: {getCciResponse.Status.StatusErrorMessage}");

            return getCciResponse.Cci;
        }

        public static async Task<CanaryViewsApiServiceClient> Connect(string viewsEndpoint, string identityToken)
        {
            return await GrpcClientConnectionHelper.Instance.GetViewsClientAsync(
                viewsEndpoint,
                (callOptions) =>
                {
                    var clientToken = CanaryClientToken.CreateApiToken(identityToken);
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

        // Called by the base class
        // Makes the connection to Views and retrieves the Client Connection Id required for data calls
        protected override async Task<ViewsClientConnectionContext> CreateClientContext()
        {
            CanaryViewsApiServiceClient client = await Connect(_viewsEndpoint, _identityApiToken);
            int cci = await GetCciAsync(client);

            // The CanaryViewsApiServiceClient and cci are saved into the base class. They are made available to the user inside the function parameter of the MakeRequestAsync method
            return new ViewsClientConnectionContext(client, cci);
        }


        protected override async Task<T> MakeRequestAsync<T>(Func<ViewsClientConnectionContext, Task<T>> requestAction)
        {
            try
            {
                // The base method calls CreateClientContext if it has not been done yet or if the context has been reset. It
                // then invokes the requestAction with the client context as the parameter
                return await base.MakeRequestAsync(requestAction);
            }
            catch (GrpcFailedRequestException)
            {
                // Logic can be added here to handle Views Service disconnects
                throw;
            }
        }
    }
}