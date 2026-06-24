using System.Text.Json;
using System.Text.Json.Serialization;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Items;

namespace HHSGameTest.HHSGame;

[TestClass]
public class JsonDeserializationTests
{
    [TestMethod]
    public void DeserializeWeapons_WithNumericEnums_ShouldWork()
    {
        // Arrange: JSON with numeric enum values (as in actual weapons.json)
        string json = """
        {
            "weapons": [
                {
                    "Id": "TestWeapon",
                    "Name": "Test Weapon",
                    "Rarity": 0,
                    "Value": 10,
                    "Weight": 1.0,
                    "Damage": 5,
                    "Penetration": 0,
                    "Range": 1,
                    "WeaponType": 0,
                    "Trajectory": 0
                }
            ]
        }
        """;

        // Act
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        // This should fail with current code because Rarity is numeric
        var result = JsonSerializer.Deserialize<WeaponListWrapper>(json, options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Weapons);
        Assert.AreEqual(1, result.Weapons.Count);
        Assert.AreEqual("TestWeapon", result.Weapons[0].Id);
        Assert.AreEqual(ItemRarity.Common, result.Weapons[0].Rarity);
    }

    [TestMethod]
    public void DeserializeWeapons_WithStringEnums_ShouldWork()
    {
        // Arrange: JSON with string enum values
        string json = """
        {
            "weapons": [
                {
                    "Id": "TestWeapon",
                    "Name": "Test Weapon",
                    "Rarity": "Common",
                    "Value": 10,
                    "Weight": 1.0,
                    "Damage": 5,
                    "Penetration": 0,
                    "Range": 1,
                    "WeaponType": "MeleeLight",
                    "Trajectory": "Line"
                }
            ]
        }
        """;

        // Act
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        var result = JsonSerializer.Deserialize<WeaponListWrapper>(json, options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Weapons);
        Assert.AreEqual(1, result.Weapons.Count);
        Assert.AreEqual(ItemRarity.Common, result.Weapons[0].Rarity);
    }

    [TestMethod]
    public void DeserializeWeapons_WithCustomConverter_ShouldHandleBoth()
    {
        // Arrange: JSON with numeric enum values
        string json = """
        {
            "weapons": [
                {
                    "Id": "TestWeapon",
                    "Name": "Test Weapon",
                    "Rarity": 0,
                    "Value": 10,
                    "Weight": 1.0,
                    "Damage": 5,
                    "Penetration": 0,
                    "Range": 1,
                    "WeaponType": 0,
                    "Trajectory": 0
                }
            ]
        }
        """;

        // Act: Use custom converter that handles both numeric and string
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new FlexibleEnumConverter<ItemRarity>() }
        };

        var result = JsonSerializer.Deserialize<WeaponListWrapper>(json, options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Weapons);
        Assert.AreEqual(1, result.Weapons.Count);
        Assert.AreEqual(ItemRarity.Common, result.Weapons[0].Rarity);
    }
}

// Wrapper class for deserialization
public class WeaponListWrapper
{
    public List<WeaponDefinition> Weapons { get; set; } = [];
}

/// <summary>
/// 灵活的枚举转换器，支持数字和字符串格式
/// </summary>
public class FlexibleEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? stringValue = reader.GetString();
            if (Enum.TryParse<T>(stringValue, true, out var result))
            {
                return result;
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            int intValue = reader.GetInt32();
            if (Enum.IsDefined(typeof(T), intValue))
            {
                return (T)(object)intValue;
            }
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
