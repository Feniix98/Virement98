using Life;
using Life.Network;
using Life.UI;
using ModKit.Interfaces;
using Format = ModKit.Helper.TextFormattingHelper;
using AAMenu;
using ModKit.Internal;

namespace Virement98
{
    public class Virement98 : ModKit.ModKit
    {
        public Virement98(IGameAPI gameAPI) : base(gameAPI)
        {
            PluginInformations = new PluginInformations("Virement98", "1.0.0", "! Fenix");
        }

        public override void OnPluginInit()
        {
            base.OnPluginInit();
            Logger.LogSuccess($"{PluginInformations.SourceName} v{PluginInformations.Version}", "initialisé");
            new SChatCommand("/virement", "Permet de faire un virement", "/virement", (player, args) =>
            {
                VirementPanel(player);
            }).Register();
            Menu.AddInteractionTabLine(PluginInformations, "Faire un virement", (ui) =>
            {
                Player player = PanelHelper.ReturnPlayerFromPanel(ui);
                VirementPanel(player);
            });
        }
        public void VirementPanel(Player player)
        {
            UIPanel panel = new UIPanel("Virement - Numéro", UIPanel.PanelType.Input);

            panel.SetInputPlaceholder("Numéro de télephone");

            panel.AddButton("Fermer", ui => player.ClosePanel(panel));
            panel.AddButton("Suite", delegate
            {
                foreach (var players in Nova.server.GetAllInGamePlayers())
                {
                    if (players.character.PhoneNumber == panel.inputText)
                    {
                        SecondVirementPanel(player, players, panel.inputText);
                    }
                    else
                    {
                        player.ClosePanel(panel);
                        player.Notify($"{Format.Color("Erreur", Format.Colors.Error)}", "Ce numéro est invalide ou alors la personne n'est pas en ville.", NotificationManager.Type.Error, 10f);
                    }
                }
            });

            player.ShowPanelUI(panel);
        }

        public void SecondVirementPanel(Player player, Player target, string phoneNumber)
        {
            UIPanel panel = new UIPanel("Virement - Montant", UIPanel.PanelType.Input);

            panel.SetInputPlaceholder("Montant...");

            panel.AddButton("Fermer", ui => player.ClosePanel(panel));
            panel.AddButton("Envoyer", delegate
            {
                if (int.TryParse(panel.inputText, out int montant))
                {
                    if (player.character.Bank >= montant)
                    {
                        player.ClosePanel(panel);
                        player.AddBankMoney(-montant, "Virement");
                        target.AddBankMoney(montant, "Virement");
                        player.Save();
                        target.Save();
                        target.SendText($"{Format.Color($"Le citoyen {player.character.Firstname} vient de vous faire un virement d'un montant de {montant}€", Format.Colors.Info)}");
                        player.Notify("Success", "L'argent a bien étais transferer .", NotificationManager.Type.Success);
                    }
                    else
                    {
                        player.ClosePanel(panel);
                        player.Notify("Erreur", "Vous n'avez pas cette argent !", NotificationManager.Type.Error);
                    }
                }
                else
                {
                    player.ClosePanel(panel);
                    player.Notify("Warning", "Veuillez entrer un montant valide !", NotificationManager.Type.Error);
                }
            });

            player.ShowPanelUI(panel);
        }
    }
}
