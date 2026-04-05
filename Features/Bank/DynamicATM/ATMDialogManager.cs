using ProjectSMP.Core;
using ProjectSMP.Features.Bank;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;

namespace ProjectSMP.Features.Bank.DynamicATM
{
    public static class ATMDialogManager
    {
        public static void ShowATMInterface(Player player)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu belum login.");
                return;
            }

            if (BankService.GetAccountCount(player) < 1)
            {
                player.SendClientMessage(Color.White, $"{{C6E2FF}}<ATM> {{FFFFFF}}Kamu tidak memiliki rekening bank. Kunjungi bank terdekat untuk membuat rekening.");
                return;
            }

            ShowATMMenu(player, 0);
        }

        private static void ShowATMMenu(Player player, int accountIndex)
        {
            var account = BankService.GetAccount(player, accountIndex);
            if (account == null) return;

            var title = $"{{FFFFFF}}ATM | {{FFFFFF}}No.Rek: {{FFFF00}}{account.AccountNumber}";
            player.ShowTabListNoHeader(title, 2)
                .WithItems(
                    new[] { "{FFFFFF}Status Rekening:", "{00FF00}Aktif" },
                    new[] { "{FFFFFF}Saldo Rekening:", $"{{00FF00}}{Utilities.GroupDigits(account.Balance)}" },
                    new[] { "{FFFFFF}Transaksi Terakhir:", $"{{FF0000}}{account.LastTransaction}" },
                    new[] { "{FFFF00}> {FFFFFF}Withdraw Uang", "" },
                    new[] { "{FFFF00}> {FFFFFF}Transfer Uang", "" })
                .WithButtons("Select", "Close")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;

                    switch (e.ListItem)
                    {
                        case 3: ShowWithdrawDialog(player, accountIndex); break;
                        case 4: ShowTransferAccountDialog(player, accountIndex); break;
                        default: ShowATMMenu(player, accountIndex); break;
                    }
                });
        }

        private static void ShowWithdrawDialog(Player player, int accountIndex)
        {
            var account = BankService.GetAccount(player, accountIndex);
            if (account == null) return;

            player.ShowInput("ATM - Tarik Uang",
                $"Saldo saat ini: {{00FF00}}{Utilities.GroupDigits(account.Balance)}\n\n{{FFFFFF}}Masukkan jumlah uang yang ingin Anda tarik:\n{{c8c8c8}}Tip: Kamu dapat menggunakan titik/koma (Cth: 10.50)")
                .WithButtons("Tarik", "Kembali")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowATMMenu(player, accountIndex);
                        return;
                    }

                    if (!double.TryParse(e.InputText.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var inputParsed) || inputParsed <= 0)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Format nominal tidak valid!");
                        ShowWithdrawDialog(player, accountIndex);
                        return;
                    }

                    var amount = (int)System.Math.Round(inputParsed * 100);
                    if (amount <= 0)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Jumlah penarikan harus lebih dari $0!");
                        ShowWithdrawDialog(player, accountIndex);
                        return;
                    }

                    if (BankService.Withdraw(player, account, amount))
                    {
                        player.SendClientMessage(Color.White,
                            $"{Msg.Bank} Berhasil menarik {{00FF00}}{Utilities.GroupDigits(amount)}{{FFFFFF}} dari ATM. Saldo: {{00FF00}}{Utilities.GroupDigits(account.Balance)}{{FFFFFF}}");
                    }
                    else
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Penarikan gagal! Saldo tidak cukup.");
                    }
                });
        }

        private static void ShowTransferAccountDialog(Player player, int accountIndex)
        {
            var account = BankService.GetAccount(player, accountIndex);
            if (account == null) return;

            player.ShowInput("ATM - Transfer Uang",
                $"Saldo saat ini: {{00FF00}}{Utilities.GroupDigits(account.Balance)}\n\n{{FFFFFF}}Masukkan nomor rekening tujuan transfer:")
                .WithButtons("Lanjutkan", "Kembali")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowATMMenu(player, accountIndex);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(e.InputText) || e.InputText.Length < 5)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Nomor rekening tidak valid!");
                        ShowTransferAccountDialog(player, accountIndex);
                        return;
                    }

                    ShowTransferAmountDialog(player, accountIndex, e.InputText);
                });
        }

        private static void ShowTransferAmountDialog(Player player, int accountIndex, string targetAccount)
        {
            var account = BankService.GetAccount(player, accountIndex);
            if (account == null) return;

            player.ShowInput("ATM - Transfer Uang",
                $"Saldo saat ini: {{00FF00}}{Utilities.GroupDigits(account.Balance)}\n{{FFFFFF}}Rekening tujuan: {{FFFF00}}{targetAccount}\n\n{{FFFFFF}}Masukkan jumlah uang yang ingin Anda transfer:\n{{c8c8c8}}Tip: Kamu dapat menggunakan titik/koma (Cth: 10.50)")
                .WithButtons("Transfer", "Kembali")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowTransferAccountDialog(player, accountIndex);
                        return;
                    }

                    if (!double.TryParse(e.InputText.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var inputParsed) || inputParsed <= 0)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Format nominal tidak valid!");
                        ShowTransferAmountDialog(player, accountIndex, targetAccount);
                        return;
                    }

                    var amount = (int)System.Math.Round(inputParsed * 100);
                    if (amount <= 0)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Jumlah transfer harus lebih dari $0!");
                        ShowTransferAmountDialog(player, accountIndex, targetAccount);
                        return;
                    }

                    if (account.Balance < amount)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Saldo tidak cukup untuk transfer ini!");
                        ShowATMMenu(player, accountIndex);
                        return;
                    }

                    ShowTransferConfirmDialog(player, accountIndex, targetAccount, amount);
                });
        }

        private static void ShowTransferConfirmDialog(Player player, int accountIndex, string targetAccount, int amount)
        {
            var account = BankService.GetAccount(player, accountIndex);
            if (account == null) return;

            var message = $"Anda akan mentransfer:\n\n" +
                          $"{{FFFFFF}}Jumlah: {{00FF00}}{Utilities.GroupDigits(amount)}\n" +
                          $"{{FFFFFF}}Dari Rekening: {{FFFF00}}{account.AccountName} ({account.AccountNumber})\n" +
                          $"{{FFFFFF}}Ke Rekening: {{FFFF00}}{targetAccount}\n\n" +
                          $"{{FF0000}}Apakah Anda yakin ingin melanjutkan transfer ini?";

            player.ShowMessage("ATM - Konfirmasi Transfer", message)
                .WithButtons("Ya", "Tidak")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowATMMenu(player, accountIndex);
                        return;
                    }

                    ProcessTransferAsync(player, accountIndex, targetAccount, amount);
                });
        }

        private static async void ProcessTransferAsync(Player player, int accountIndex, string targetAccount, int amount)
        {
            var account = BankService.GetAccount(player, accountIndex);
            if (account == null) return;

            await BankService.TransferAsync(player, account, targetAccount, amount);
        }
    }
}