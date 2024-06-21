using Canary.StoreAndForward2.Grpc.Api;
using Canary.StoreAndForward2.Grpc.Api.Helper;
using Canary.Utility.ProtobufSharedTypes;

namespace SAF_Examples
{
    public class RawClientExample
    {
        private enum Qualities
        {
            Good = 192, // 0xC0
            NoData = -32768, // 0x8000
        }

        private readonly string _dataset = "RawExample";
        private readonly string _historian = "localhost";
        private readonly Dictionary<string, int> _tagMap = [];
        private IGrpcApiSession? _session = null;

        public async Task CreateNewFileAsync()
        {
            var session = Connect();

            await session.WriteAsync(new StreamElement()
            {
                FileNew = new FileNewElement()
                {
                    DataSet = _dataset,
                    FileTime = DateTime.MinValue.ToGrpcTimestamp()
                }
            }, CancellationToken.None);
        }

        public async Task DisconnectAsync()
        {
            if (_session != null)
            {
                await _session.CloseAsync(TimeSpan.FromSeconds(30));
                _session = null;
            }
        }

        public async Task FileRollOverAsync()
        {
            var session = Connect();

            await session.WriteAsync(new StreamElement()
            {
                FileRollover = new FileRolloverElement()
                {
                    DataSet = _dataset,
                    FileTime = DateTime.MinValue.ToGrpcTimestamp()
                }
            }, CancellationToken.None);
        }

        public Dictionary<string, int> GetTagIds()
        {
            var session = Connect();

            if (_tagMap.Count == 0)
            {
                int tagCount = 5;
                ConfigureTagRequest[] requests = new ConfigureTagRequest[tagCount];

                for (int i = 0; i < tagCount; i++)
                {
                    requests[i] = new ConfigureTagRequest()
                    {
                        TagPath = string.Format("{0}.Tag {1:D4}", _dataset, i + 1),
                        NullableTransformEquation = null,
                        NullableTimestampNormalizationMilliseconds = null,
                        IsTimestampExtensionEnabled = true
                    };
                }

                session.ConfigureTags(requests);

                for (int i = 0; i < requests.Length; i++)
                {
                    _tagMap.Add(requests[i].TagPath, session.GetConfiguredTagId(requests[i].TagPath));
                }
            }

            return _tagMap;
        }

        public async Task NoDataAsync()
        {
            var session = Connect();

            Dictionary<string, int> tagIds = GetTagIds();

            IEnumerable<StreamElement> streamElements = tagIds.Values.Select(p => new StreamElement()
            {
                NoData = new NoDataElement()
                {
                    TagId = p
                }
            });

            await session.WriteAsync(streamElements, CancellationToken.None);
        }

        public async Task StoreDataAsync()
        {
            var session = Connect();

            List<StreamElement> streamElements = [];

            // create data to store
            DateTime now = DateTime.Now;
            Dictionary<string, int> tagIds = GetTagIds();
            foreach (KeyValuePair<string, int> pair in tagIds)
            {
                string tagName = pair.Key;
                int id = pair.Value;

                // add tvq data
                for (int i = 0; i < 5; i++)
                {
                    streamElements.Add(new StreamElement()
                    {
                        Data = new DataElement()
                        {
                            TagId = id,
                            Timestamp = now.AddTicks(i).ToGrpcTimestamp(),
                            Value = (i % 100).ToVariant(),
                            Quality = 192
                        }
                    });
                }

                // add property data
                streamElements.Add(new StreamElement()
                {
                    Property = new PropertyElement()
                    {
                        TagId = id,
                        NullableDescription = null,
                        Timestamp = now.ToGrpcTimestamp(),
                        Name = "ScaleMax",
                        Value = 100.ToVariant(),
                        Quality = (int)Qualities.Good
                    }
                });

                // add property data
                streamElements.Add(new StreamElement()
                {
                    Property = new PropertyElement()
                    {
                        TagId = id,
                        NullableDescription = null,
                        Timestamp = now.ToGrpcTimestamp(),
                        Name = "ScaleLow",
                        Value = 0.ToVariant(),
                        Quality = (int)Qualities.Good
                    }
                });

                // add property data
                streamElements.Add(new StreamElement()
                {
                    Property = new PropertyElement()
                    {
                        TagId = id,
                        NullableDescription = null,
                        Timestamp = now.ToGrpcTimestamp(),
                        Name = "SampleInterval",
                        Value = TimeSpan.FromSeconds(1).ToString().ToVariant(),
                        Quality = (int)Qualities.Good
                    }
                });

                // add annotation
                streamElements.Add(new StreamElement()
                {
                    Annotation = new AnnotationElement()
                    {
                        CreationTimestamp = now.ToGrpcTimestamp(), // only necessary if creation time differs from annotation timestamp
                        TagId = id,
                        Timestamp = now.ToGrpcTimestamp(),
                        CreationUser = "Example User",
                        Value = "Example Annotation".ToVariant()
                    }
                });
            }

            await session.WriteAsync(streamElements, CancellationToken.None);
        }

        private static IGrpcApiSession CreateSenderSession()
        {
            List<ConfigureSettingRequest> settings = new List<ConfigureSettingRequest>()
            {
                new ConfigureSettingRequest { Kind = SettingKind.IsCreateDataSetEnabled, Value = true.ToVariant() },
                new ConfigureSettingRequest { Kind = SettingKind.IsWriteNoDataOnCloseEnabled, Value = false.ToVariant() },
                new ConfigureSettingRequest { Kind = SettingKind.IsTimestampExtensionEnabled, Value = true.ToVariant() },
                new ConfigureSettingRequest { Kind = SettingKind.IsInsertOrReplaceDataEnabled, Value = false.ToVariant() }
            };

            GrpcApiSessionBuilder builder = new GrpcApiSessionBuilder(new LocalEndpointContext());
            var sessionContext = new SessionContext() { CollectorType = "SAF_Example_Collector", Name = "SAF_Example", NullableDestination = "localhost" };
            var session = builder.Build(sessionContext);
            session.ConfigureSettings(settings);
            return session;
        }

        private IGrpcApiSession Connect()
        {
            _session ??= CreateSenderSession();
            return _session;
        }
    }
}