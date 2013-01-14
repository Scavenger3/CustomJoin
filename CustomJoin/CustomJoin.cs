using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Hooks;
using Terraria;
using TShockAPI;

namespace CustomJoin
{
	[APIVersion(1, 12)]
	public class CustomJoin : TerrariaPlugin
	{
		public override string Name { get { return "CustomJoin"; } }
		public override string Author { get { return "Scavenger"; } }
		public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

		public static cjConfig cfg { get; set; }
		int LastJoinIndex { get; set; }

		public CustomJoin(Main game)
			: base(game)
		{
			Order = -10;
			cfg = new cjConfig();
			LastJoinIndex = -1;
		}

		public override void Initialize()
		{
			GameHooks.Initialize += onInitialize;
			NetHooks.GetData += onGetData;
			NetHooks.SendData += onSendData;
			ServerHooks.Leave += onLeave;
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				GameHooks.Initialize -= onInitialize;
				NetHooks.GetData -= onGetData;
				NetHooks.SendData -= onSendData;
				ServerHooks.Leave -= onLeave;
			}
			base.Dispose(disposing);
		}

		void onInitialize()
		{
			Commands.ChatCommands.Add(new Command("customjoin.reload", cjConfig.ReloadConfig, "cjreload"));
			Commands.ChatCommands.Add(new Command("customjoin.setjoin", CMDsetjoin, "setjoin"));
			Commands.ChatCommands.Add(new Command("customjoin.setleave", CMDsetleave, "setleave"));
			cjConfig.LoadConfig();
		}

		void CMDsetjoin(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendWarningMessage("Usage: /setjoin <text / hide / reset>");
				return;
			}

			string message = string.Join(" ", args.Parameters);

			if (!cfg.CustomMessages.ContainsKey(args.Player.Name))
				cfg.CustomMessages.Add(args.Player.Name, new cfgMessage());

			switch (message.ToLower())
			{
				case "hide":
					cfg.CustomMessages[args.Player.Name].HideJoin = true;
					cfg.CustomMessages[args.Player.Name].JoinMessage = string.Empty;
					args.Player.SendMessage("Your join message will not be shown!", Color.MediumSeaGreen);
					break;
				case "reset":
					if (cfg.CustomMessages[args.Player.Name].HideLeave == false && cfg.CustomMessages[args.Player.Name].LeaveMessage == string.Empty)
						cfg.CustomMessages.Remove(args.Player.Name);
					else
					{
						cfg.CustomMessages[args.Player.Name].HideJoin = false;
						cfg.CustomMessages[args.Player.Name].JoinMessage = string.Empty;
					}
					args.Player.SendMessage("Using default TShock join message!", Color.MediumSeaGreen);
					break;
				default:
					cfg.CustomMessages[args.Player.Name].HideJoin = false;
					cfg.CustomMessages[args.Player.Name].JoinMessage = message;
					args.Player.SendMessage("Your join message is now:", Color.MediumSeaGreen);
					args.Player.SendInfoMessage(string.Format(cfg.CustomMessages[args.Player.Name].JoinMessage, args.Player.Name, args.Player.Country ?? string.Empty));
					break;
			}

			cfg.Write();
		}
		void CMDsetleave(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendWarningMessage("Usage: /setleave <message / hide / reset>");
				return;
			}

			string message = string.Join(" ", args.Parameters);

			if (!cfg.CustomMessages.ContainsKey(args.Player.Name))
				cfg.CustomMessages.Add(args.Player.Name, new cfgMessage());

			switch (message.ToLower())
			{
				case "hide":
					cfg.CustomMessages[args.Player.Name].HideLeave = true;
					cfg.CustomMessages[args.Player.Name].LeaveMessage = string.Empty;
					args.Player.SendMessage("Your leave message will not be shown!", Color.MediumSeaGreen);
					break;
				case "reset":
					if (cfg.CustomMessages[args.Player.Name].HideJoin == false && cfg.CustomMessages[args.Player.Name].JoinMessage == string.Empty)
						cfg.CustomMessages.Remove(args.Player.Name);
					else
					{
						cfg.CustomMessages[args.Player.Name].HideLeave = false;
						cfg.CustomMessages[args.Player.Name].LeaveMessage = string.Empty;
					}
					args.Player.SendMessage("Using default TShock leave message!", Color.MediumSeaGreen);
					break;
				default:
					cfg.CustomMessages[args.Player.Name].HideLeave = false;
					cfg.CustomMessages[args.Player.Name].LeaveMessage = message;
					args.Player.SendMessage("Your join message is now:", Color.MediumSeaGreen);
					args.Player.SendInfoMessage(string.Format(cfg.CustomMessages[args.Player.Name].LeaveMessage, args.Player.Name, args.Player.Country ?? string.Empty));
					break;
			}

			cfg.Write();
		}

		void onSendData(SendDataEventArgs e)
		{
			try
			{
				if (e.MsgID == PacketTypes.ChatText && e.Handled == false && LastJoinIndex > -1 && LastJoinIndex < TShock.Players.Length && e.text.EndsWith(" has joined.") && e.number2 == Color.Yellow.R && e.number3 == Color.Yellow.G && e.number4 == Color.Yellow.B)
				{
					var tsplr = TShock.Players[LastJoinIndex];
					LastJoinIndex = -1;

					if (tsplr == null) return;

					if (cfg.CustomMessages.ContainsKey(tsplr.Name))
					{
						if (cfg.CustomMessages[tsplr.Name].HideJoin)
							e.Handled = true;
						else if (cfg.CustomMessages[tsplr.Name].JoinMessage != string.Empty)
							e.text = string.Format(cfg.CustomMessages[tsplr.Name].JoinMessage, tsplr.Name, tsplr.Country ?? string.Empty);
					}
				}
			}
			catch { }
		}

		void onGetData(GetDataEventArgs e)
		{
			try
			{
				if (e.MsgID == PacketTypes.TileGetSection && e.Handled == false)
				{
					LastJoinIndex = e.Msg.whoAmI;
				}
			}
			catch { }
		}

		void onLeave(int ply)
		{
			try
			{
				var tsplr = TShock.Players[ply];
				if (tsplr != null && cfg.CustomMessages.ContainsKey(tsplr.Name) && tsplr.ReceivedInfo)
				{
					if (cfg.CustomMessages[tsplr.Name].HideLeave == false && cfg.CustomMessages[tsplr.Name].LeaveMessage == string.Empty) return;

					tsplr.SilentKickInProgress = true;
					tsplr.State = 1;//Remove in tshock 4.1

					if (cfg.CustomMessages[tsplr.Name].LeaveMessage != string.Empty)
						TShock.Utils.Broadcast(string.Format(cfg.CustomMessages[tsplr.Name].LeaveMessage, tsplr.Name, tsplr.Country ?? string.Empty), Color.Yellow);
				}
			}
			catch { }
		}
	}
}
