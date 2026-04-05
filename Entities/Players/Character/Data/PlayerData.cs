using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Character;
using ProjectSMP.Entities.Players.Delay.Data;
using ProjectSMP.Entities.Players.Inventory.Data;
using ProjectSMP.Features.Bank.Data;
using ProjectSMP.Features.Bank.Paycheck;
using ProjectSMP.Features.ProgressBar.Data;
using System;
using System.Collections.Generic;

namespace ProjectSMP
{
    public partial class Player
    {
        public string CitizenId { get; set; } = "";
        public string Ucp { get; set; } = "";
        public string RegDate { get; set; } = "";
        public string LastLogin { get; set; } = "";
        public bool IsCharLoaded { get; set; }

        public CharInfo CharInfo { get; set; } = new();
        public int VerifiedChar { get; set; }

        public int Level { get; set; } = 1;
        public int LevelPoints { get; set; }
        public int CharMoney { get; set; }
        public int Admin { get; set; }
        public int MaskId { get; set; }
        public int Warn { get; set; }
        public bool Cuffed { get; set; }

        public CharPosition CharSpawnPos { get; set; } = new();
        public CharVitals Vitals { get; set; } = new();
        public CharPlaytime Playtime { get; set; } = new();

        public InventoryData InventoryData { get; set; } = new();
        public CharPhone Phone { get; set; } = new();
        public CharJailInfo JailInfo { get; set; } = new();
        public CharBanInfo BanInfo { get; set; } = new();
        public CharCondition Condition { get; set; } = new();
        public CharSettings Settings { get; set; } = new();
        public List<CharJob> Jobs { get; set; } = new();
        public PaycheckData PaycheckData { get; set; } = new();
        public CharDelays Delays { get; set; } = new();

        public List<PlayerBankAccount> BankAccounts { get; set; } = new();
        public long LastSpawnTick { get; set; }
        public ClientType ClientType { get; set; } = ClientType.PC;
        public string ClientVersion { get; set; } = string.Empty;
        public string ClientCISerial { get; set; } = string.Empty;
        public DateTime LastDoorInteraction { get; set; } = DateTime.MinValue;
        public ProgressBarData ProgressBarData { get; set; } = new();
    }
}