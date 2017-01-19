using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OVTools {
    public const float thousand = 1000;
    public const float million = 1000 * thousand;
    public const float billion = 1000 * million;
    public const float trillion = 1000 * billion;
    public const float minute = 60;
    public const float hour = 60 * minute;
    public const float day = 24 * hour;
    public const float week = 7 * day;
    public const float year = 52 * week;

    public static string FormatTime(float time)
    {
        string timeStr;
        string suffix = "S";
                if (time < minute)    { }
        else if (time < hour) { time /= minute; suffix = "M"; }
        else if (time < day)  { time /= hour;   suffix = "H"; }
        else if (time < week) { time /= day;    suffix = "D"; }
        else if (time < year) { time /= week;   suffix = "W"; }

        int integer = (int)time;
        int tenth = (int)((time - integer) * 10);
        timeStr = integer.ToString() + "." + tenth + suffix;
        return timeStr;
    }
    public static string FormatMoney(float money)
	{
		return "$" + FormatNumber(money);
    }
    public static string FormatDistance(float dist)
    {
        return FormatNumber(dist) + "m";
    }
    public static string FormatNumber(float number) {
		string suffix = "";
		if (number < thousand)  		{ }
		else if (number < million)	{ number /= thousand; suffix = "K"; }
		else if (number < billion)	{ number /= million; suffix = "M"; }
		else if (number < trillion)	{ number /= billion; suffix = "B"; }
		else 						{ number /= trillion; suffix = "T"; }
		int integer = (int)number;
		int tenth = (int)((number - integer) * 10);
		return integer + "." + tenth + suffix;
	}
}