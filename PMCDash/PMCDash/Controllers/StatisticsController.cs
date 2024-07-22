using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMCDash.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using PMCDash.Services;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Data;
using System.Data.SqlClient;

namespace PMCDash.Controllers
{
    [Route("api/[controller]")]
    public class StatisticsController : BaseApiController
    {
        private readonly AlarmService _alarmService;
        ConnectStr _ConnectStr = new ConnectStr();
        public StatisticsController(AlarmService alarmService)
        {
            _alarmService = alarmService;
        }

        /// <summary>
        /// 取得各廠狀態統計資料(狀態個數)
        /// </summary>
        /// <returns></returns>
        public ActionResponse<List<FactoryStatistics>> GetStaticsByFactory()
        {
            var result = new List<FactoryStatistics>();
            int MachineNum = 90;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json");
            var config = builder.Build();
            //double run = Math.Round(MachineNum * Convert.ToDouble(config["MCStatusRatio:RUN"]) / 100.0);
            //double idle = Math.Round(MachineNum * Convert.ToDouble(config["MCStatusRatio:IDLE"]) / 100.0);
            //double alarm = Math.Round(MachineNum * Convert.ToDouble(config["MCStatusRatio:ALARM"]) / 100.0);
            //double off = Math.Round(MachineNum * Convert.ToDouble(config["MCStatusRatio:OFF"]) / 100.0);
            double run = 0;
            double idle = 0;
            double alarm = 0;
            double off = 0;
            var devices = new List<DeviceList>();

            #region 由實際生產資料統計廠區機台狀態
            //取得工單資料
            var sqlStr = @$"SELECT
	                            b.remark,
                                a.WorkOrderID,
                                a.OPID,
	                            c.MAKTX
                                
                            FROM
                               {_ConnectStr.APSDB}.[dbo].[WipRegisterLog] AS a
                            LEFT JOIN
                                {_ConnectStr.APSDB}.[dbo].[Device] AS b ON a.DeviceID = b.ID
                            LEFT JOIN
                                {_ConnectStr.APSDB}.[dbo].[Assignment] AS c ON a.WorkOrderID = c.OrderID AND a.OPID = c.OPID
                            WHERE b.remark is not null and b.external_com=0";
            using (var conn = new SqlConnection(_ConnectStr.Local))
            {
                using (var comm = new SqlCommand(sqlStr, conn))
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();
                    using (SqlDataReader SqlData = comm.ExecuteReader())
                    {
                        if (SqlData.HasRows)
                        {
                            while (SqlData.Read())
                            {

                                devices.Add(new DeviceList(
                                    remark: SqlData["remark"].ToString().Trim(),
                                    workorderID: SqlData["WorkOrderID"].ToString().Trim(),
                                    opid: SqlData["OPID"].ToString().Trim(),
                                    maktx: SqlData["MAKTX"].ToString().Trim()

                                    ));

                            };
                        }
                    }
                }
            }

            var status = new string[] { "RUN", "IDLE", "ALARM", "OFF" };
            foreach (var item in devices)
            {
                if (!string.IsNullOrEmpty(item.MAKTX))
                {
                    run += 1;
                }
                else
                {
                    idle += 1;
                }
            }
            #endregion

            result.Add(new FactoryStatistics
                (
                    factoryName : "安南新廠",
                    statistics :
                    new StatusStatistics
                    (
                        run: (int)run,
                        idle: (int)idle,
                        alarm: (int)alarm,
                        off: (int)off
                    )
                ));
            return new ActionResponse<List<FactoryStatistics>>
            {
                Data = result
            };
        }

