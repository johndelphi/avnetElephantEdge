using IOTConnectSDK;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IOTConnect_SDK_Sample
{
    class Program
    {
        static SDKClient client;
        static void Main(string[] args)
        {
            //Get your ENV and CPID from the portal key vaults module or visit https://help.iotconnect.io SDK section.
            string cpId = "14cf4939849c4183ac5c74660ca9ab27", uniqueId = "elephantedge1", env = "Avnet";

            if (string.IsNullOrWhiteSpace(cpId))
            {
                Console.WriteLine("cpId can not be blank.");
                return;
            }

            if (string.IsNullOrWhiteSpace(uniqueId))
            {
                Console.WriteLine("uniqueId can not be blank.");
                return;
            }

            //Initialize device sdk client to connect device. 
            client = new SDKClient(cpId, uniqueId, DeviceCallback, TwinUpdateCallBack, env);

            //Prepare device data with valid format.[Type1]
            var deviceData = "[{\"uniqueId\":\"<deviceUniqueId>\",\"time\":\"2020-02-14T12:18:23.5499042Z\",\"d\":[{\"humidity\":13}]}]";

            //Send data to device using SendData method.
            client.SendData(deviceData);

            Console.ReadLine();
        }

        private static Task TwinUpdateCallBack(Dictionary<string, object> arg)
        {
            //TODO : TwinUpdateCallBack
            //NOTE: To update twin using sdk method
            //client.UpdateTwin("twinName", "value");
            return Task.CompletedTask;
        }

        private static Task DeviceCallback(string message)
        {
            /* Received command format for DeviceCommand from IOTConnect.Net SDK version < 2.0 */
            //{"cpId":"<cpid>","guid":"00000000-0000-0000-0000-000000000000","uniqueId":"<deviceUniqueId>","command":"<Command> Command_Text","value":null,"ack":true,"ackId":"00000000-0000-0000-0000-000000000000"}

            /* Received command format for Firmware/OTA update from IOTConnect.Net SDK version < 2.0 */
            //{"cpId":"<cpid>","guid":"00000000-0000-0000-0000-000000000000","uniqueId":"<deviceUniqueId>","command":"<Command> OTA_Update_Url","value":null,"ack":false,"ackId":"00000000-0000-0000-0000-000000000000"}

            /* Received command format for DeviceCommand from IOTConnect.Net SDK version 2.0 */
            //{"cpId":"<cpid>","guid":"00000000-0000-0000-0000-000000000000","uniqueId":"<deviceUniqueId>","command":"<Command> Command_Text","value":null,"ack":true,"ackId":"00000000-0000-0000-0000-000000000000","cmdType": "0x01"}

            /* Received command format for Firmware/OTA update from IOTConnect.Net SDK version 2.0 */
            //{"cpId":"<cpid>","guid":"00000000-0000-0000-0000-000000000000","uniqueId":"<deviceUniqueId>","command":"<Command>","value":null,"ack":false,"ackId":"00000000-0000-0000-0000-000000000000","cmdType": "0x02","urls": [{"url": "<URL1>"}, {"url": "<URL2>"}]}


            //"mt" = 5 - DeviceCommandAck, 11 - OtaAck/FirmwareUpgradeAck
            //"st" = 4 - Failed, 6 - ExecutedAck[DeviceCommandAck], 7 - Success[OtaAck/FirmwareUpgradeAck]
            try
            {
                var commandData = JsonConvert.DeserializeObject<CommandData>(message);

                DeviceAckModel ackDetails = new DeviceAckModel()
                {
                    AckId = commandData.AcknowledgeId,
                    ChildId = ""
                };

                //IOTConnect.Net SDK version < 2.0
                bool isOta = (commandData != null && !string.IsNullOrWhiteSpace(commandData.Command) && commandData.Command.StartsWith("<CommandPrefix> ", StringComparison.CurrentCultureIgnoreCase));

                //IOTConnect.Net SDK version >= 2.0
                isOta = commandData.CommandType.Equals("0x02", StringComparison.CurrentCultureIgnoreCase);

                if (isOta)
                {
                    //TODO : download and save ota file from url [commandData.Command]
                    ackDetails.Msg = "OTA updated successfully.";
                    ackDetails.Status = 7;
                }
                else
                {
                    ackDetails.Msg = "Device command received successfully.";
                    ackDetails.Status = 6;
                }

                var dataOta = JsonConvert.SerializeObject(ackDetails);

                //NOTE: SendAck method have different arguments as per the SDK version. Choose accordingly.
                //IOTConnect.Net SDK version < 2.0
                //client.SendAck(dataOta, isOta ? 11 : 5);

                //IOTConnect.Net SDK version >= 2.0
                client.SendAck(dataOta, "2020-02-14T12:19:33.7769262Z", isOta ? 11 : 5);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Task.CompletedTask;
        }
    }

    public class DeviceAckModel
    {
        [JsonProperty("ackId")]
        public string AckId { get; set; }
        [JsonProperty("st")]
        public int Status { get; set; }
        [JsonProperty("msg")]
        public string Msg { get; set; }
        [JsonProperty("childId")]
        public string ChildId { get; set; }
    }

    public class CommandData
    {
        [JsonProperty("cpId")]
        public string CpId { get; set; }

        [JsonProperty("guid")]
        public string Guid { get; set; } //Device Guid

        [JsonProperty("uniqueId")]
        public string UniqueId { get; set; } //Device unique id

        [JsonProperty("command")]
        public string Command { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("ack")]
        public bool Acknowledge { get; set; }

        [JsonProperty("ackId")]
        public string AcknowledgeId { get; set; }

        [JsonProperty("cmdType")]
        public string CommandType { get; set; }

        [JsonProperty("urls")]
        public object Urls { get; set; }
    }
}