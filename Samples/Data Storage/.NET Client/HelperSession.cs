using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// helper
using SAF_Helper;
using SAF_Helper.SAF_SenderService;

namespace SAF_Examples
{
    public class HelperSessionExample
    {
        private string _historian = "localhost";
        private string _dataset = "SessionExample";
        private HelperSession _session = null;
        private Dictionary<string, int> _tagMap = new Dictionary<string, int>();

        public Setting BuildSetting(string name, object value)
        {
            Setting setting = new Setting();
            setting.name = name;
            setting.value = value;
            return setting;
        }

        public void Connect()
        {
            if (_session != null)
                return;

            _session = new HelperSession();

            // arbitrary settings used for example code
            string clientId = "SessionExample";
            List<Setting> settings = new List<Setting>();
            settings.Add(BuildSetting("AutoCreateDataSets", true));
            settings.Add(BuildSetting("PacketDelay", 500));

            string result = _session.Connect(_historian, clientId, settings.ToArray());
        }

        public void GetSessionId()
        {
            // NOTE: the helper session class will keep track of the session id
            // internally so that it is not required when making calls
        }

        public Dictionary<string, int> GetTagIds()
        {
            Connect();

            int tagCount = 1;
            if (_tagMap.Count != tagCount)
            {
                Tag[] tags = new Tag[tagCount];
                for (int i = 0; i < tagCount; i++)
                {
                    Tag tag = new Tag();
                    tag.name = String.Format("{0}.Tag {1:D4}", _dataset, i + 1);
                    tag.transformEquation = null;
                    tag.timeExtension = true;
                    tags[i] = tag;
                }

                int[] tagIds;
                string error = _session.GetTagIds(tags, out tagIds);
                if (error != null)
                {
                    // handle error
                    return null;
                }

                for (int i = 0; i < tagCount; i++)
                    _tagMap.Add(tags[i].name, tagIds[i]);
            }

            return _tagMap;
        }

        public string StoreData()
        {
            Connect();

            List<TVQ> tvqs = new List<TVQ>();
            List<Property> properties = new List<Property>();
            List<Annotation> annotations = new List<Annotation>();

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
                    TVQ tvq = new TVQ();
                    tvq.id = id;
                    tvq.timestamp = now.AddTicks(i);
                    tvq.value = i % 100;
                    tvq.quality = StandardQualities.Good;
                    tvqs.Add(tvq);
                }

                // add property data
                Property highScale = new Property();
                highScale.id = id;
                highScale.description = null;
                highScale.timestamp = now;
                highScale.name = StandardPropertyNames.ScaleHigh;
                highScale.value = 100;
                highScale.quality = StandardQualities.Good;
                properties.Add(highScale);

                // add property data
                Property lowScale = new Property();
                lowScale.id = id;
                lowScale.description = null;
                lowScale.timestamp = now;
                lowScale.name = StandardPropertyNames.ScaleLow;
                lowScale.value = 0;
                lowScale.quality = StandardQualities.Good;
                properties.Add(lowScale);

                // add property data
                Property sampleInterval = new Property();
                sampleInterval.id = id;
                sampleInterval.description = null;
                sampleInterval.timestamp = now;
                sampleInterval.name = StandardPropertyNames.SampleInterval;
                sampleInterval.value = TimeSpan.FromSeconds(1);
                sampleInterval.quality = StandardQualities.Good;
                properties.Add(sampleInterval);

                // add annotation
                Annotation annotation = new Annotation();
                annotation.id = id;
                annotation.timestamp = now;
                //annotation.createdAt = now; // only necessary if creation time differs from annotation timestamp
                annotation.user = "Example User";
                annotation.value = "Example Annotation";
                annotations.Add(annotation);
            }

            // send only tvqs in this call
            //string result = session.StoreTVQs(tvqs.ToArray());

            // send only properties in this call
            //string result = session.StoreProperties(properties.ToArray());

            // send only annotations in this call
            //string result = session.StoreAnnotations(annotations.ToArray());

            // send tvqs, properties, and annotations in this call
            string result = _session.StoreData(tvqs.ToArray(), properties.ToArray(), annotations.ToArray());

            return result;
        }

        // NOTE: this method of writing data will buffer the added items in the 
        // sessions class and send all items when the FlushData method is called
        public string StoreData2()
        {
            Connect();

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
                    _session.AddTVQ(tvq);
                }

                // add property data
                Property highScale = new Property();
                highScale.id = id;
                highScale.description = null;
                highScale.timestamp = now;
                highScale.name = StandardPropertyNames.ScaleHigh;
                highScale.value = 100;
                highScale.quality = StandardQualities.Good;
                _session.AddProperty(highScale);

                // add property data
                Property lowScale = new Property();
                lowScale.id = id;
                lowScale.description = null;
                lowScale.timestamp = now;
                lowScale.name = StandardPropertyNames.ScaleLow;
                lowScale.value = 0;
                lowScale.quality = StandardQualities.Good;
                _session.AddProperty(lowScale);

                // add property data
                Property sampleInterval = new Property();
                sampleInterval.id = id;
                sampleInterval.description = null;
                sampleInterval.timestamp = now;
                sampleInterval.name = StandardPropertyNames.SampleInterval;
                sampleInterval.value = TimeSpan.FromSeconds(1);
                sampleInterval.quality = StandardQualities.Good;
                _session.AddProperty(sampleInterval);

                // add annotation
                Annotation annotation = new Annotation();
                annotation.id = id;
                annotation.timestamp = now;
                //annotation.createdAt = now; // only necessary if creation time differs from annotation timestamp
                annotation.user = "Example User";
                annotation.value = "Example Annotation";
                _session.AddAnnotation(annotation);
            }

            int storedTVQs;
            int storedProperties;
            int storedAnnotations;
            return _session.FlushData(out storedTVQs, out storedProperties, out storedAnnotations);
        }

        public string Disconnect()
        {
            return _session.Disconnect();
        }
    }
}
