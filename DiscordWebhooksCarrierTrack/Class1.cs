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
        public class Config
        {
            public Settings settings;
            public string CurrentSystemGuess;
            public string LastJumpSystemRequest;
            public string TrackName;
            public bool Canceled;

            public string CarrierName;
            public string CarrierIdent;
            public class Settings
            {
                public string WebhookLink;
                public string WebhookName;
                public bool Enabled;
            }
        }
        public void Main(string Event)
        {
            Run(Event);
        }
        private void Run(string Event)
        {
            Config config = new Config();
            if (!File.Exists("CarrierTrackSettings.json"))
            {
                StreamWriter sw = File.CreateText("CarrierTrackSettings.json");
                sw.Write(JsonConvert.SerializeObject(new Config() { settings = new Config.Settings() { WebhookLink = "Webhook link", WebhookName = "Webhook Name", Enabled = true }, LastJumpSystemRequest = "Unknown" }, Formatting.Indented));
                sw.Close();
                config = new Config() { settings = new Config.Settings() { WebhookLink = "Webhook link", WebhookName = "Webhook Name", Enabled = true }, LastJumpSystemRequest = "Unknown" };
            }
            else
            {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("CarrierTrackSettings.json"));
            }
            if (config.settings.Enabled)
            {
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
                        if (!config.Canceled)
                        {
                            config.CurrentSystemGuess = config.LastJumpSystemRequest;
                        }
                        CarrierJumpRequestInfo info = JsonConvert.DeserializeObject<CarrierJumpRequestInfo>(Event);
                        DiscordWebhook webhook = new DiscordWebhook();
                        webhook.Url = config.settings.WebhookLink;
                        if (string.IsNullOrWhiteSpace(info.Body))
                            webhook.Send(new Discord.DiscordMessage() { Username = config.settings.WebhookName, Embeds = new List<Discord.DiscordEmbed> { new Discord.DiscordEmbed() { Title = "Запланирован прыжок", Description = $"Запланирован прыжок носителя {config.CarrierName} {config.CarrierIdent} в систему {info.SystemName} из системы {config.LastJumpSystemRequest}", Color = Color.Blue } } });
                        else
                            webhook.Send(new Discord.DiscordMessage() { Username = config.settings.WebhookName, Embeds = new List<Discord.DiscordEmbed> { new Discord.DiscordEmbed() { Title = "Запланирован прыжок", Description = $"Запланирован прыжок носителя {config.CarrierName} {config.CarrierIdent} в систему {info.SystemName} к телу {info.Body} из системы {config.LastJumpSystemRequest}", Color = Color.Blue } } });
                        webhook = null;
                        config.LastJumpSystemRequest = info.SystemName;
                        config.Canceled = false;
                    }
                    else if (ev.Event == "CarrierJumpCancelled")
                    {
                        config.Canceled = true;
                        DiscordWebhook webhook = new DiscordWebhook();
                        webhook.Url = config.settings.WebhookLink;
                        webhook.Send(new Discord.DiscordMessage() { Username = config.settings.WebhookName, Embeds = new List<Discord.DiscordEmbed> { new Discord.DiscordEmbed() { Title = "Прыжок отменён", Description = $"Прыжок носителя {config.CarrierName} {config.CarrierIdent} отменён", Color = Color.Red } } });
                        webhook = null;
                        config.LastJumpSystemRequest = config.CurrentSystemGuess;
                    }
                    else if (ev.Event == "CarrierNameChange")
                    {
                        CarrierNameChangeInfo info = JsonConvert.DeserializeObject<CarrierNameChangeInfo>(Event);
                        DiscordWebhook webhook = new DiscordWebhook();
                        webhook.Url = config.settings.WebhookLink;
                        webhook.Send(new Discord.DiscordMessage() { Username = config.settings.WebhookName, Embeds = new List<Discord.DiscordEmbed> { new Discord.DiscordEmbed() { Title = "Изменение имени носителя", Description = $"Имя носителя изменилось на {info.Name}", Color = Color.Brown } } });
                        webhook = null;
                    }
                    else if (ev.Event == "CarrierDockingPermission")
                    {
                        CarrierDockingPermissionInfo info = JsonConvert.DeserializeObject<CarrierDockingPermissionInfo>(Event);
                        DiscordWebhook webhook = new DiscordWebhook();
                        webhook.Url = config.settings.WebhookLink;
                        webhook.Send(new Discord.DiscordMessage() { Username = config.settings.WebhookName, Embeds = new List<Discord.DiscordEmbed> { new Discord.DiscordEmbed() { Title = "Изменение разрешения на стыковку", Description = $"Носитель {config.CarrierName} {config.CarrierIdent} сменил разрешение на стыковку на {info.DockingAccess}\nСтыковка для преступников сейчас {(info.AllowNotorious ? "разрешена" : "запрещена")}", Color = Color.Gray } } });
                        webhook = null;
                    }
                    else if (ev.Event == "Music")
                    {
                        MusicInfo info = JsonConvert.DeserializeObject<MusicInfo>(Event);
                        if (info.MusicTrack == "NoInGameMusic" && config.TrackName == "NoTrack")
                        {
                            DiscordWebhook webhook = new DiscordWebhook();
                            webhook.Url = config.settings.WebhookLink;
                            webhook.Send(new Discord.DiscordMessage() { Username = config.settings.WebhookName, Embeds = new List<Discord.DiscordEmbed> { new Discord.DiscordEmbed() { Title = "Прыжок совершён", Description = $"Носитель {config.CarrierName} {config.CarrierIdent} совершил прыжок в систему {config.LastJumpSystemRequest} из системы {config.CurrentSystemGuess}", Color = Color.LightGreen } } });
                            webhook = null;
                        }
                        config.TrackName = info.MusicTrack;
                    }
                }
                catch
                {

                }
                finally
                {
                    StreamWriter sw = File.CreateText("CarrierTrackSettings.json");
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
        private class MusicInfo : CommonEvent
        {
            public string MusicTrack;
        }
    }
}
