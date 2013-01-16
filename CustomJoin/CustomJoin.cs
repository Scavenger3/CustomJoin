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
			if (args.Parameters.Count < 1 || args.Parameters.Count > 2)
			{
				args.Player.SendWarningMessage("Usage: /setjoin [player] \"<text / hide / reset>\"");
				return;
			}

			var Ply = args.Player;
			string message = args.Parameters[0];
			if (args.Parameters.Count == 2)
			{
				var PlySearch = TShock.Utils.FindPlayer(args.Parameters[0]);
				if (PlySearch.Count != 1)
				{
					args.Player.SendWarningMessage(string.Concat(PlySearch.Count < 1 ? "Less" : "More", " than one player matched!"));
					return;
				}
				Ply = PlySearch[0];
				message = args.Parameters[1];
			}

			if (!cfg.CustomMessages.ContainsKey(Ply.Name))
				cfg.CustomMessages.Add(Ply.Name, new cfgMessage());

			switch (message.ToLower())
			{
				case "hide":
					cfg.CustomMessages[Ply.Name].HideJoin = true;
					cfg.CustomMessages[Ply.Name].JoinMessage = string.Empty;
					Ply.SendMessage("Your join message will not be shown!", Color.MediumSeaGreen);
					if (Ply != args.Player)
						args.Player.SendMessage(string.Concat(Ply.Name, "'s join message will not be shown!"), Color.MediumSeaGreen);
					break;
				case "reset":
					if (cfg.CustomMessages[Ply.Name].HideLeave == false && cfg.CustomMessages[Ply.Name].LeaveMessage == string.Empty)
						cfg.CustomMessages.Remove(Ply.Name);
					else
					{
						cfg.CustomMessages[Ply.Name].HideJoin = false;
						cfg.CustomMessages[Ply.Name].JoinMessage = string.Empty;
					}
					Ply.SendMessage("Removed your custom join message!", Color.MediumSeaGreen);
					if (Ply != args.Player)
						args.Player.SendMessage(string.Concat("Removed ", Ply.Name, "'s custom join message!"), Color.MediumSeaGreen);
					break;
				default:
					cfg.CustomMessages[Ply.Name].HideJoin = false;
					cfg.CustomMessages[Ply.Name].JoinMessage = message;
					string Message = string.Format(cfg.CustomMessages[Ply.Name].JoinMessage, Ply.Name, Ply.Country ?? string.Empty);
					Ply.SendMessage("Your join message is now:", Color.MediumSeaGreen);
					Ply.SendInfoMessage(Message);
					if (Ply != args.Player)
					{
						args.Player.SendMessage(string.Concat(Ply.Name, "'s join message is now:"), Color.MediumSeaGreen);
						args.Player.SendInfoMessage(Message);
					}
					break;
			}

			cfg.Write();
		}
		void CMDsetleave(CommandArgs args)
		{
			if (args.Parameters.Count < 1 || args.Parameters.Count > 2)
			{
				args.Player.SendWarningMessage("Usage: /setleave [player] \"<text / hide / reset>\"");
				return;
			}

			var Ply = args.Player;
			string message = args.Parameters[0];
			if (args.Parameters.Count == 2)
			{
				var PlySearch = TShock.Utils.FindPlayer(args.Parameters[0]);
				if (PlySearch.Count != 1)
				{
					args.Player.SendWarningMessage(string.Concat(PlySearch.Count < 1 ? "Less" : "More", " than one player matched!"));
					return;
				}
				Ply = PlySearch[0];
				message = args.Parameters[1];
			}

			if (!cfg.CustomMessages.ContainsKey(Ply.Name))
				cfg.CustomMessages.Add(Ply.Name, new cfgMessage());

			switch (message.ToLower())
			{
				case "hide":
					cfg.CustomMessages[Ply.Name].HideLeave = true;
					cfg.CustomMessages[Ply.Name].LeaveMessage = string.Empty;
					Ply.SendMessage("Your leave message will not be shown!", Color.MediumSeaGreen);
					if (Ply != args.Player)
						args.Player.SendMessage(string.Concat(Ply.Name, "'s leave message will not be shown!"), Color.MediumSeaGreen);
					break;
				case "reset":
					if (cfg.CustomMessages[Ply.Name].HideJoin == false && cfg.CustomMessages[Ply.Name].JoinMessage == string.Empty)
						cfg.CustomMessages.Remove(Ply.Name);
					else
					{
						cfg.CustomMessages[Ply.Name].HideLeave = false;
						cfg.CustomMessages[Ply.Name].LeaveMessage = string.Empty;
					}
					Ply.SendMessage("Removed your custom leave message!", Color.MediumSeaGreen);
					if (Ply != args.Player)
						args.Player.SendMessage(string.Concat("Removed ", Ply.Name, "'s custom leave message!"), Color.MediumSeaGreen);
					break;
				default:
					cfg.CustomMessages[Ply.Name].HideLeave = false;
					cfg.CustomMessages[Ply.Name].LeaveMessage = message;
					string Message = string.Format(cfg.CustomMessages[Ply.Name].LeaveMessage, Ply.Name, Ply.Country ?? string.Empty);
					Ply.SendMessage("Your leave message is now:", Color.MediumSeaGreen);
					Ply.SendInfoMessage(Message);
					if (Ply != args.Player)
					{
						args.Player.SendMessage(string.Concat(Ply.Name, "'s leave message is now:"), Color.MediumSeaGreen);
						args.Player.SendInfoMessage(Message);
					}
					break;
			}

			cfg.Write();
		}
		

		void onSendData(SendDataEventArgs e)
		{
			try
			{
				if (e.MsgID == PacketTypes.ChatText && e.Handled == false && LastJoinIndex > -1)
				{
					if (LastJoinIndex < TShock.Players.Length && e.text.EndsWith(" has joined.") && e.number2 == Color.Yellow.R && e.number3 == Color.Yellow.G && e.number4 == Color.Yellow.B)
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
					else
						LastJoinIndex = -1;
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
