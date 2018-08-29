using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// helper
using SAF_Helper;
using SAF_Helper.SAF_SenderService;

namespace SAF_Examples
{
    public class HelperClassExample
    {
        string host = "localhost"; // necessary when using named pipe binding (other bindings could be configured if necessary)
        SAFSenderServiceContractClient client = null;
        string sessionId = null;
        Dictionary<string, int> tagMap = new Dictionary<string, int>();

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
                string clientId = "HelperExample";

                // arbitrary settings used for example code
                List<Setting> settings = new List<Setting>();
                settings.Add(BuildSetting(SettingNames.AutoCreateDatasets, true));
                settings.Add(BuildSetting(SettingNames.PacketDelay, 500));
                settings.Add(BuildSetting(SettingNames.TrackErrors, true));

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

        public void Connect()
        {
            if (client == null)
            {
                ConnectionType connectionType = ConnectionType.NetPipe_Anonymous;
                string host = null;
                string usernameCredentials = null;
                string passwordCredentials = null;
                HelperClass.Connect(connectionType, host, usernameCredentials, passwordCredentials, out client);
            }
        }

        public Dictionary<string, int> GetTagIds()
        {
            Connect();

            int tagCount = 4;
            if (tagMap.Count != tagCount)
            {
                bool failed;
                string sessionId = GetSessionId();

                string dataSet = "HelperExample";
                Tag[] tags = new Tag[tagCount];
                for (int i = 0; i < tagCount; i++)
                {
                    Tag tag = new Tag();
                    tag.name = String.Format("{0}.Tag {1:D4}", dataSet, i + 1);
                    tag.transformEquation = null;
                    tag.timeExtension = true;
                    tags[i] = tag;
                }

                // no time extension
                object[] results = HelperClass.GetTagIds(out failed, client, sessionId, tags);

                if (failed)
                {
                    for (int i = 0; i < tagCount; i++)
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
                    for (int i = 0; i < tagCount; i++)
                    {
                        int id = (int)results[i];
                        tagMap.Add(tags[i].name, id);
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
                    tvq.quality = StandardQualities.Good;
                    tvqsList.Add(tvq);
                }

                // add property data
                Property highScale = new Property();
                highScale.id = id;
                highScale.description = null;
                highScale.timestamp = now;
                highScale.name = StandardPropertyNames.ScaleHigh;
                highScale.value = 100;
                highScale.quality = StandardQualities.Good;
                propertiesList.Add(highScale);

                // add property data
                Property lowScale = new Property();
                lowScale.id = id;
                lowScale.description = null;
                lowScale.timestamp = now;
                lowScale.name = StandardPropertyNames.ScaleLow;
                lowScale.value = 0;
                lowScale.quality = StandardQualities.Good;
                propertiesList.Add(lowScale);

                // add property data
                Property sampleInterval = new Property();
                sampleInterval.id = id;
                sampleInterval.description = null;
                sampleInterval.timestamp = now;
                sampleInterval.name = StandardPropertyNames.SampleInterval;
                sampleInterval.value = TimeSpan.FromSeconds(1);
                sampleInterval.quality = StandardQualities.Good;
                propertiesList.Add(sampleInterval);

                // add annotation
                Annotation annotation = new Annotation();
                annotation.id = id;
                annotation.timestamp = now;
                //annotation.createdAt = now; // only necessary if creation time differs from annotation timestamp
                annotation.user = "Example User";
                annotation.value = "Example Annotation";
                annotationsList.Add(annotation);
            }

            int tvqsStored;
            int propertiesStored;
            int annotationsStored;
            string sessionId = GetSessionId();
            TVQ[] tvqs = tvqsList.ToArray();
            Property[] properties = propertiesList.ToArray();
            Annotation[] annotations = annotationsList.ToArray();

            // send only tvqs in this call
            //string result = SAF_HelperClass.StoreData(client, sessionId, tvqs, out tvqsStored);

            // send only properties in this call
            //string result = SAF_HelperClass.StoreData(client, sessionId, properties, out propertiesStored);

            // send only annotations in this call
            //string result = SAF_HelperClass.StoreData(client, sessionId, annotations, out annotationsStored);

            // send tvqs, properties, and annotations in this call
            string result = HelperClass.StoreData(client, sessionId, tvqs, properties, annotations, out tvqsStored, out propertiesStored, out annotationsStored);

            return result;
        }

        public bool IsLocalhost()
        {
            bool result = HelperClass.IsLocalhost(host);
            return result;
        }

        public bool TryParseEndpoint()
        {
            ConnectionType connectionType;
            string host;
            int port;

            string endpoint = "net.pipe://localhost/saf/sender/anonymous";
            bool result = HelperClass.TryParseEndpoint(endpoint, out connectionType, out host, out port);

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