        /// <summary>
        /// 取得特定廠區中的產線統計資料
        /// </summary>
        /// <param name="factory">廠區名稱</param>
        /// <returns></returns>
        [HttpGet("productionline/{factory}")]
        public ActionResponse<FactoryStatisticsImformation> GetStaticsByProductionLine(string factory)
        {
            var result = new List<ProductionLineStatistics>();
            int MachineNum = 90;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json");
            var config = builder.Build();
            //double run = Math.Round(MachineNum * Convert.ToDouble(config["MCStatusRatio:RUN"]) / 100.0);
            //double idle = Math.Round(MachineNum * Convert.ToDouble(config["MCStatusRatio:IDLE"]) / 100.0);
            //double alarm = Math.Round(MachineNum * Convert.ToDouble(config["MCStatusRatio:ALARM"]) / 100.0);
            //double off = Math.Round(MachineNum * Convert.ToDouble(config["MCStatusRatio:OFF"]) / 100.0);

            double run = 0;
            double idle = 0;
            double alarm = 0;
            double off = 0;
            var devices = new List<DeviceList>();

            #region 由實際生產資料統計廠區機台狀態
            //取得工單資料
            var sqlStr = @$"SELECT
	                            b.remark,
                                a.WorkOrderID,
                                a.OPID,
	                            c.MAKTX
                                
                            FROM
                               {_ConnectStr.APSDB}.[dbo].[WipRegisterLog] AS a
                            LEFT JOIN
                                {_ConnectStr.APSDB}.[dbo].[Device] AS b ON a.DeviceID = b.ID
                            LEFT JOIN
                                {_ConnectStr.APSDB}.[dbo].[Assignment] AS c ON a.WorkOrderID = c.OrderID AND a.OPID = c.OPID
                            WHERE b.remark is not null and b.external_com=0";
            using (var conn = new SqlConnection(_ConnectStr.Local))
            {
                using (var comm = new SqlCommand(sqlStr, conn))
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();
                    using (SqlDataReader SqlData = comm.ExecuteReader())
                    {
                        if (SqlData.HasRows)
                        {
                            while (SqlData.Read())
                            {

                                devices.Add(new DeviceList(
                                    remark: SqlData["remark"].ToString().Trim(),
                                    workorderID: SqlData["WorkOrderID"].ToString().Trim(),
                                    opid: SqlData["OPID"].ToString().Trim(),
                                    maktx: SqlData["MAKTX"].ToString().Trim()

                                    ));

                            };
                        }
                    }
                }
            }

            var status = new string[] { "RUN", "IDLE", "ALARM", "OFF" };
            foreach (var item in devices)
            {
                if (!string.IsNullOrEmpty(item.MAKTX))
                {
                    run += 1;
                }
                else
                {
                    idle += 1;
                }
            }
            #endregion
            result.Add(new ProductionLineStatistics
                (
                    productionLineName: "全產線",
                    statistics:
                    new StatusStatistics
                    (
                        run: (int)run,
                        idle: (int)idle,
                        alarm: (int)alarm,
                        off: (int)off
                    )
                ));


