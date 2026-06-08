using HHSGame.Core.Classes;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Map;
using HHSGame.Core.Stats;

namespace HHSGame.Core.CharacterCreation
{
    /// <summary>
    /// Steps in the character creation dialogue flow.
    /// </summary>
    public enum CreationStep
    {
        Welcome,
        MapSelection,
        PlaystyleChoice,
        NameInput,
        DiceRoll,
        AttributeAssignment,
        ClassConfirmation,
        Complete
    }

    /// <summary>
    /// Manages the dialogue-based character creation flow.
    /// Tracks state through steps and produces the final character config.
    /// </summary>
    public sealed class CharacterCreationManager
    {
        private readonly ClassCatalog classCatalog;
        private readonly Random random;

        public CreationStep CurrentStep { get; private set; } = CreationStep.Welcome;

        // Map selection
        public string? SelectedMap { get; private set; }
        public bool UseCustomMap { get; private set; }
        public List<string> AvailableMaps { get; private set; } = [];

        // Playstyle
        public PlaystylePreference? SelectedPlaystyle { get; private set; }

        // Name
        public string PlayerName { get; private set; } = string.Empty;

        // Dice results
        public Attributes? RolledAttributes { get; private set; }
        public List<DiceRollResult>? DiceResults { get; private set; }

        // Class
        public ClassConfig? SelectedClass { get; private set; }

        // Final result
        public bool IsComplete => CurrentStep == CreationStep.Complete;

        public CharacterCreationManager(ClassCatalog classCatalog, Random random)
        {
            this.classCatalog = classCatalog;
            this.random = random;
        }

        /// <summary>
        /// Initializes available maps.
        /// </summary>
        public void Initialize()
        {
            AvailableMaps = MapLoader.GetAvailableMaps("data/maps");
            CurrentStep = CreationStep.Welcome;
        }

        /// <summary>
        /// Advances to the next step after welcome.
        /// </summary>
        public void AdvanceFromWelcome()
        {
            SelectedMap = "city-center";
            UseCustomMap = true;
            CurrentStep = CreationStep.PlaystyleChoice;
        }

        /// <summary>
        /// Sets map selection and advances to playstyle.
        /// </summary>
        public void SelectMap(string map, bool isCustom)
        {
            SelectedMap = map;
            UseCustomMap = isCustom;
            CurrentStep = CreationStep.PlaystyleChoice;
        }

        /// <summary>
        /// Sets playstyle preference and generates a name, advances to name step.
        /// </summary>
        public void SelectPlaystyle(PlaystylePreference preference)
        {
            SelectedPlaystyle = preference;
            PlayerName = DiceRoller.GenerateName(random);
            CurrentStep = CreationStep.NameInput;
        }

        /// <summary>
        /// Confirms name (or sets a custom one) and advances to dice roll.
        /// </summary>
        public void ConfirmName(string name)
        {
            PlayerName = string.IsNullOrWhiteSpace(name) ? DiceRoller.GenerateName(random) : name.Trim();
            RollDice();
            CurrentStep = CreationStep.DiceRoll;
        }

        /// <summary>
        /// Rolls dice for attributes based on playstyle preference.
        /// </summary>
        public void RollDice()
        {
            DiceResults = [];
            for (int i = 0; i < 5; i++)
            {
                DiceResults.Add(DiceRoller.RollAttribute(random));
            }

            if (SelectedPlaystyle != null)
            {
                RolledAttributes = DiceRoller.RollWithBias(random, SelectedPlaystyle);
            }
            else
            {
                RolledAttributes = DiceRoller.RollAllAttributes(random);
            }
        }

        /// <summary>
        /// Re-rolls all dice (player can re-roll once).
        /// </summary>
        public void RerollDice()
        {
            RollDice();
        }

        /// <summary>
        /// Confirms dice results and advances to class confirmation.
        /// </summary>
        public void ConfirmDice()
        {
            // Suggest class based on playstyle ID (e.g. "Resistant" matches class ID)
            if (SelectedPlaystyle?.SuggestedClassId != null)
            {
                SelectedClass = classCatalog.TryResolve(SelectedPlaystyle.SuggestedClassId, out ClassConfig? resolved) ? resolved : null;
            }

            // Fallback to default
            SelectedClass ??= classCatalog.GetDefault();
            CurrentStep = CreationStep.ClassConfirmation;
        }
        /// <summary>
        /// Sets a specific class and advances to completion.
        /// </summary>
        public void SelectClass(ClassConfig classConfig)
        {
            SelectedClass = classConfig;
            CurrentStep = CreationStep.Complete;
        }

        /// <summary>
        /// Confirms the suggested class and completes creation.
        /// </summary>
        public void ConfirmClass()
        {
            CurrentStep = CreationStep.Complete;
        }

        /// <summary>
        /// Gets all available classes for selection.
        /// </summary>
        public IReadOnlyList<ClassConfig> GetAvailableClasses()
        {
            return classCatalog.GetAll().ToList();
        }

        /// <summary>
        /// Gets the final attributes to use for the player.
        /// </summary>
        public Attributes GetFinalAttributes()
        {
            return RolledAttributes ?? new Attributes
            {
                Strength = 5, Perception = 5, Agility = 5, Charisma = 5, Intelligence = 5
            };
        }

        /// <summary>
        /// Gets the formatted dice roll summary for display.
        /// </summary>
        public string GetDiceSummary()
        {
            if (DiceResults == null || RolledAttributes == null)
            {
                return "No rolls yet.";
            }

            string[] names = ["STR", "PER", "AGI", "CHA", "INT"];
            int[] values = [
                RolledAttributes.Strength, RolledAttributes.Perception,
                RolledAttributes.Agility, RolledAttributes.Charisma,
                RolledAttributes.Intelligence
            ];

            List<string> lines = [];
            for (int i = 0; i < 5; i++)
            {
                DiceRollResult r = DiceResults[i];
                lines.Add($"{names[i]}: [{r.Rolls[0]}][{r.Rolls[1]}][{r.Rolls[2]}][{r.Rolls[3]}] drop {r.Dropped} = {values[i]}");
            }

            return string.Join("\n", lines);
        }
    }
}