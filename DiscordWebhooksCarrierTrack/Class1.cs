using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using System.Drawing;
using Discord.Webhook;

namespace DiscordWebhooksCarrierTrack
{
    public class Class1
    {
        private class Config
        {
            public string WebhookLink;
            public string WebhookName;

            public string LastJumpSystem;
            public string LastJumpBak;

            public string CarrierName;
            public string CarrierIdent;

            public DateTime? LastJumpReq;
        }
        public void Main(string Event)
        {
            Run(Event);
        }
        private void Run(string Event)
        {
            if (!File.Exists("CarrierTrack.json"))
            {
                StreamWriter sw = File.CreateText("CarrierTrack.json");
                sw.Write(JsonConvert.SerializeObject(new Config() { WebhookLink = "Webhook link", WebhookName = "Something|CMDR Name|Carrier name", LastJumpSystem = "\\*\\*\\*" }, Formatting.Indented));
                sw.Close();
            }
            else
            {
                Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("CarrierTrack.json"));
                try
                {
                    CommonEvent ev = JsonConvert.DeserializeObject<CommonEvent>(Event);
                    if (ev.Event == "CarrierStats")
                    {
                        CarrierStatsInfo info = JsonConvert.DeserializeObject<CarrierStatsInfo>(Event);
                        config.CarrierName = info.Name;
                        config.CarrierIdent = info.Callsign;
                    }
                    else if (ev.Event == "CarrierJumpRequest")
                    {
                        CarrierJumpRequestInfo info = JsonConvert.DeserializeObject<CarrierJumpRequestInfo>(Event);

                        DiscordWebhook webhook = new DiscordWebhook();
                        webhook.Url = config.WebhookLink;
                        webhook.Send(new Discord.DiscordMessage() { Username = config.WebhookName, Embeds = new List<Discord.DiscordEmbed> { new Discord.DiscordEmbed() { Title = "Запланирован прыжок", Description = $"Запланирован прыжок носителя {config.CarrierName} {config.CarrierIdent} в систему {info.SystemName} к телу {info.Body} из системы {config.LastJumpSystem}", Color = Color.Blue } } });
                        webhook = null;
                        config.LastJumpSystem = info.SystemName;
                        config.LastJumpReq = DateTime.UtcNow;
                    }
                    else if (ev.Event == "CarrierJumpCancelled")
                    {
                        config.LastJumpSystem = config.LastJumpBak;
                        config.LastJumpReq = null;

                        DiscordWebhook webhook = new DiscordWebhook();
                        webhook.Url = config.WebhookLink;
                        webhook.Send(new Discord.DiscordMessage() { Username = config.WebhookName, Embeds = new List<Discord.DiscordEmbed> { new Discord.DiscordEmbed() { Title = "Прыжок отменён", Description = $"Прыжок носителя {config.CarrierName} {config.CarrierIdent} отменён", Color = Color.Red } } });
                        webhook = null;
                    }
                    else if (ev.Event == "CarrierNameChange")
                    {
                        CarrierNameChangeInfo info = JsonConvert.DeserializeObject<CarrierNameChangeInfo>(Event);
                        DiscordWebhook webhook = new DiscordWebhook();
                        webhook.Url = config.WebhookLink;
                        webhook.Send(new Discord.DiscordMessage() { Username = config.WebhookName, Embeds = new List<Discord.DiscordEmbed> { new Discord.DiscordEmbed() { Title = "Изменение имени носителя", Description = $"Имя носителя изменилось на {info.Name}", Color = Color.Brown } } });
                        webhook = null;
                    }
                    else if (ev.Event == "CarrierDockingPermission")
                    {
                        CarrierDockingPermissionInfo info = JsonConvert.DeserializeObject<CarrierDockingPermissionInfo>(Event);
                        DiscordWebhook webhook = new DiscordWebhook();
                        webhook.Url = config.WebhookLink;
                        webhook.Send(new Discord.DiscordMessage() { Username = config.WebhookName, Embeds = new List<Discord.DiscordEmbed> { new Discord.DiscordEmbed() { Title = "Изменение разрешения на стыковку", Description = $"Носитель {config.CarrierName} {config.CarrierIdent} сменил разрешение на стыковку на {info.DockingAccess}\nСтыковка для преступников сейчас {(info.AllowNotorious ? "разрешена" : "запрещена")}", Color = Color.Gray } } });
                        webhook = null;
                    }
                    if (config.LastJumpReq != null && (DateTime.UtcNow - config.LastJumpReq).Value.TotalMinutes > 16.15d)
                    {
                        DiscordWebhook webhook = new DiscordWebhook();
                        webhook.Url = config.WebhookLink;
                        webhook.Send(new Discord.DiscordMessage() { Username = config.WebhookName, Embeds = new List<Discord.DiscordEmbed> { new Discord.DiscordEmbed() { Title = "Прыжок совершён", Description = $"Носитель {config.CarrierName} {config.CarrierIdent} совершил прыжок в систему {config.LastJumpSystem} из системы {config.LastJumpBak}", Color = Color.LightGreen } } });
                        webhook = null;

                        config.LastJumpBak = config.LastJumpSystem;
                        config.LastJumpReq = null;
                    }
                }
                catch
                {

                }
                finally
                {
                    StreamWriter sw = File.CreateText("CarrierTrack.json");
                    sw.Write(JsonConvert.SerializeObject(config, Formatting.Indented));
                    sw.Close();
                }
            }
        }
        private class CommonEvent
        {
            [JsonProperty("event")]
            public string Event;
        }
        private class CarrierJumpRequestInfo : CommonEvent
        {
            public string SystemName;
            public string Body;
        }
        private class CarrierStatsInfo : CommonEvent
        {
            public string Callsign;
            public string Name;
        }
        private class CarrierNameChangeInfo : CommonEvent
        {
            public string Name;
        }
        private class CarrierDockingPermissionInfo : CommonEvent
        {
            public string DockingAccess;
            public bool AllowNotorious;
        }
    }
}
