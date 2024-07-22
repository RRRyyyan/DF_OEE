using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PMCDash.Models;
namespace PMCDash.Services
{
    public class AlarmService
    {
        public AlarmService()
        {

        }

        public List<AlarmStatistics> GetAlarm(object requst)
        {
            switch (requst)
            {
                case ActionRequest<Factory> req:
                    break;
                case ActionRequest<RequestFactory> req:
                    break;
            }

            var result = new List<AlarmStatistics>();
            Random random = new Random();
            
            for (int i = 0; i < 10; i++)
            {
                result.Add(new AlarmStatistics
                (
                    alarmMSg: $@"Alarm0059{i,2:00}",
                    times: random.Next(10, 40),
                    totalMin: random.Next(100, 200)
                ));
            }
            return result;
        }
        public List<AlarmPercent> GetAlarmPercent(object requst)
        {
            var EachAlarm = new List<AlarmStatistics>();
            var result = new List<AlarmPercent>();
            Random random = new Random();
            for (int i = 0; i < 10; i++)
            {
                EachAlarm.Add(new AlarmStatistics
                (
                    alarmMSg: $@"Alarm0059{i,2:00}",
                    times: random.Next(10, 40),
                    totalMin: random.Next(100, 200)
                ));
            }
            // 計算 times 和 totalMin 的總和
            int totalTimes = EachAlarm.Sum(alarm => alarm.Times);
            double totalMin = EachAlarm.Sum(alarm => alarm.TotalMin);

            // 計算每個 alarmMSg 的比例並填充 result 集合
            foreach (var alarm in EachAlarm)
            {
                double timesRatio = (double)alarm.Times / totalTimes * 100;
                double minRatio = (double)alarm.TotalMin / totalMin * 100;

                result.Add(new AlarmPercent(
                    alarmMSg: alarm.AlarmMSg,
                    Times_Ratio: timesRatio,
                    Min_Ratio: minRatio
                ));
            }
            return result;
        }

    }
}
