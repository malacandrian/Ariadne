using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.Piccolo.Util;
using System.Drawing;
using UMD.HCIL.Piccolo.Event;
using System.Windows.Forms;
using UMD.HCIL.PiccoloX.Nodes;
using System.Runtime.InteropServices;
using zoom.Commands;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;


namespace zoom.Interfaces
{
    public class CommandInterface : AbstractInterface
    {
        #region Fields
        protected List<ICommand> commands = new List<ICommand>();
        protected List<ICommand> _RecentCommands = new List<ICommand>();
        public ICommand[] RecentCommands
        {
            get
            {
                ICommand[] output = _RecentCommands.ToArray();
                return output.Reverse().ToArray();
            }
        }

        protected PNode AuxillaryBox;
        protected PPath AuxillaryBoxBorder;
        protected PText AutoComplete;

        public bool AutoCompleteExists { get { return AutoComplete != null && IndexOfChild(AutoComplete) >= 0; } }

        public PCamera Camera { get; protected set; }

        public ICommand[] Commands
        {
            get
            {
                return commands.ToArray();
            }
        }

        public Selection Selection { get { return ((Window)Camera.Canvas.FindForm()).Selection; } }

        /// <summary>
        /// Whether the system should suggest commands to the user
        /// </summary>
        public bool ShouldSuggest { get { return ShouldAutoComplete; } }

        /// <summary>
        /// Whether the entry field should provide an autocomplete
        /// </summary>
        public bool ShouldAutoComplete
        {
            get
            {
                if (!EntryContainsText) { return true; };
                return !Entry.Text.Contains(" ");
            }
        }

        /// <summary>
        /// Whether the user has entered any text
        /// </summary>
        public bool EntryContainsText { get { return Entry.Text != null && Entry.Text.Length > 0; } }

        public string[] EntryParts
        {
            get
            {
                if (EntryContainsText) { return Entry.Text.Split(' '); }
                else { return new string[0]; }
            }
        }

        public string CommandName
        {
            get
            {
                if (EntryContainsText) { return EntryParts[0]; }
                else { return ""; }
            }
        }

        public ICommand ChosenCommand
        {
            get
            {
                return Commands.FirstOrDefault(x => x.Name == CommandName);
            }
        }

        public string[] Arguments { get { return EntryParts.Skip(1).ToArray(); } }

        /// <summary>
        /// The list of suggestions 
        /// </summary>
        public ICommand[] Suggestions
        {
            get
            {
                if (EntryContainsText)
                {
                    return Commands.Where(x => x.Name.StartsWith(CommandName)).ToArray();
                }
                else { return RecentCommands; }
            }
        }

        protected int CurrentSuggestion;

        protected bool ShouldPreview { get { return Suggestions.Length == 1; } }

        #endregion Fields

        public CommandInterface(PCamera camera)
            : base(new PImage(Properties.Resources.Gear))
        {
            Camera = camera;

            //Add the Auxillary box
            AuxillaryBox = new PNode();
            AddChild(AuxillaryBox);

            //Add the commands
            commands.Add(new ToggleStyleCommand("bold", FontStyle.Bold));
            commands.Add(new ToggleStyleCommand("italic", FontStyle.Italic));
            commands.Add(new ToggleStyleCommand("underline", FontStyle.Underline));
            commands.Add(new ToggleStyleCommand("strike", FontStyle.Strikeout));
            commands.Add(new StyleCommand());
            commands.Add(new SizeCommand());
            commands.Add(new ColorCommand());
            commands.Add(new CalculateCommand(Camera));
            commands.Add(new TranslateCommand("En", Camera));

            _RecentCommands = commands.OrderBy(x => x.Name).Take(4).Reverse().ToList();

            Entry.KeyUp += EntryKeyUp;
        }

        #region Events
        public override void Release(object sender, PInputEventArgs e)
        {
            if (ShouldAutoComplete) { ConfirmAutoComplete(); }
            ICommand command = ChosenCommand;

            UpdateRecentCommands(command);
            command.Execute(Selection, Arguments);
        }

        public override void Activate(object sender, PInputEventArgs e)
        {
            UpdateAuxillaryBox();
        }

        public override void RegisterActivateButtonPress(object sender, PInputEventArgs e) { }

        public override bool Accepts(PInputEventArgs e)
        {
            return e.IsKeyEvent && MatchKeys(Keys.F1, e.KeyData);
        }

        private void EntryKeyUp(object sender, PInputEventArgs e)
        {
            if (ShouldAutoComplete)
            {
                if (e.KeyCode == Keys.Back) { TruncateUntilAmbiguous(); }
                else { EnsureEnteredTextMatchesCommand(); }

                if ((Suggestions.Length == 1 || e.KeyCode == Keys.Tab)) { ConfirmAutoComplete(); }
            }

            UpdateCurrentSuggestion(e);
            UpdateAuxillaryBox();
        }
        #endregion Events

        #region AutoComplete methods
        protected void UpdateAutoComplete()
        {
            if (ShouldAutoComplete)
            {
                if (!AutoCompleteExists) { AddEmptyAutoComplete(); }

                AutoComplete.Text = GetTextForAutoComplete();
                AutoComplete.Bounds = new RectangleF(Entry.Bounds.Right, Entry.Bounds.Top, 0, 0);
            }
            else if (AutoCompleteExists)
            {
                ClearAutoComplete();
            }
        }

