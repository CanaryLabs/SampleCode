using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

// helper
using SAF_Helper.SAF_SenderService;

namespace SAF_Examples
{
    public class RawClientExample
    {
        string host = "localhost"; // necessary when using named pipe binding (other bindings could be configured if necessary)
        SAFSenderServiceContractClient client = null;
        string sessionId = null;
        Dictionary<string, int> tagMap = new Dictionary<string, int>();

        static private NetNamedPipeBinding CreateNamedPipeBindingBase()
        {
            NetNamedPipeBinding pipeBinding = new NetNamedPipeBinding();
            pipeBinding.MaxBufferPoolSize = 2147483647;
            pipeBinding.MaxReceivedMessageSize = 2147483647;
            pipeBinding.MaxConnections = 50;
            pipeBinding.ReaderQuotas.MaxStringContentLength = 2147483647;
            pipeBinding.ReaderQuotas.MaxArrayLength = 2147483647;
            pipeBinding.ReaderQuotas.MaxBytesPerRead = 2147483647;
            pipeBinding.ReceiveTimeout = TimeSpan.FromHours(1);

            return pipeBinding;
        }

        private void Connect()
        {
            if (client == null)
            {
                NetNamedPipeBinding pipeBinding = CreateNamedPipeBindingBase();
                pipeBinding.Security.Mode = NetNamedPipeSecurityMode.None;
                System.ServiceModel.Channels.Binding binding = pipeBinding;

                // host and port are ignored because this is always local
                string address = "net.pipe://localhost/saf/sender/anonymous";
                EndpointAddress endpoint = new EndpointAddress(new Uri(address));
                client = new SAFSenderServiceContractClient(binding, endpoint);
            }
        }

        public string InterfaceVersion()
        {
            Connect();

            return client.Version();
        }

        public string[] GetDataSets()
        {
            Connect();

            bool failed;
            string historian = host;

            string[] results = client.GetDataSets(out failed, historian);
            if (failed)
            {
                // handle error
                string error = results[0];
            }

            return results;
        }

        public Setting BuildSetting(string name, object value)
        {
            Setting setting = new Setting();
            setting.name = name;
            setting.value = value;
            return setting;
        }

        public string GetSessionId()
        {
            Connect();

            if (sessionId == null)
            {
                bool failed;
                string historian = host;
                string clientId = "RawExample";

                // arbitrary settings used for example code
                List<Setting> settings = new List<Setting>();
                settings.Add(BuildSetting("AutoCreateDataSets", true));
                settings.Add(BuildSetting("PacketDelay", 500));
                settings.Add(BuildSetting("TrackErrors", true));

                string result = client.GetSessionId(out failed, historian, clientId, settings.ToArray());
                if (failed)
                {
                    // handle error
                    string error = result;
                }
                else
                    sessionId = result;
            }

            return sessionId;
        }

        public string[] UpdateSettings()
        {
            Connect();

            bool failed;
            string sessionId = GetSessionId();

            // arbitrary settings used for example code
            List<Setting> settings = new List<Setting>();
            settings.Add(BuildSetting("PacketDelay", 0));
            settings.Add(BuildSetting("TrackErrors", false));

            string[] results = client.UpdateSettings(out failed, sessionId, settings.ToArray());
            if (failed)
            {
                foreach (string result in results)
                {
                    if (result != null)
                    {
                        // handle error
                        string error = result;
                    }
                }
            }

            return results;
        }

        public Dictionary<string, int> GetTagIds()
        {
            Connect();

            int tagCount = 4;
            if (tagMap.Count != tagCount)
            {
                bool failed;
                string sessionId = GetSessionId();

                string dataSet = "RawExample";
                List<Tag> tags = new List<Tag>();
                for (int i = 0; i < tagCount; i++)
                {
                    Tag tag = new Tag();
                    tag.name = String.Format("{0}.Tag {1:D4}", dataSet, i + 1);
                    tag.transformEquation = "Value * 10";
                    tag.timeExtension = true;
                    tags.Add(tag);
                }

                object[] results = client.GetTagIds(out failed, sessionId, tags.ToArray());
                if (failed)
                {
                    for (int i = 0; i < tags.Count; i++)
                    {
                        object result = results[i];
                        if (!(result is int))
                        {
                            // handle error
                            string error = (string)result;
                        }
                    }
                    return tagMap;
                }
                else
                {
                    // create tag mapping to reference id
                    for (int i = 0; i < tags.Count; i++)
                    {
                        string tagName = tags[i].name;
                        int id = (int)results[i];
                        tagMap.Add(tagName, id);
                    }
                }
            }

            return tagMap;
        }

