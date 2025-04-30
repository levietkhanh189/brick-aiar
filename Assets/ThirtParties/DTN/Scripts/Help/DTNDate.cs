using System;
using System.Collections.Generic;

public class DTNDate
{

    public static long NowTimeInterval()
    {
        return TimeInterval(DateTime.UtcNow);
    }

    public static long TimeInterval(DateTime date)
    {
        DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        long currentEpochTime = (long)(date - epochStart).TotalSeconds;

        return currentEpochTime;
    }

    public static string GetDayHouseMinSecondString(long timeInterval)
    {

        long second = timeInterval % 60;
        timeInterval = timeInterval / 60;

        long min = timeInterval % 60;
        timeInterval = timeInterval / 60;

        long hour = timeInterval;
        //List<long> listResult = new List<long>();
        //listResult.Add((timeInterval % (60 * 60 * 60 *24)) / (60 * 60 * 60 *24));//day
        //listResult.Add((timeInterval % (60 * 60 * 60)) / (60 * 60 *60));//hours
        //listResult.Add((timeInterval % (60 * 60))/ (60 * 60));//min
        //listResult.Add(timeInterval % 60);//second

        //bool isZero = true;
        string result = "";
        //foreach (long time in listResult)
        //{
        //    if (time == 0 && isZero)
        //    {
        //        break;
        //    }
        //    if (result.Length == 0)
        //    {
        //        result = time+"";
        //    }
        //    else
        //    {
        //        result += ":" + time;
        //    }
            
        //}

        return hour+":"+min+":"+second;
    }
}


