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
using zoom.Command;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;


namespace zoom.Interfaces
{
    /// <summary>
    /// The command interface allows the user to execute _Commands that manipulate their data
    /// </summary>
    public class CommandInterface : AbstractInterface
    {
        #region Fields
        /// <summary>
        /// The list of commands that the interface can fire
        /// </summary>
        protected List<ICommand> _Commands = new List<ICommand>();

        /// <summary>
        /// The list of commands that the interface can fire
        /// </summary>
        public ICommand[] Commands { get { return _Commands.ToArray(); } }

        /// <summary>
        /// The most recently fired commands
        /// </summary>
        protected List<ICommand> _RecentCommands = new List<ICommand>();

        /// <summary>
        /// The most recently fired commands
        /// </summary>
        public ICommand[] RecentCommands
        {
            get
            {
                ICommand[] output = _RecentCommands.ToArray();
                return output.Reverse().ToArray();
            }
        }

        /// <summary>
        /// The box that contains command suggestions and previews
        /// </summary>
        protected PNode AuxillaryBox;

        /// <summary>
        /// The border to the box that contains command suggestions and previews
        /// </summary>
        protected PPath AuxillaryBoxBorder;

        /// <summary>
        /// The displayed text suggesting what the user might be typing
        /// </summary>
        protected PText AutoComplete;

        /// <summary>
        /// Whether there is currently any text being suggested by AutoComplete
        /// </summary>
        public bool AutoCompleteExists { get { return AutoComplete != null && IndexOfChild(AutoComplete) >= 0; } }

        /// <summary>
        /// The Camera the interface is attached to
        /// </summary>
        public PCamera Camera { get; protected set; }

        /// <summary>
        /// The currently selecttion
        /// </summary>
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
        /// Whether the interface should display the preview
        /// </summary>
        protected bool ShouldPreview { get { return Suggestions.Length == 1; } }

        /// <summary>
        /// Whether the user has entered any text
        /// </summary>
        public bool EntryContainsText { get { return Entry.Text != null && Entry.Text.Length > 0; } }

        /// <summary>
        /// What the user has entered into the interface, split on spaces
        /// </summary>
        public string[] EntryParts
        {
            get
            {
                if (EntryContainsText) { return Entry.Text.Split(' '); }
                else { return new string[0]; }
            }
        }

        /// <summary>
        /// The part of the entered text that is the Name of the command
        /// </summary>
        public string CommandName
        {
            get
            {
                if (EntryContainsText) { return EntryParts[0]; }
                else { return ""; }
            }
        }

        /// <summary>
        /// The command that the user has selected
        /// </summary>
        public ICommand ChosenCommand { get { return Commands.FirstOrDefault(x => x.Name == CommandName); } }

        /// <summary>
        /// The arguments to be passed to the command
        /// </summary>
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

        /// <summary>
        /// The index of the suggestion the user is currently considering
        /// </summary>
        protected int CurrentSuggestion;
        #endregion Fields

        /// <summary>
        /// Create a new CommandInterface attached to a specific camera
        /// </summary>
        /// <param Name="camera"></param>
        public CommandInterface(PCamera camera)
            : base(new PImage(Properties.Resources.Gear))
        {
            Camera = camera;

            //Add the Auxillary box
            AuxillaryBox = new PNode();
            AddChild(AuxillaryBox);

            //Add the _Commands
            _Commands.Add(new ToggleStyleCommand("bold", FontStyle.Bold));
            _Commands.Add(new ToggleStyleCommand("italic", FontStyle.Italic));
            _Commands.Add(new ToggleStyleCommand("underline", FontStyle.Underline));
            _Commands.Add(new ToggleStyleCommand("strike", FontStyle.Strikeout));
            _Commands.Add(new StyleCommand());
            _Commands.Add(new SizeCommand());
            _Commands.Add(new ColorCommand());
            _Commands.Add(new CalculateCommand(Camera));
            _Commands.Add(new TranslateCommand("En", Camera));

            _RecentCommands = _Commands.OrderBy(x => x.Name).Take(4).Reverse().ToList();

            Entry.KeyUp += EntryKeyUp;
        }

        #region Events
        /// <summary>
        /// Execute the command the user has chosen
        /// </summary>
        public override void Execute(object sender, PInputEventArgs e)
        {
            //If the user hasn't typed the entire command, fill out the rest for them
            if (ShouldAutoComplete) { ConfirmAutoComplete(); }

            //Find the appropriate command, push it to the stack of recent commands, and execute it
            ICommand command = ChosenCommand;
            UpdateRecentCommands(command);
            command.Execute(Selection, Arguments);
        }

