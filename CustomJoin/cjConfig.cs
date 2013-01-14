using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using TShockAPI;

namespace CustomJoin
{
	public class cjConfig
	{
		public Dictionary<string, cfgMessage> CustomMessages;

		public static string cfgPath { get { return Path.Combine(TShock.SavePath, "PluginConfigs", "CustomJoinConfg.json"); } }
		public static cjConfig Read()
		{
			if (!File.Exists(cfgPath))
				return new cjConfig();
			using (var fs = new FileStream(cfgPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return Read(fs);
			}
		}
		public static cjConfig Read(Stream stream)
		{
			using (var sr = new StreamReader(stream))
			{
				var cf = JsonConvert.DeserializeObject<cjConfig>(sr.ReadToEnd());
				if (ConfigRead != null)
					ConfigRead(cf);
				return cf;
			}
		}
		public void Write()
		{
			using (var fs = new FileStream(cfgPath, FileMode.Create, FileAccess.Write, FileShare.Write))
			{
				Write(fs);
			}
		}
		public void Write(Stream stream)
		{
			var str = JsonConvert.SerializeObject(this, Formatting.Indented);
			using (var sw = new StreamWriter(stream))
			{
				sw.Write(str);
			}
		}

		public static Action<cjConfig> ConfigRead;

		/*Load, Reload & Create Config*/
		public static void ReloadConfig(CommandArgs args)
		{
			if (LoadConfig())
				args.Player.SendMessage("CustomJoin config reloaded successfully!", Color.MediumSeaGreen);
			else
				args.Player.SendMessage("An exception occoured while reloading CustomJoin, Check the logs for more details.", Color.OrangeRed);
		}
		public static bool LoadConfig()
		{
			try
			{
				if (!File.Exists(cfgPath))
					CreateConfig();

				CustomJoin.cfg = cjConfig.Read();

				if (CustomJoin.cfg.CustomMessages == null)
					CustomJoin.cfg.CustomMessages = new Dictionary<string, cfgMessage>()
					{
						{ "SomePlayer", new cfgMessage() },
					};

				CustomJoin.cfg.Write();
				return true;
			}
			catch (Exception ex)
			{
				Log.ConsoleError("An exception occoured while loading CustomJoin config file, Check the logs for more details.");
				Log.Error(ex.ToString());
				return false;
			}
		}
		public static void CreateConfig()
		{
			File.WriteAllText(cfgPath,
				"{" + Environment.NewLine +
				"  \"CustomMessages\": {" + Environment.NewLine +
				"    \"DarkOS\": {" + Environment.NewLine +
				"      \"HideJoin\": false," + Environment.NewLine +
				"      \"JoinMessage\": \"\"," + Environment.NewLine +
				"      \"HideLeave\": false," + Environment.NewLine +
				"      \"LeaveMessage\": \"DarkOS in the house... holla!\"" + Environment.NewLine +
				"    }" + Environment.NewLine +
				"  }" + Environment.NewLine +
				"}");
		}
	}

	public class cfgMessage
	{
		public bool HideJoin { get; set; }
		public string JoinMessage { get; set; }
		public bool HideLeave { get; set; }
		public string LeaveMessage { get; set; }
		public cfgMessage()
		{
			this.HideJoin = false;
			this.JoinMessage = string.Empty;
			this.HideLeave = false;
			this.LeaveMessage = string.Empty;
		}
	}
}
