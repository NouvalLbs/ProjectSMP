using System.Linq;
using ProjectSMP.Core;
using ProjectSMP.Features.Bank.Paycheck;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;

namespace ProjectSMP.Features.Bank
{
    public static class BankDialogManager
    {
        public static void ShowBankInterface(Player player)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu belum login.");
                return;
            }

            if (BankService.GetAccountCount(player) < 1)
            {
                ShowCreateAccountDialog(player);
                return;
            }

            ShowBankMenu(player, 0);
        }

        private static void ShowCreateAccountDialog(Player player)
        {
            player.ShowList("Bank Teller Menu", "{FFFF00}> {FFFFFF}Buat rekening baru")
                .WithButtons("Select", "Close")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;
                    CreateNewAccountAsync(player);
                });
        }

        private static async void CreateNewAccountAsync(Player player)
        {
            if (await BankService.CreateAccountAsync(player, "Tabungan"))
            {
                var account = player.BankAccounts[0];
                player.SendClientMessage(Color.White,
                    $"{Msg.Bank} Akun bank berhasil dibuat! ({account.AccountName} - No.Rek: {account.AccountNumber})");
            }
            else
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Gagal membuat akun bank!");
            }
        }

        private static void ShowBankMenu(Player player, int accountIndex)
        {
            var account = BankService.GetAccount(player, accountIndex);
            if (account == null) return;

            var title = $"{{FFFFFF}}Bank: {{FFFF00}}{account.AccountName} {{FFFFFF}}| No.Rek: {{FFFF00}}{account.AccountNumber}";
            player.ShowTabListNoHeader(title, 2)
                .WithItems(
                    new[] { "{FFFFFF}Status Rekening:", "{00FF00}Aktif" },
                    new[] { "{FFFFFF}Saldo Rekening:", $"{{00FF00}}{Utilities.GroupDigits(account.Balance)}" },
                    new[] { "{FFFFFF}Transaksi Terakhir:", $"{{FF0000}}{account.LastTransaction}" },
                    new[] { "{FFFF00}> {FFFFFF}Deposit Uang", "" },
                    new[] { "{FFFF00}> {FFFFFF}Withdraw Uang", "" },
                    new[] { "{FFFF00}> {FFFFFF}Transfer Uang", "" },
                    new[] { "{FFFF00}> {FFFFFF}Ambil Paycheck", "" })
                .WithButtons("Select", "Close")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;

                    switch (e.ListItem)
                    {
                        case 3: ShowDepositDialog(player, accountIndex); break;
                        case 4: ShowWithdrawDialog(player, accountIndex); break;
                        case 5: ShowTransferAccountDialog(player, accountIndex); break;
                        case 6:
                            if (!PaycheckService.CanClaim(player)) {
                                player.SendClientMessage(Color.White,
                                    $"{Msg.Bank} Gaji belum bisa diambil! Tunggu {{FF6347}}{PaycheckService.GetTimeLeft(player)}{{FFFFFF}} lagi.");
                                return;
                            }

                            var claimTotal = PaycheckService.GetTotal(player);
                            if (claimTotal <= 0) {
                                player.SendClientMessage(Color.White, $"{Msg.Bank} Tidak ada gaji yang bisa diambil saat ini.");
                                return;
                            }

                            var claimAccount = player.BankAccounts.FirstOrDefault(a => a.IsActive);
                            if (claimAccount == null) {
                                player.SendClientMessage(Color.White, $"{Msg.Bank} Kamu tidak memiliki rekening bank aktif!");
                                return;
                            }

                            if (PaycheckService.ClaimPaycheck(player)) {
                                player.SendClientMessage(Color.White,
                                    $"{Msg.Bank} Berhasil mengambil gaji {{00FF00}}{Utilities.GroupDigits(claimTotal)}{{FFFFFF}} ke rekening {{FFFF00}}{claimAccount.AccountName}{{FFFFFF}}!");
                            }
                            break;
                        default:
                            ShowBankMenu(player, 0);
                            break;
                    }
                });
        }

        private static void ShowDepositDialog(Player player, int accountIndex)
        {
            var account = BankService.GetAccount(player, accountIndex);
            if (account == null) return;

            player.ShowInput("Bank - Deposit Uang",
                $"Saldo saat ini: {{00FF00}}{Utilities.GroupDigits(account.Balance)}\n\n{{FFFFFF}}Masukkan jumlah uang yang ingin Anda deposit:\n{{c8c8c8}}Tip: Kamu dapat menggunakan titik/koma (Cth: 10.50)")
                .WithButtons("Deposit", "Kembali")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowBankMenu(player, accountIndex);
                        return;
                    }

                    if (!double.TryParse(e.InputText.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var inputParsed) || inputParsed <= 0)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Format nominal tidak valid!");
                        ShowDepositDialog(player, accountIndex);
                        return;
                    }

                    var amount = (int)System.Math.Round(inputParsed * 100);
                    if (amount <= 0)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Jumlah deposit harus lebih dari $0!");
                        ShowDepositDialog(player, accountIndex);
                        return;
                    }

                    if (BankService.Deposit(player, account, amount))
                    {
                        player.SendClientMessage(Color.White,
                            $"{Msg.Bank} Berhasil deposit {{00FF00}}{Utilities.GroupDigits(amount)}{{FFFFFF}} ke akun {account.AccountName}. Saldo: {{00FF00}}{Utilities.GroupDigits(account.Balance)}{{FFFFFF}}");
                    }
                    else
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Deposit gagal! Uang kamu tidak cukup.");
                    }
                });
        }

        private static void ShowWithdrawDialog(Player player, int accountIndex)
        {
            var account = BankService.GetAccount(player, accountIndex);
            if (account == null) return;

            player.ShowInput("Bank - Tarik Uang",
                $"Saldo saat ini: {{00FF00}}{Utilities.GroupDigits(account.Balance)}\n\n{{FFFFFF}}Masukkan jumlah uang yang ingin Anda tarik:\n{{c8c8c8}}Tip: Kamu dapat menggunakan titik/koma (Cth: 10.50)")
                .WithButtons("Tarik", "Kembali")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowBankMenu(player, accountIndex);
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
                            $"{Msg.Bank} Berhasil menarik {{00FF00}}{Utilities.GroupDigits(amount)}{{FFFFFF}} dari akun {account.AccountName}. Saldo: {{00FF00}}{Utilities.GroupDigits(account.Balance)}{{FFFFFF}}");
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

            player.ShowInput("Bank - Transfer Uang",
                $"Saldo saat ini: {{00FF00}}{Utilities.GroupDigits(account.Balance)}\n\n{{FFFFFF}}Masukkan nomor rekening tujuan transfer:")
                .WithButtons("Lanjutkan", "Kembali")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowBankMenu(player, accountIndex);
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

            player.ShowInput("Bank - Transfer Uang",
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
                        player.SendClientMessage(Color.White, $"{Msg.Error} Saldo Anda tidak cukup untuk transfer ini!");
                        ShowBankMenu(player, accountIndex);
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

            player.ShowMessage("Bank - Konfirmasi Transfer", message)
                .WithButtons("Ya", "Tidak")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowBankMenu(player, accountIndex);
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