        /// <summary>
        /// When activated, display the most recent four commands
        /// </summary>
        public override void Activate(object sender, PInputEventArgs e) { UpdateAuxillaryBox(); }

        /// <summary>
        /// Not used in this instance
        /// </summary>
        public override void RegisterActivateButtonPress(object sender, PInputEventArgs e) { }

        /// <summary>
        /// Accept only key events involving F1
        /// </summary>
        /// <param Name="e">The details of the event</param>
        /// <returns>Whether it was a key event that involved F1</returns>
        public override bool Accepts(PInputEventArgs e) { return e.IsKeyEvent && MatchKeys(Keys.F1, e.KeyData); }

        /// <summary>
        /// Update the autocomplte, suggestions, and preview every time a key is pressed
        /// </summary>
        private void EntryKeyUp(object sender, PInputEventArgs e)
        {
            if (ShouldAutoComplete)
            {
                //Because the system autofills the rest of the command Name when
                //it becomes unambiguous (e.g. tr -> translate) to save the user
                //unnecesery keypresses, pressing backspace should have a similar effect
                //Therefore, any backspaces should delete until there are at least two
                //different commands it could be
                if (e.KeyCode == Keys.Back) { TruncateUntilAmbiguous(); }

                //Prevent the user from entering text that isn't a command
                else { EnsureEnteredTextMatchesCommand(); }

                //If the Name of the command the user is trying to type is unambiuous
                //Or they have pressed tab, indicating they want to use the suggested command Name
                //Autofill the rest of the command Name for them
                if ((Suggestions.Length == 1 || e.KeyCode == Keys.Tab)) { ConfirmAutoComplete(); }
            }

            //Adjust which of the suggestions the user has selected
            UpdateCurrentSuggestion(e);

            //Update the suggestions or preview, as appropriate
            UpdateAuxillaryBox();
        }
        #endregion Events

        #region AutoComplete methods
        /// <summary>
        /// Change the currently suggested command Name
        /// </summary>
        protected void UpdateAutoComplete()
        {
            //Do not attempt to autocomplete if the system is in the wrong state
            if (ShouldAutoComplete)
            {
                //Create an autocomplete field if required
                if (!AutoCompleteExists) { AddEmptyAutoComplete(); }

                //Fill it with the appropriate text
                AutoComplete.Text = GetTextForAutoComplete();

                //Position the autocomplete so it immedietely follows the text the user entered
                AutoComplete.Bounds = new RectangleF(Entry.Bounds.Right, Entry.Bounds.Top, 0, 0);
            }

            //If there shouldn't be an autocomplete, but is, remove it
            else if (AutoCompleteExists) { ClearAutoComplete(); }
        }

        /// <summary>
        /// Get to fill the autocomplete field with
        /// </summary>
        /// <returns>The rest of the command Name the user is trying to enter</returns>
        protected string GetTextForAutoComplete()
        {
            //Get the full Name of the suggested command
            string output = Suggestions[CurrentSuggestion].Name;

            //If the user has already entered some characters, only display the ones they haven't entered
            if (EntryContainsText) { output = output.Substring(Entry.Text.Length); }

            return output;
        }

        /// <summary>
        /// Create a new, blank autocomplete field
        /// </summary>
        protected void AddEmptyAutoComplete()
        {
            AutoComplete = new PText();
            AddChild(AutoComplete);
            AutoComplete.Font = Entry.Font;
            AutoComplete.TextBrush = new SolidBrush(Color.FromArgb(200, 200, 200));
        }

        /// <summary>
        /// Accept the suggested command, and fill the entry box with the required characters
        /// </summary>
        protected void ConfirmAutoComplete()
        {
            //This only makes sense to perform if there is a current suggestion
            if (AutoCompleteExists)
            {
                //Ensure the current suggestion is up to date
                UpdateAutoComplete();
                //Finish the suggested command Name
                Entry.Text += AutoComplete.Text + " ";
                //Remove the autocomplete field
                ClearAutoComplete();
            }
        }

        /// <summary>
        /// Remove the autocomplete field
        /// </summary>
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
        /// <summary>
        /// Update the list of suggestions
        /// </summary>
        protected void UpdateSuggestions()
        {
            //This should only happen when the system is in the right state
            if (ShouldSuggest)
            {
                //Remove any existing suggestion or preview
                AuxillaryBox.RemoveAllChildren();
                //Add all the suggestions to the box
                AddSuggestionsToAuxillaryBox(Suggestions);
                //Ensure the autocomplete is up to date
                UpdateAutoComplete();
            }
        }

