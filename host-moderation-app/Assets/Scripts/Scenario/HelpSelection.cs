using Host;
using Host.Network;
using Michsky.UI.ModernUIPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpSelection : MonoBehaviour
{
    private HelpRPC _helpRPC;

    public CustomDropdown EnigmeDropdown;
    public CustomDropdown HelpsDropdown;

    public class HelpCommand
    {
        public string Text;
        public string CommandName;

        public string TextParameter;
        public int ActionIndex;
    }

    private List<string> Enigmes;
    private List<List<HelpCommand>> Helps;

    // Start is called before the first frame update
    void Start()
    {
        if(GlobalElements.Instance != null)
        {
            _helpRPC = GlobalElements.Instance.HelpRPC;
        }

        Enigmes = new List<string>();
        Enigmes.Add("Dossier patiente");
        Enigmes.Add("Cadenas directionnel");
        Enigmes.Add("Cryptex");
        Enigmes.Add("Si�ges");

        Helps = new List<List<HelpCommand>>();

        var folder = new List<HelpCommand>();
        folder.Add(new HelpCommand() { CommandName = "SendImage", Text = "Image du boarding pass", ActionIndex = 51});
        folder.Add(new HelpCommand() { CommandName = "SendText", Text = "Attention aux couleurs", TextParameter = "Faites attention aux couleurs sur le boarding pass"});
        folder.Add(new HelpCommand() { CommandName = "SendEvent", Text = "Fl�ches lumi�res", ActionIndex = 52});
        folder.Add(new HelpCommand() { CommandName = "SendImage", Text = "D�codage", ActionIndex = 53});
        folder.Add(new HelpCommand() { CommandName = "SendText", Text = "Remettez les si�ges dans l'ordre", TextParameter = "Remettez dans l'ordre les si�ges"});

        Helps.Add(folder);

        var directionnal = new List<HelpCommand>();
        directionnal.Add(new HelpCommand() { CommandName = "SendImage", Text = "Image boussole", ActionIndex = 54 });
        directionnal.Add(new HelpCommand() { CommandName = "SendText", Text = "M�t�o qui oriente", TextParameter = "C'est la m�t�o qui nous oriente" });
        directionnal.Add(new HelpCommand() { CommandName = "SendEvent", Text = "Fl�che �cran", ActionIndex = 55 });
        directionnal.Add(new HelpCommand() { CommandName = "SendText", Text = "Exemple m�t�o", TextParameter = "Les pluies dans le NORD -> Direction = haut" });
        directionnal.Add(new HelpCommand() { CommandName = "SendText", Text = "R�initialiser le cadenas", TextParameter = "Appuyer deux fois sur l'anse pour r�initialiser le cadenas" });

        Helps.Add(directionnal);


        var cryptex = new List<HelpCommand>();
        cryptex.Add(new HelpCommand() { CommandName = "SendText", Text = "Regarder message crypt�", TextParameter = "Un message crypt� est cach� avec la pince kocher" });
        cryptex.Add(new HelpCommand() { CommandName = "SendText", Text = "Message un mot", TextParameter = "Le message donne un seul mot" });
        cryptex.Add(new HelpCommand() { CommandName = "SendText", Text = "Mot de 5 lettres", TextParameter = "On cherche un mot de 5 lettres" });
        cryptex.Add(new HelpCommand() { CommandName = "SendText", Text = "Scouts dos des si�ges", TextParameter = "Des scouts ont griffon�s sur le dos des si�ges" });
        cryptex.Add(new HelpCommand() { CommandName = "SendEvent", Text = "Fl�ches si�ges", ActionIndex = 56 });
        cryptex.Add(new HelpCommand() { CommandName = "SendImage", Text = "Exemple d�codage", ActionIndex = 57 });

        Helps.Add(cryptex);

        var seats = new List<HelpCommand>();
        seats.Add(new HelpCommand() { CommandName = "SendEvent", Text = "Fl�che panneau", ActionIndex = 58 });
        seats.Add(new HelpCommand() { CommandName = "SendText", Text = "Alimenter le panneau", TextParameter = "Le panneau doit �tre aliment�" });
        seats.Add(new HelpCommand() { CommandName = "SendEvent", Text = "Fl�che batterie", ActionIndex = 59 });
        seats.Add(new HelpCommand() { CommandName = "SendImage", Text = "Image num�ros si�ges", ActionIndex = 60 });
        seats.Add(new HelpCommand() { CommandName = "SendText", Text = "Couleurs des pieds", TextParameter = "Avez-vous remarqu� la couleur des pieds des si�ges ?" });
        seats.Add(new HelpCommand() { CommandName = "SendText", Text = "Vert=ON, Rouge=OFF", TextParameter = "Vert=ON, Rouge=OFF" });

        Helps.Add(seats);


        EnigmeDropdown.dropdownItems.Clear();

        foreach(var item in Enigmes)
        {
            EnigmeDropdown.SetItemTitle(item);
            EnigmeDropdown.CreateNewItem();
        }

        EnigmeDropdown.ChangeDropdownInfo(0);

        // Setup with folder by default

        HelpsDropdown.dropdownItems.Clear();

        foreach (var item in folder)
        {
            HelpsDropdown.SetItemTitle(item.Text);
            HelpsDropdown.CreateNewItem();
        }

        HelpsDropdown.ChangeDropdownInfo(0);
    }

    public void ChangeSelectedEnigme(int index)
    {
        if (index >= Helps.Count)
            return;

        HelpsDropdown.index = 0;
        HelpsDropdown.selectedItemIndex = 0;
        HelpsDropdown.dropdownItems.Clear();

        foreach (var item in Helps[index])
        {
            HelpsDropdown.SetItemTitle(item.Text);
            HelpsDropdown.CreateNewItem();
        }

        HelpsDropdown.ChangeDropdownInfo(0);
    }

    public void ChangeSelectedHelp(int index)
    {

    }

    public void SendButtonSelected()
    {
        InvokeCurrentHelpMessage();
    }

    public void InvokeCurrentHelpMessage()
    {

        int index = EnigmeDropdown.selectedItemIndex;
        int helpIndex = HelpsDropdown.selectedItemIndex;

        HelpCommand command = Helps[index][helpIndex];

        Debug.Log($"Invoking an help message: {command.CommandName}, {command.Text}");

        _currentCommand = command;

        Invoke(command.CommandName, 0f);
    }

    private HelpCommand _currentCommand;

    public void SendText()
    {
        MessageEvent m = new MessageEvent(scenario: 0, type: "Alert", content: _currentCommand.TextParameter, recipient: "All");  ;
        _helpRPC.SendEvent(m);
    }

    public void SendImage()
    {
        HelpEvent m = new HelpEvent(scenario: 0, action_number: _currentCommand.ActionIndex.ToString(), recipient: "All", name: _currentCommand.Text);
        _helpRPC.SendEvent(m);
    }

    public void SendEvent()
    {
        HelpEvent m = new HelpEvent(scenario: 0, action_number: _currentCommand.ActionIndex.ToString(), recipient: "All", name: _currentCommand.Text);
        _helpRPC.SendEvent(m);
    }

    public void GenerateCryptedMessage()
    {
        
    }
}
