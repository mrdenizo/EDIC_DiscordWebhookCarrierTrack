using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DiscordWebhooksCarrierTrack.Program;

namespace DiscordWebhooksCarrierTrack
{
    public partial class ConfigSettings : UserControl
    {
        public ConfigSettings()
        {
            InitializeComponent();
        }

        private void ConfigSettings_Load(object sender, EventArgs e)
        {
            if (File.Exists("CarrierTrackSettings.json"))
            {
                Config config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(File.ReadAllText("CarrierTrackSettings.json"));
                textBox1.Text = config.settings.WebhookName;
                textBox2.Text = config.settings.WebhookLink;
                checkBox1.Checked = config.settings.Enabled;
            }
            else
            {
                Config config = new Config() { settings = new Config.Settings() { WebhookLink = "Webhook link", WebhookName = "Webhook Name", Enabled = true }, LastJumpSystemRequest = "Unknown" };
                textBox1.Text = config.settings.WebhookName;
                textBox2.Text = config.settings.WebhookLink;
                checkBox1.Checked = config.settings.Enabled;
            }
            ((Form)this.TopLevelControl).FormClosing += (s, ev) =>
            {
                Config config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(File.ReadAllText("CarrierTrackSettings.json"));
                config.settings = new Config.Settings() { Enabled = checkBox1.Checked, WebhookLink = textBox2.Text, WebhookName = textBox1.Text };
                if (File.Exists("CarrierTrackSettings.json"))
                {
                    File.Delete("CarrierTrackSettings.json");
                }
                StreamWriter sw = File.CreateText("CarrierTrackSettings.json");
                sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(config));
                sw.Close();
            };
        }
    }
}
