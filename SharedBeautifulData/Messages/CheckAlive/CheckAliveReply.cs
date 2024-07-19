using Newtonsoft.Json;
using Remote.Communication.Common.Implementations;

namespace SharedBeautifulData.Messages.CheckAlive
{
    public class CheckAliveReply : BaseMessage<bool>
    {
        [JsonIgnore]
        public bool Success
        {
            get => MessageObject;
            set => MessageObject = value;
        }
    }
}