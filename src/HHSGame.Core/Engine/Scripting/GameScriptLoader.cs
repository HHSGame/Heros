using System.Text.Json;

namespace HHSGame.Core.Engine.Scripting
{
    public sealed class GameScriptLoader(GameDataLoader dataLoader)
    {
        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        public GameScript Load(string path)
        {
            byte[] payload = dataLoader.LoadBytes(path);
            using JsonDocument document = JsonDocument.Parse(payload);
            GameScript script = Parse(document.RootElement, path);
            Validate(script, path);
            return script;
        }

        private static GameScript Parse(JsonElement root, string path)
        {
            if (root.ValueKind == JsonValueKind.Array)
            {
                return new GameScript
                {
                    Steps = ParseInputArray(root, path)
                };
            }

            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException($"Script must be a JSON object or array in {path}.");
            }

            if (TryGetProperty(root, "inputs", out JsonElement inputsElement))
            {
                return new GameScript
                {
                    Name = ReadName(root),
                    Steps = ParseInputArray(inputsElement, path)
                };
            }

            if (TryGetProperty(root, "steps", out JsonElement stepsElement) || TryGetProperty(root, "script", out stepsElement))
            {
                List<ScriptStep>? steps = JsonSerializer.Deserialize<List<ScriptStep>>(stepsElement.GetRawText(), Options);
                if (steps == null)
                {
                    throw new InvalidDataException($"Script steps are empty in {path}.");
                }

                return new GameScript
                {
                    Name = ReadName(root),
                    Steps = steps
                };
            }

            throw new InvalidDataException($"Script must contain 'inputs' or 'steps' in {path}.");
        }

        private static string? ReadName(JsonElement root)
        {
            if (TryGetProperty(root, "name", out JsonElement nameElement) && nameElement.ValueKind == JsonValueKind.String)
            {
                return nameElement.GetString();
            }

            return null;
        }

        private static List<ScriptStep> ParseInputArray(JsonElement inputArray, string path)
        {
            if (inputArray.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException($"Script inputs must be an array in {path}.");
            }

            List<ScriptStep> steps = [];
            foreach (JsonElement element in inputArray.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.String)
                {
                    throw new InvalidDataException($"Script inputs must be strings in {path}.");
                }

                string? input = element.GetString();
                if (string.IsNullOrWhiteSpace(input))
                {
                    throw new InvalidDataException($"Script input cannot be empty in {path}.");
                }

                steps.Add(new ScriptStep { Input = input });
            }

            return steps;
        }

        private static void Validate(GameScript script, string path)
        {
            if (script.Steps.Count == 0)
            {
                throw new InvalidDataException($"Script has no steps in {path}.");
            }

            for (int i = 0; i < script.Steps.Count; i++)
            {
                ValidateStep(script.Steps[i], path, $"steps[{i}]");
            }
        }

        private static void ValidateStep(ScriptStep step, string path, string context)
        {
            if (step.Input == null && step.WaitUntil == null && step.When == null && step.Repeat == null && step.Assert == null)
            {
                throw new InvalidDataException($"Script step has no action at {context} in {path}.");
            }

            if (step.When != null && (step.Then == null || step.Then.Count == 0))
            {
                throw new InvalidDataException($"Script 'when' must include 'then' steps at {context} in {path}.");
            }

            if (step.Repeat != null)
            {
                if (step.Repeat.While == null)
                {
                    throw new InvalidDataException($"Script 'repeat' must include 'while' at {context} in {path}.");
                }

                if (step.Repeat.Steps.Count == 0)
                {
                    throw new InvalidDataException($"Script 'repeat' must include steps at {context} in {path}.");
                }

                for (int i = 0; i < step.Repeat.Steps.Count; i++)
                {
                    ValidateStep(step.Repeat.Steps[i], path, $"{context}.repeat.steps[{i}]");
                }
            }

            if (step.Then != null)
            {
                for (int i = 0; i < step.Then.Count; i++)
                {
                    ValidateStep(step.Then[i], path, $"{context}.then[{i}]");
                }
            }

            if (step.Else != null)
            {
                for (int i = 0; i < step.Else.Count; i++)
                {
                    ValidateStep(step.Else[i], path, $"{context}.else[{i}]");
                }
            }
        }

        private static bool TryGetProperty(JsonElement root, string name, out JsonElement value)
        {
            foreach (JsonProperty property in root.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
