using Canary.StoreAndForward2.Grpc.Api;
using Canary.StoreAndForward2.Grpc.Api.Helper;
using Canary.Utility.ProtobufSharedTypes;

namespace SAF_Examples
{
    // This class uses extension methods that simplify tag configuration and storing data
    public class ExtensionMethodsExample
    {
        private enum Qualities
        {
            Good = 192, // 0xC0
            NoData = -32768, // 0x8000
        }

        private readonly string _dataset = "ExtensionExample";
        private readonly string _historian = "localhost";
        private readonly Dictionary<string, int> _tagMap = [];
        private IGrpcApiSession? _session = null;

        public async Task CreateNewFileAsync()
        {
            var session = Connect();

            HistorianFile file = new()
            {
                DataSet = _dataset,
                FileTime = DateTime.Now
            };

            await session.CreateNewFileAsync(file, CancellationToken.None);
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

            HistorianFile file = new()
            {
                DataSet = _dataset,
                FileTime = DateTime.MinValue
            };

            await session.RollOverFileAsync(file, CancellationToken.None);
        }

        public Dictionary<string, int> GetTagIds()
        {
            var session = Connect();

            if (_tagMap.Count == 0)
            {
                int tagCount = 5;
                Tag[] tags = new Tag[tagCount];
                for (int i = 0; i < tagCount; i++)
                {
                    tags[i] = new Tag
                    {
                        Name = string.Format("{0}.Tag {1:D4}", _dataset, i + 1),
                        TransformEquation = null,
                        TimeExtension = true
                    };
                }

                session.GetTagIds(tags, out int[] tagIds);

                for (int i = 0; i < tagCount; i++)
                    _tagMap.Add(tags[i].Name, tagIds[i]);
            }

            return _tagMap;
        }

        public async Task NoDataAsync()
        {
            var session = Connect();

            Dictionary<string, int> tagIds = GetTagIds();

            await session.StoreNoDataAsync(tagIds.Values, CancellationToken.None);
        }

        public async Task StoreDataAsync()
        {
            var session = Connect();

            List<DataElement> dataElements = [];
            List<PropertyElement> propertyElements = [];
            List<AnnotationElement> annotationElements = [];

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
                    dataElements.Add(new DataElement()
                    {
                        TagId = id,
                        Timestamp = now.AddTicks(i).ToGrpcTimestamp(),
                        Value = (i % 100).ToVariant(),
                        Quality = 192
                    });
                }

                // add property data
                propertyElements.Add(new PropertyElement()
                {
                    TagId = id,
                    NullableDescription = null,
                    Timestamp = now.ToGrpcTimestamp(),
                    Name = "ScaleMax",
                    Value = 100.ToVariant(),
                    Quality = (int)Qualities.Good
                });

                // add property data
                propertyElements.Add(new PropertyElement()
                {
                    TagId = id,
                    NullableDescription = null,
                    Timestamp = now.ToGrpcTimestamp(),
                    Name = "ScaleLow",
                    Value = 0.ToVariant(),
                    Quality = (int)Qualities.Good
                });

                // add property data
                propertyElements.Add(new PropertyElement()
                {
                    TagId = id,
                    NullableDescription = null,
                    Timestamp = now.ToGrpcTimestamp(),
                    Name = "SampleInterval",
                    Value = TimeSpan.FromSeconds(1).ToString().ToVariant(),
                    Quality = (int)Qualities.Good
                });

                // add annotation
                annotationElements.Add(new AnnotationElement()
                {
                    CreationTimestamp = now.ToGrpcTimestamp(), // only necessary if creation time differs from annotation timestamp
                    TagId = id,
                    Timestamp = now.ToGrpcTimestamp(),
                    CreationUser = "Example User",
                    Value = "Example Annotation".ToVariant()
                });
            }

            await session.AddTvqsAsync(dataElements, CancellationToken.None);
            await session.AddPropertiesAsync(propertyElements, CancellationToken.None);
            await session.WriteAsync(annotationElements.Select(p => new StreamElement() { Annotation = p }), CancellationToken.None); // No extension method for annotations
        }

        private static IGrpcApiSession CreateSenderSession()
        {
            List<ConfigureSettingRequest> settings = new()
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