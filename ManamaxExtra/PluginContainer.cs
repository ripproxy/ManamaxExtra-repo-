using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using static TShockAPI.GetDataHandlers;

namespace ManamaxExtra
{
    [ApiVersion(2, 1)]
    public class ManamaxExtra : TerrariaPlugin
    {
        public override string Author => "Anonim, Kaisar Hati Xien menambahkan penyesuaian";
        public override string Description => "Tingkatkan batas mana";
        public override string Name => "ManamaxExtra";
        public override Version Version => new Version(1, 0, 0, 6);
        public static Configuration Config;
        private bool[] controlUseItemOld;
        private int[] itemUseTime;

        public ManamaxExtra(Main game) : base(game)
        {
            LoadConfig();
            this.controlUseItemOld = new bool[255];
            this.itemUseTime = new int[255];
        }

        private static void LoadConfig()
        {
            Config = Configuration.Read(Configuration.FilePath);
            Config.Write(Configuration.FilePath);
        }

        private static void ReloadConfig(ReloadEventArgs args)
        {
            LoadConfig();
            args.Player?.SendSuccessMessage("[{0}] Konfigurasi pemuatan ulang selesai", typeof(ManamaxExtra).Name);
        }

        public override void Initialize()
        {
            GeneralHooks.ReloadEvent += ReloadConfig;
            GetDataHandlers.PlayerUpdate += OnPlayerUpdate;
            PlayerHooks.PlayerPostLogin += OnPlayerPostLogin;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeneralHooks.ReloadEvent -= ReloadConfig;
                PlayerHooks.PlayerPostLogin -= OnPlayerPostLogin;
                GetDataHandlers.PlayerUpdate -= OnPlayerUpdate;
            }
            base.Dispose(disposing);
        }

        private void OnPlayerPostLogin(PlayerPostLoginEventArgs args)
        {
            foreach (TSPlayer tsplayer in TShock.Players)
            {
                if (tsplayer != null)
                {
                    // Periksa batas mana dan tetapkan nilai mana
                    CheckAndSetPlayerMana(tsplayer);
                }
            }
        }

        private static void CheckAndSetPlayerMana(TSPlayer tsplayer)
        {
            int index = tsplayer.Index;
            Player tplayer = tsplayer.TPlayer;

            // Jika batas atas masa pakai lebih besar dari nilai maksimum yang dikonfigurasi
            if (tplayer.statManaMax > Config.ManaCrystalMaxMana)
            {
                // Atur kesehatan ke maksimum yang dikonfigurasi
                tplayer.statManaMax = Config.ManaCrystalMaxMana;
                tsplayer.SendData(PacketTypes.PlayerMana, "", index);
            }
        }

        private void OnPlayerUpdate(object sender, PlayerUpdateEventArgs args)
        {
            TSPlayer tsplayer = TShock.Players[args.PlayerId];
            if (tsplayer != null)
            {
                int index = tsplayer.Index;
                Player tplayer = tsplayer.TPlayer;
                Item heldItem = tplayer.HeldItem;

                if (!this.controlUseItemOld[index] && tsplayer.TPlayer.controlUseItem && this.itemUseTime[index] <= 0)
                {
                    int useTime = heldItem.useTime; // Dapatkan waktu penggunaan item
                    int type = heldItem.type; // Dapatkan jenis barang

                    if (type != 109) // Jika barang tersebut bukan barang dengan ID 109
                    {
                        if (tplayer.statManaMax <= Config.ManaCrystalMaxMana) // Jika kesehatan maksimum pemain kurang dari atau sama dengan kesehatan kristal maksimum
                        {
                            if (tsplayer.TPlayer.statManaMax < Config.ManaCrystalMaxMana) // Jika batas hidup pemain saat ini kurang dari nilai kesehatan kristal maksimum yang dikonfigurasi
                            {
                                tsplayer.TPlayer.inventory[tplayer.selectedItem].stack--; // Mengurangi jumlah tumpukan item yang dipilih di ransel pemain
                                tsplayer.SendData(PacketTypes.PlayerSlot, "", index, (float)tplayer.selectedItem); // Memperbarui slot item yang dipilih klien
                                tplayer.statManaMax += 20; // Tingkatkan batas hidup pemain
                                tsplayer.SendData(PacketTypes.PlayerMana, "", index); // Perbarui tampilan kesehatan klien
                            }
                            else if (tsplayer.TPlayer.statManaMax > Config.ManaCrystalMaxMana) // Jika batas hidup pemain saat ini lebih besar dari nilai kesehatan maksimum yang dikonfigurasi
                            {
                                tplayer.statManaMax = Config.ManaCrystalMaxMana; // Tetapkan batas hidup maksimum pemain ke nilai kesehatan buah hidup maksimum yang dikonfigurasi
                                tsplayer.SendData(PacketTypes.PlayerMana, "", index); // Perbarui tampilan kesehatan klien
                            }
                        }
                    }
                }
                this.controlUseItemOld[index] = tsplayer.TPlayer.controlUseItem;
            }
        }
    }
}
