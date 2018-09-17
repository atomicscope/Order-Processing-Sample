using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AtomicScope.Client
{
    /// <summary>
    /// Sample atomic scope client
    /// </summary>
    public class AsActivityLogger
    {
        /// <summary>
        /// The API resource identifier
        /// </summary>
        public readonly Guid ApiResourceId;
        /// <summary>
        /// The business process name
        /// </summary>
        public readonly string BusinessProcessName;
        /// <summary>
        /// The transaction name
        /// </summary>
        public readonly string TransactionName;

        /// <summary>
        /// The current stage
        /// </summary>
        public string CurrentStage;

        /// <summary>
        /// The previous stage
        /// </summary>
        private string _previousStage;

        private Guid _currentMainActivityId;
        private Guid _currentStageActivityId;

        /// <summary>
        /// The base URL
        /// </summary>
        private readonly string _baseUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsActivityLogger"/> class.
        /// </summary>
        /// <param name="apiResourceId">The resource guid id from Atomic Scope portal</param>
        /// <param name="businessProcessName">The business process name configured in the Atomic Scope portal.</param>
        /// <param name="transactionName">The transaction name configure in Atomic Scope portal.</param>
        public AsActivityLogger(Guid apiResourceId, string businessProcessName, string transactionName)
        {
            ApiResourceId = apiResourceId;
            BusinessProcessName = businessProcessName;
            TransactionName = transactionName;
            _baseUrl = "http://localhost/atomicscope/activities";
            CurrentStage = ".";
        }

        /// <summary>
        /// Starts an AtomicScope activity.
        /// </summary>
        public void Start(string currentStage)
        {
            _previousStage = CurrentStage;
            CurrentStage = currentStage;
            JObject startActivityResponse;

            var startActivityMessageBody = new JObject
            {
                //Message Body for StartActivity 
                { "previousStage", _previousStage },
                { "startedOn", "" }
            };
            // get main activity id from request and update current main activity

            if (_currentMainActivityId == Guid.Empty)
            {
                startActivityResponse = JsonConvert.DeserializeObject<JObject>(Post(currentStage, "start", startActivityMessageBody).Result);
                _currentMainActivityId = (Guid)startActivityResponse["mainActivityId"];
                _currentStageActivityId = (Guid)startActivityResponse["stageActivityId"];
                return;
            }

            startActivityMessageBody.Add("MainactivityId", _currentMainActivityId);
            startActivityResponse = JsonConvert.DeserializeObject<JObject>(Post(currentStage, "start", startActivityMessageBody).Result);
            _currentStageActivityId = (Guid)startActivityResponse["stageActivityId"];
        }

        /// <summary>
        /// Updates as AtomicScope activity.
        /// </summary>
        public void Update(IDictionary<string, string> trackedProperties)
        {
            var updateActivityMessageBody = new UpdateActivityMessageBody
            {
                MainActivityId = _currentMainActivityId,
                StageActivityId = _currentStageActivityId,
                StageStatus = 2,
                TrackedProperties = trackedProperties
            };

            var updateActivityResponse = Post(CurrentStage, "update", updateActivityMessageBody);
        }

        /// <summary>
        /// Archives data.
        /// </summary>
        public void Archive(dynamic messageBody)
        {
            Post(CurrentStage, "archive", messageBody);
        }


        /// <summary>
        /// Logs this instance.
        /// </summary>
        public async void Log(string currentStage, IDictionary<string, string> trackedProperties)
        {
            _previousStage = CurrentStage;
            CurrentStage = currentStage;
            var logActivityMessageBody = new LogActivityMessageBody
            {
                MainActivityId = _currentMainActivityId,
                PreviousStage = _previousStage,
                TrackedProperties = trackedProperties
            };
            await Post(CurrentStage, "log", logActivityMessageBody);

        }
        public async Task<string> Post(string stageName, string activityName, dynamic messageBody)
        {
            var url = _baseUrl + "/" + activityName;

            if (activityName == "archive")
            {
                url = _baseUrl + "/" + activityName + "?stageactivityid=" + _currentStageActivityId;
            }

            var client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });
            var jsonData = JsonConvert.SerializeObject(messageBody);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Add("AS-ResourceId", Convert.ToString(ApiResourceId));
            client.DefaultRequestHeaders.Add("AS-BusinessProcess", BusinessProcessName);
            client.DefaultRequestHeaders.Add("AS-BusinessTransaction", TransactionName);
            client.DefaultRequestHeaders.Add("AS-CurrentStage", stageName);
            var respone = await client.PostAsync(url, content);
            return await respone.Content.ReadAsStringAsync();
        }
    }
    public class UpdateActivityMessageBody
    {
        public Guid MainActivityId { get; set; }
        public Guid StageActivityId { get; set; }
        public int StageStatus { get; set; }
        public IDictionary<string, string> TrackedProperties { get; set; }
    }

    public class LogActivityMessageBody
    {
        public Guid MainActivityId { get; set; }
        public string PreviousStage { get; set; }
        public IDictionary<string, string> TrackedProperties { get; set; }
    }
}