            //for (int i = 0; i < 5; i++)
            //{
            //    result.Add(new ProductionLineStatistics
            //    (
            //        $@"PRL-0{i + 1}",
            //        new StatusStatistics
            //        (
            //            run: 52,
            //            idle: 28,
            //            alarm: 17,
            //            off: 3
            //        )
            //    ));
            //}
            return new ActionResponse<FactoryStatisticsImformation>
            {
                Data = new FactoryStatisticsImformation(factory, result)
            };
        }

        /// <summary>
        /// 取的特定產線中的機台狀態統計資料
        /// </summary>       
        /// <returns></returns>
        [HttpPost("status")]
        public ActionResponse<ProductionLineMachineImformation> GetMachineStatus([FromBody] RequestFactory prl)
        {
            var result = new List<MachineStatus>();

            var devices = new List<DeviceList>();

            #region 撈取機台編號資料
            //取得工單資料
            var sqlStr = @$"SELECT
	                            b.remark,
                                a.WorkOrderID,
                                a.OPID,
	                            c.MAKTX
                            FROM
                               {_ConnectStr.APSDB}.[dbo].[WipRegisterLog] AS a
                            LEFT JOIN
                                {_ConnectStr.APSDB}.[dbo].[Device] AS b ON a.DeviceID = b.ID
                            LEFT JOIN
                                {_ConnectStr.APSDB}.[dbo].[Assignment] AS c ON a.WorkOrderID = c.OrderID AND a.OPID = c.OPID
                            WHERE b.remark is not null";
            using (var conn = new SqlConnection(_ConnectStr.Local))
            {
                using (var comm = new SqlCommand(sqlStr, conn))
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();
                    using (SqlDataReader SqlData = comm.ExecuteReader())
                    {
                        if (SqlData.HasRows)
                        {
                            while (SqlData.Read())
                            {

                                devices.Add(new DeviceList(
                                    remark : SqlData["remark"].ToString().Trim(),
                                    workorderID : SqlData["WorkOrderID"].ToString().Trim(),
                                    opid: SqlData["OPID"].ToString().Trim(),
                                    maktx: SqlData["MAKTX"].ToString().Trim()

                                    ));

                            };
                        }
                    }
                }
            }
            #endregion

            var status = new string[] { "RUN", "IDLE", "ALARM", "OFF" };
            foreach(var item in devices)
            {
                result.Add(new MachineStatus
                (
                    machineName : item.Remark,
                    status : !String.IsNullOrEmpty(item.MAKTX)?"RUN":"IDLE"
                ));
            }
            //for (int i = 0; i < 20; i++)
            //{
            //    result.Add(new MachineStatus
            //    (
            //        $@"CNC-{i + 1,2:00}",
            //        status[(i + 1) % 4]
            //    ));
            //}
            return new ActionResponse<ProductionLineMachineImformation>
            {
                Data = new ProductionLineMachineImformation(result)
            };
        }

        /// <summary>
        /// 取得TOP 10 異常訊息累計資料
        /// </summary>
        /// <param name="request">廠區名稱 EX: FA-05、all(整廠) 產線名稱 EX: 空白、PR-01</param>
        /// <returns></returns>
        [HttpPost("alarm")]
        public ActionResponse<List<AlarmStatistics>> GetAlarm([FromBody] RequestFactory request)
        {
            return new ActionResponse<List<AlarmStatistics>>
            {
                Data = _alarmService.GetAlarm(request)
            };
        }

        /// <summary>
        /// 取得各警報時間與次數占比
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("alarm_percent")]
        public ActionResponse<List<AlarmPercent>> GetAlarmPercent([FromBody] RequestFactory request)
        {
            return new ActionResponse<List<AlarmPercent>>
            {
                Data = _alarmService.GetAlarmPercent(request)
            };
        }

        /// <summary>
        /// 取得停機次數統計
        /// </summary>
        /// <param name="request">廠區名稱 EX: FA-05、all(整廠) 產線名稱 EX: 空白、PR-01</param>
        /// <returns></returns>
        [HttpPost("stop")]
        public ActionResponse<List<StopStatistics>> GetStop([FromBody] RequestFactory request)
        {
            var result = new List<StopStatistics>();
            var devices = new List<Device>();

            #region 撈取機台編號資料
            //取得工單資料
            var sqlStr = @$"SELECT distinct remark
                            FROM {_ConnectStr.APSDB}.[dbo].[Device]";
            using (var conn = new SqlConnection(_ConnectStr.Local))
            {
                using (var comm = new SqlCommand(sqlStr, conn))
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();
                    using (SqlDataReader SqlData = comm.ExecuteReader())
                    {
                        if (SqlData.HasRows)
                        {
                            while (SqlData.Read())
                            {

                                devices.Add(new Device(SqlData["remark"].ToString().Trim(), SqlData["remark"].ToString().Trim()));

                            };
                        }
                    }
                }
            }
            #endregion



            var random = new Random(Guid.NewGuid().GetHashCode());
            for(int i=0;i<devices.Count();i+=10)
            {
                result.Add(new StopStatistics(
                    machineName: devices[i].Text,
                    times: random.Next(5, 150)
                    ));
            }
            //for (int i = 0; i < 10; i++)
            //{
            //    result.Add(new StopStatistics($@"CNC-{i,2:00}", random.Next(5, 150)));
            //}
            return new ActionResponse<List<StopStatistics>>
            {
                Data = result
            };
        }

    }
}
