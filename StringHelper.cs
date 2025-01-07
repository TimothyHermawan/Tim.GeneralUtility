using System;
using System.Collections;
using System.Collections.Generic;
using ChocDino.UIFX;
using Tim.GeneralUtility;
using UnityEngine;

public static class StringHelper
{
    public static string ConvertAsCurrency(this string value, bool adjustColor)
    {
        double val = double.Parse(value);

        string color = "#000000"; // color.black

        if(val < 0)
        {
            color = "#C70000"; // color.red
        }
        else if( val > 0)
        {
            color = "#009300"; // color.green
        }


        var doubleValue = double.Parse(value);
        bool containsDecimal = doubleValue % 1 != 0;
        int decimalPlaces = containsDecimal ? 2 : 0;

        if (adjustColor)
        {
            return val.ToString($"N{decimalPlaces}").WithColor(color);
        }
        else
        {
            return val.ToString($"N{decimalPlaces}");
        }
    }

    public static string ConvertAsCurrency(this string value)
    {
        var doubleValue = double.Parse(value);
        bool containsDecimal = Math.Abs(doubleValue) % 1 != 0;
        int decimalPlaces = containsDecimal ? 2 : 0;

        return doubleValue.ToString($"N{decimalPlaces}");
    }

    public static string ConvertAsNumber(this string value)
    {
        return double.Parse(value).ToString("N0");
    }

    public static string ConvertToLocalTime(this string dateTimeString)
    {
        return DateTime.Parse(dateTimeString).ToLocalTime().ToString();
    }

    public static string ConvertToLocalTimeTwoLine(this string dateTimeString)
    {
        return DateTime.Parse(dateTimeString).ToLocalTime().ToString().ReplaceFirstSpaceWithLineBreak();
    }

    static string ReplaceFirstSpaceWithLineBreak(this string input)
    {
        int index = input.IndexOf(' '); // Find the index of the first space
        if (index != -1)
        {
            // Replace the first space with a line break
            return input.Substring(0, index) + Environment.NewLine + input.Substring(index + 1);
        }
        return input; // Return the original string if no space is found
    }

    public static string Underline(this string value)
    {
        return $"<u>{value}</u>";
    }
}
