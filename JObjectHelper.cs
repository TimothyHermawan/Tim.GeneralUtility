using System;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class JObjectHelper
{
    public static string GetCurrentValueString(this JObject jsonObject, string fetcher)
    {
        // Split the parameter to get the path
        string[] parts = fetcher.Split(new char[] { '[', ']', '.' }, StringSplitOptions.RemoveEmptyEntries);

        // Traverse the JObject based on the path parts
        JToken token = jsonObject;
        foreach (var part in parts)
        {
            if (token == null) return "";

            if (int.TryParse(part, out int index))
            {
                // Check if token is an array and contains the index
                if (token.Type == JTokenType.Array && token.Count() > index)
                {
                    token = token[index];
                }
                else
                {
                    return ""; // Index out of range
                }
            }
            else
            {
                // Check if token is an object and contains the key
                if (token.Type == JTokenType.Object && token[part] != null)
                {
                    token = token[part];
                }
                else
                {
                    return ""; // Key not found
                }
            }
        }

        return token != null ? token.ToString() : "";
    }

    public static string GetCurrentValueStringWithCondition(this JObject jsonObject, string fetcher)
    {
        // Split the parameter to get the path, and extract conditions if present
        string[] parts = fetcher.Split(new char[] { '[', ']', '.' }, StringSplitOptions.RemoveEmptyEntries);

        JToken token = jsonObject;
        foreach (var part in parts)
        {
            if (token == null) return "";

            // Check if the part contains a condition, e.g., mode=1
            string[] conditionParts = part.Split('=');
            if (conditionParts.Length == 2)
            {
                // This part has a condition (e.g., mode=1)
                string key = conditionParts[0];
                string expectedValue = conditionParts[1];

                // Check if the token is an array
                if (token.Type == JTokenType.Array)
                {
                    // Iterate through the array to find the object that matches the condition
                    var matchingToken = token.FirstOrDefault(item => item[key] != null && item[key].ToString() == expectedValue);
                    if (matchingToken != null)
                    {
                        token = matchingToken;
                    }
                    else
                    {
                        return ""; // No matching token found
                    }
                }
                else
                {
                    return ""; // Expected an array but found a different type
                }
            }
            else if (int.TryParse(part, out int index))
            {
                // Handle array index
                if (token.Type == JTokenType.Array && token.Count() > index)
                {
                    token = token[index];
                }
                else
                {
                    return ""; // Index out of range
                }
            }
            else
            {
                // Normal object key lookup
                if (token.Type == JTokenType.Object && token[part] != null)
                {
                    token = token[part];
                }
                else
                {
                    return ""; // Key not found
                }
            }
        }

        return token != null ? token.ToString() : "";
    }


    // Special case where the expression need to be resolved in one expression. Like the one in "GetAgentProfits.TitleText"
    public static string GetCurrentValueCurrency(this JObject jsonObject, string fetcher)
    {
        string val = jsonObject.GetCurrentValueString(fetcher);

        return StringHelper.ConvertAsCurrency(val);
    }

    public static string GetCurrentValueAsNumber(this JObject jsonObject, string fetcher)
    {
        string val = jsonObject.GetCurrentValueString(fetcher);

        return StringHelper.ConvertAsNumber(val);
    }

    public static string ResolveJObjectAndExpression(this JObject JObject, string JObjectKey, string expression, MonoBehaviour target, string defaultValue = "")
    {
        string resolved = defaultValue;

        var value = string.IsNullOrEmpty(JObjectKey) ? defaultValue : JObject.GetCurrentValueStringWithCondition(JObjectKey);

        if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(expression))
        {
            string test = string.IsNullOrEmpty(expression) ? value : expression.Replace("@value", value);

            resolved = ExpressionResolver.ResolveTemplate(test, target) as string;

            return resolved;
        }
        else
        {
            return value;
        }


    }

    public static JObject ModifyJson(ref JObject jsonObj, string modifierKey, object newValue)
    {
        // Parse the key into parts while respecting quotes for id with dots
        var parts = ParseModifierKey(modifierKey);

        JToken currentToken = jsonObj;
        JToken parentToken = null;
        string lastPart = null;

        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            lastPart = part;  // Store the last part for assigning the value at the end

            if (part.Contains('='))
            {
                // Handle the condition like prizes.id="direct_prize_0.01" or mode=1
                var split = part.Split(new[] { '=' }, 2);  // Split into the field name and the value
                string fieldName = split[0].Trim();  // e.g., "id" or "mode"
                string conditionValue = split[1].Trim('"');  // Strip quotes around the value if any

                if (currentToken is JArray array)
                {
                    // Find the matching element in the array based on the condition
                    currentToken = array.FirstOrDefault(o => o[fieldName].ToString() == conditionValue);

                    if (currentToken == null)
                    {
                        throw new ArgumentException($"No element found with condition '{fieldName}={conditionValue}'");
                    }
                }
            }
            else
            {
                // Handle array indexes, like 'payoutDatas[0]'
                if (part.EndsWith("]"))
                {
                    // Parse the array index
                    var arrayPart = part.Substring(0, part.IndexOf('['));
                    var indexString = part.Substring(part.IndexOf('[') + 1, part.IndexOf(']') - part.IndexOf('[') - 1);
                    int index = int.Parse(indexString);  // Convert to integer

                    parentToken = currentToken[arrayPart];
                    if (parentToken == null || !(parentToken is JArray))
                    {
                        throw new ArgumentException($"Array '{arrayPart}' does not exist in the JSON.");
                    }

                    // Access the array element by index
                    currentToken = parentToken[index];
                }
                else
                {
                    // Navigate through the object, updating the current token
                    parentToken = currentToken;
                    currentToken = currentToken[part];

                    if (currentToken == null)
                    {
                        throw new ArgumentException($"Path '{part}' does not exist in the JSON.");
                    }
                }
            }
        }

        // Assign the new value to the final token
        if (parentToken is JObject parentObject)
        {
            parentObject[lastPart] = JToken.FromObject(newValue);
        }

        // Return the modified JObject
        return jsonObj;
    }


    // Function to parse the modifier key into parts while respecting quotes
    private static string[] ParseModifierKey(string modifierKey)
    {
        // This regex ensures dots within quotes aren't treated as separators
        var parts = Regex.Split(modifierKey, @"(?<!\\)\.(?=(?:[^""]*""[^""]*"")*[^""]*$)");
        return parts;
    }
}
