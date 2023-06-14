using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Program
{
	[ApiVersion(2, 1)]
	public class CustomMedium : TerrariaPlugin
	{
		public static readonly string path = Path.Combine(TShock.SavePath, "CustomMediumItems.json");
		public override string Author => "Ashforest";
		public override string Description => 
			"This plugin is copied from Miffyli where it is possible to select " +
			"what is dropped from the player as if he is in mediumcore";
		public override string Name => "CustomMedium";
		public override Version Version => new(1,4,1);

		private static ItemDropConfig config = new();

		public CustomMedium(Main game) : base(game)
		{
			
		}

		public override void Initialize()
		{
			GeneralHooks.ReloadEvent += OnReload;
			if (File.Exists(path))
			{
				config = ItemDropConfig.Read();
			}
			else
				config.Write();
			ServerApi.Hooks.GameInitialize.Register(this, OnStart);
			ServerApi.Hooks.NetGetData.Register(this, OnGetData);
			ServerApi.Hooks.WorldSave.Register(this, OnWorldSave);
		}

		private void OnGetData(GetDataEventArgs args)
		{
			if (args.MsgID == PacketTypes.PlayerDeathV2)
			{
				var player = TShock.Players[args.Msg.whoAmI];
				if (player.Difficulty == PlayerDifficultyID.SoftCore)
					OnPlayerDeath(player);
			}
		}

		private void OnPlayerDeath(TSPlayer player)
		{
			int size = player.TPlayer.inventory.Length;

			for (int i = 0; i < size; ++i)
			{
				Item cur = player.TPlayer.inventory[i];
				if (cur.netID == ItemID.None || 
					!config.Items.ContainsKey(cur.netID)) continue;

				int ItemIndex = Item.NewItem(
					null, (int)player.X, (int)player.Y,
					player.TPlayer.width, player.TPlayer.height,
					cur.netID, cur.stack, false, cur.prefix,
					false, false);
				
				NetMessage.SendData((int)PacketTypes.ItemDrop, player.Index, -1, Terraria.Localization.NetworkText.FromFormattable(""), ItemIndex);
				player.TPlayer.inventory[i] = new();
				NetMessage.SendData((int)PacketTypes.PlayerSlot, -1,			-1, Terraria.Localization.NetworkText.FromLiteral(player.TPlayer.inventory[i].Name), player.Index, i, player.TPlayer.inventory[i].prefix);
				NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index,	-1, Terraria.Localization.NetworkText.FromLiteral(player.TPlayer.inventory[i].Name), player.Index, i, player.TPlayer.inventory[i].prefix);
			}
		}

		private void OnStart(EventArgs args)
		{
			Commands.ChatCommands.Add(new Command("mediumcoreplug.customize", Customize, "mediumcustom", "medcustom", "medcust", "mc"));
		}

		private void Customize(CommandArgs args)
		{
			
			if (args.Parameters.Count == 1)
			{
				if (args.Parameters[0] == "help")
				{
					if (args.Player != null)
						args.Player.SendMessage("Choose any one of these options\n" +
							"add: To add an item into the drop list\n" +
							"del: To remove an item from the drop list\n" +
							"check: To get a full list of the items in the drop list", Color.Green);
					else
						Console.WriteLine("\nChoose any one of these options\n" +
							"add: To add an item into the drop list\n" +
							"del: To remove an item from the drop list\n" +
							"check: To get a full list of the items in the drop list");
					return;
				}
				else if (args.Parameters[0] == "check")
				{
					if (args.Player != null)
					{
						args.Player.SendMessage("[Item ID] : [Item Name]", Color.Yellow);
						foreach (var item in config.Items)
						{
							args.Player.SendMessage($"{item.Key} : {item.Value}", Color.Yellow);
						}
					}
					else
					{
						Console.WriteLine("\n[Item ID] : [Item Name]");
						foreach (var item in config.Items)
						{
							Console.WriteLine($"{item.Key} : {item.Value}");
						}
					}
					return;
				}
				else
				{
					RuntimeLogging(args.Player, Color.Red);
					return;
				}
			}
			else if (args.Parameters.Count == 2)
			{

				if (args.Parameters[0] == "add")
				{
					if (int.TryParse(args.Parameters[1], out int ItemId))
					{
						Item item = TShock.Utils.GetItemById(ItemId);
						if (item != null) 
						{
							if (config.Items.ContainsKey(item.netID))
							{
								RuntimeLogging(args.Player, Color.Red, $"The item {item.Name} of {item.netID} is already added");
								return;
							}
							config.Items.Add(item.netID, item.Name);
							RuntimeLogging(args.Player, Color.Teal,$"Added {item.netID}, which is {item.Name}");
							return;
						}
					}
					RuntimeLogging(args.Player, Color.Red,"Invalid Item ID");

				}
				else if (args.Parameters[0] == "del")
				{
					if (int.TryParse(args.Parameters[1], out int ItemID))
					{
						Item item = TShock.Utils.GetItemById(ItemID);
						if (item != null)
						{
							if (config.Items.Remove(item.netID))
								RuntimeLogging(args.Player, Color.Teal, $"Removed {item.netID}, which is {item.Name}");
							else
								RuntimeLogging(args.Player, Color.Red,"Item does not exist");
							return;
						}
					}
					RuntimeLogging(args.Player, Color.Red,"Invalid Item ID");
				}
				else
				{
					RuntimeLogging(args.Player, Color.Red);
					return;
				}
			}            
			else
				RuntimeLogging(args.Player, Color.Red);

		}

		private void RuntimeLogging(TSPlayer player, Color color, string msg = "Invalid option for the custom medium command, use the option help for more info\n")
		{
			if (player != null)
				player.SendMessage(msg, color);
			else
				Console.WriteLine("\n" + msg);
		}

		private void OnWorldSave(WorldSaveEventArgs args)
		{
			config.Write();
		}

		private void OnReload(ReloadEventArgs e)
		{
			if (File.Exists(path))
				config = ItemDropConfig.Read();
			else
				config.Write();
			Console.WriteLine("CustomMedium plugin has been reloaded");
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.GameInitialize.Deregister(this, OnStart);
				ServerApi.Hooks.WorldSave.Deregister(this, OnWorldSave);
				ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
			}
			base.Dispose(disposing);
		}
	}
}