        protected string GetTextForAutoComplete()
        {
            string output = Suggestions[CurrentSuggestion].Name;

            if (EntryContainsText) { output = output.Substring(Entry.Text.Length); }

            return output;
        }

        protected void AddEmptyAutoComplete()
        {
            AutoComplete = new PText();
            AddChild(AutoComplete);
            AutoComplete.Font = Entry.Font;
            AutoComplete.TextBrush = new SolidBrush(Color.FromArgb(200, 200, 200));
        }

        protected void ConfirmAutoComplete()
        {
            if (AutoCompleteExists)
            {
                UpdateAutoComplete();
                Entry.Text += AutoComplete.Text + " ";
                ClearAutoComplete();
            }
        }

        protected void ClearAutoComplete()
        {
            if (AutoCompleteExists)
            {
                RemoveChild(AutoComplete);
                AutoComplete = null;
            }
        }

        #endregion AutoComplete methods

        #region Suggestion methods

        protected void UpdateSuggestions()
        {
            if (ShouldSuggest)
            {
                AuxillaryBox.RemoveAllChildren();
                AddSuggestionsToAuxillaryBox(Suggestions);
                UpdateAutoComplete();
            }
        }

        protected void AddSuggestionsToAuxillaryBox(ICommand[] suggestions)
        {
            float curHeight = Background.Height;

            foreach (ICommand suggestion in suggestions)
            {
                PText text = new PText(suggestion.Name);
                text.Bounds = new RectangleF(Bounds.Left, curHeight, 0, 0);
                text.Font = Entry.Font;
                text.Brush = new SolidBrush(Color.Transparent);
                AuxillaryBox.AddChild(text);
                curHeight += text.Height;
            }
        }

        /// <summary>
        /// To prevent the user typing something that isn't a command, remove any text until it matches at least one command
        /// </summary>
        protected void EnsureEnteredTextMatchesCommand() { TruncateCommon(1); }

        /// <summary>
        /// Remove characters from the entered text until it could be multiple commands
        /// </summary>
        protected void TruncateUntilAmbiguous() { TruncateCommon(2); }

        /// <summary>
        /// Truncate the entered until there are at least this many suggestions
        /// </summary>
        /// <param name="numSuggestions">The minimum number of suggestions</param>
        protected void TruncateCommon(int numSuggestions)
        {
            if (EntryContainsText && Suggestions.Length < numSuggestions)
            {
                Entry.Text = Entry.Text.Substring(0, Entry.Text.Length - 1);
                TruncateCommon(numSuggestions);
            }
        }

        protected void UpdateCurrentSuggestion(PInputEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    if (CurrentSuggestion < Suggestions.Length - 1)
                    {
                        CurrentSuggestion += 1;
                    }
                    break;

                case Keys.Up:
                    if (CurrentSuggestion > 0)
                    {
                        CurrentSuggestion -= 1;
                    }
                    break;

                default:
                    CurrentSuggestion = 0;
                    break;
            }

        }

        protected void UpdateRecentCommands(ICommand command)
        {
            //Attempt to remove the executed command from the list. If that fails, remove the first one
            if (!_RecentCommands.Remove(command))
            {
                _RecentCommands.RemoveAt(0);
            }

            //Add the executed command to the list
            _RecentCommands.Add(command);
        }


        #endregion Suggestion methods

        #region Preview methods

        protected void UpdatePreview()
        {
            if (ShouldPreview)
            {
                AuxillaryBox.RemoveAllChildren();
                PText preview = ChosenCommand.Preview(Selection, Arguments);

                preview.ConstrainWidthToTextWidth = false;
                preview.Width = Background.Width;
                preview.X = Background.Bounds.Left;
                preview.Y = Background.Bounds.Bottom;

                AuxillaryBox.AddChild(preview);
            }
        }

        #endregion Preview methods

        #region Auxillary Box methods

        protected void UpdateAuxillaryBox()
        {
            if (ShouldSuggest) { UpdateSuggestions(); }
            else { UpdatePreview(); }
            UpdateAuxillaryBoxBorder();
        }

        protected void UpdateAuxillaryBoxBorder()
        {
            if (AuxillaryBoxBorder != null && AuxillaryBox.IndexOfChild(AuxillaryBoxBorder) > 0)
            {
                AuxillaryBox.RemoveChild(AuxillaryBoxBorder);
            }

            AuxillaryBoxBorder = PPath.CreateRectangle(Background.Bounds.Left, Background.Bounds.Bottom, Background.Bounds.Width, AuxillaryBox.UnionOfChildrenBounds.Height);
            AuxillaryBox.AddChild(AuxillaryBoxBorder);
            AuxillaryBoxBorder.MoveToBack();

            //AuxillaryBox.Bounds = new RectangleF(Background.Bounds.Left, Background.Bounds.Bottom, Background.Bounds.Width, AuxillaryBox.UnionOfChildrenBounds.Height);
        }

        #endregion Auxillary Box methods
    }
}