        /// <summary>
        /// Fill the auxillary box with suggestions
        /// </summary>
        /// <param Name="suggestions">The suggestions to add to the box</param>
        protected void AddSuggestionsToAuxillaryBox(ICommand[] suggestions)
        {
            //For each suggestion, create a PText with the Name of the command in
            //Each beneath the other
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
        /// Remove characters from the entered text until it could be multiple _Commands
        /// </summary>
        protected void TruncateUntilAmbiguous() { TruncateCommon(2); }

        /// <summary>
        /// Truncate the entered until there are at least this many suggestions
        /// </summary>
        /// <param Name="numSuggestions">The minimum number of suggestions</param>
        protected void TruncateCommon(int numSuggestions)
        {
            //If there are not enough suggestions, remove a character from the entered text, and recurse
            if (EntryContainsText && Suggestions.Length < numSuggestions)
            {
                Entry.Text = Entry.Text.Substring(0, Entry.Text.Length - 1);
                TruncateCommon(numSuggestions);
            }
        }

        /// <summary>
        /// Changes which suggestion is currently being selected
        /// Based on whether the userr pressed up or down arrow
        /// or another character
        /// </summary>
        /// <param Name="e">The key press that triggered the chage</param>
        protected void UpdateCurrentSuggestion(PInputEventArgs e)
        {
            switch (e.KeyCode)
            {
                //When the user presses down, select the next suggestion in the list if possible
                case Keys.Down:
                    if (CurrentSuggestion < Suggestions.Length - 1) { CurrentSuggestion += 1; }
                    break;

                //When the user presses up, select the previous suggestion in the list, if possible
                case Keys.Up:
                    if (CurrentSuggestion > 0) { CurrentSuggestion -= 1; }
                    break;

                //If the user presses anything else, the suggestion list probably changed
                //So we want the first item in the list again
                default:
                    CurrentSuggestion = 0;
                    break;
            }

        }

        /// <summary>
        /// Push a new command to the list of recent commands
        /// </summary>
        /// <param Name="command">The command to push</param>
        protected void UpdateRecentCommands(ICommand command)
        {
            //Attempt to remove the executed command from the list. If that fails, remove the first one
            if (!_RecentCommands.Remove(command)) { _RecentCommands.RemoveAt(0); }

            //Add the executed command to the list
            _RecentCommands.Add(command);
        }


        #endregion Suggestion methods

        #region Preview methods

        /// <summary>
        /// Updates the preview of the currently selected command to the new arguments
        /// </summary>
        protected void UpdatePreview()
        {
            //Only display if the system is in the correct state
            if (ShouldPreview)
            {
                //Remove any existing preview or suggestions from the box
                AuxillaryBox.RemoveAllChildren();

                //Get the potential output from the selected command
                PText preview = ChosenCommand.Preview(Selection, Arguments);

                //Format the output for display
                preview.ConstrainWidthToTextWidth = false;
                preview.Width = Background.Width;
                preview.X = Background.Bounds.Left;
                preview.Y = Background.Bounds.Bottom;

                //Display the output
                AuxillaryBox.AddChild(preview);
            }
        }

        #endregion Preview methods

        #region Auxillary Box methods

        /// <summary>
        /// Show the suggestions, or previews, as appropriate
        /// </summary>
        protected void UpdateAuxillaryBox()
        {
            if (ShouldSuggest) { UpdateSuggestions(); }
            else { UpdatePreview(); }
            UpdateAuxillaryBoxBorder();
        }

        /// <summary>
        /// Ensure the border to the auxillary box always contains the contents of the box
        /// </summary>
        protected void UpdateAuxillaryBoxBorder()
        {
            //If there is already a border, remove it
            if (AuxillaryBoxBorder != null && AuxillaryBox.IndexOfChild(AuxillaryBoxBorder) > 0) { AuxillaryBox.RemoveChild(AuxillaryBoxBorder); }

            //Create a new border that contains the contents of the box
            AuxillaryBoxBorder = PPath.CreateRectangle(Background.Bounds.Left, Background.Bounds.Bottom, Background.Bounds.Width, AuxillaryBox.UnionOfChildrenBounds.Height);
            AuxillaryBox.AddChild(AuxillaryBoxBorder);
            AuxillaryBoxBorder.MoveToBack();
        }

        #endregion Auxillary Box methods
    }
}