        public string StoreData()
        {
            Connect();

            List<TVQ> tvqsList = new List<TVQ>();
            List<Property> propertiesList = new List<Property>();
            List<Annotation> annotationsList = new List<Annotation>();

            // create data to store
            DateTime now = DateTime.Now;
            Dictionary<string, int> tagIds = GetTagIds();
            foreach (KeyValuePair<string, int> pair in tagIds)
            {
                string tagName = pair.Key;
                int id = pair.Value;

                // add tvq data
                for (int i = 0; i < 500; i++)
                {
                    TVQ tvq = new TVQ();
                    tvq.id = id;
                    tvq.timestamp = now.AddTicks(i);
                    tvq.value = i % 100;
                    tvq.quality = 0xC0;
                    tvqsList.Add(tvq);
                }

                // add property data
                Property highScale = new Property();
                highScale.id = id;
                highScale.description = null;
                highScale.timestamp = now;
                highScale.name = "Default High Scale";
                highScale.value = 100;
                highScale.quality = 0xC0;
                propertiesList.Add(highScale);

                // add property data
                Property lowScale = new Property();
                lowScale.id = id;
                lowScale.description = null;
                lowScale.timestamp = now;
                lowScale.name = "Default Low Scale";
                lowScale.value = 0;
                lowScale.quality = 0xC0;
                propertiesList.Add(lowScale);

                // add annotation
                Annotation annotation = new Annotation();
                annotation.id = id;
                annotation.timestamp = now;
                //annotation.createdAt = now; // only necessary if creation time differs from annotation timestamp
                annotation.user = "Example User";
                annotation.value = "Example Annotation";
                annotationsList.Add(annotation);
            }

            bool failed;
            string sessionId = GetSessionId();
            TVQ[] tvqs = tvqsList.ToArray();
            Property[] properties = propertiesList.ToArray();
            Annotation[] annotations = annotationsList.ToArray();

            string result = client.StoreData(out failed, sessionId, tvqs, properties, annotations);
            if (failed)
            {
                // handle error
                string error = result;
            }

            return result;
        }

        public string[] NoData()
        {
            Connect();

            bool failed;
            string sessionId = GetSessionId();
            Dictionary<string, int> tagIds = GetTagIds();

            return client.NoData(out failed, sessionId, tagIds.Values.ToArray());
        }

        public string CreateNewFile()
        {
            Connect();

            bool failed;
            string sessionId = GetSessionId();
            HistorianFile newFile = new HistorianFile();
            newFile.dataSet = "RawExample";
            newFile.fileTime = DateTime.MinValue;

            string result = client.CreateNewFile(out failed, sessionId, newFile);
            if (failed)
            {
                // handle error
                string error = result;
            }

            return result;
        }

        public string FileRollOver()
        {
            Connect();

            bool failed;
            string sessionId = GetSessionId();
            HistorianFile rollOverFile = new HistorianFile();
            rollOverFile.dataSet = "RawExample";
            rollOverFile.fileTime = DateTime.MinValue;

            string result = client.FileRollOver(out failed, sessionId, rollOverFile);
            if (failed)
            {
                // handle error
                string error = result;
            }

            return result;
        }

        public string GetErrors()
        {
            Connect();

            bool failed;
            Errors errors;
            string sessionId = GetSessionId();

            string result = client.GetErrors(out failed, out errors, sessionId);
            if (failed)
            {
                // handle error
                string error = result;
            }

            return result;
        }

        public string ReleaseSession()
        {
            if ((client != null) && (sessionId != null))
            {
                bool failed;
                string result = client.ReleaseSession(out failed, sessionId);
                if (failed)
                {
                    // handle error
                    string error = result;
                }

                // reset stored variables
                this.sessionId = null;
                this.tagMap.Clear();

                return result;
            }

            return null;
        }
    }
}
