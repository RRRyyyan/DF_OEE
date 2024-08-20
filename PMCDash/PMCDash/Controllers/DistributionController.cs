using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PMCDash.Models;
using PMCDash.Services;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Data;
using System.Data.SqlClient;

namespace PMCDash.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistributionController : BaseApiController
    {
        private readonly DeviceDistributionService _deviceDistributionService;
        ConnectStr _ConnectStr = new ConnectStr();
        public DistributionController(DeviceDistributionService deviceDistributionService)
        {
            _deviceDistributionService = deviceDistributionService;
        }

        /// <summary>
        /// 取得各廠機台狀態分布比例
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResponse<List<FactorysStatusDistribution>> Get()
        {
            var result = new List<FactorysStatusDistribution>();

            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json");
            var config = builder.Build();
            //double run = Convert.ToDouble(config["MCStatusRatio:RUN"]);
            //double idle = Convert.ToDouble(config["MCStatusRatio:IDLE"]);
            //double alarm = Convert.ToDouble(config["MCStatusRatio:ALARM"]);
            //double off = Convert.ToDouble(config["MCStatusRatio:OFF"]);
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

            //StatusDistribution statusDistribution = new StatusDistribution
            //(
            //    run: (decimal)run,
            //    idle: (decimal)idle,
            //    alarm: (decimal)alarm,
            //    off: (decimal)off
            //);
            StatusDistribution statusDistribution = new StatusDistribution
            (
                run: (decimal)Math.Round(run / devices.Count() * 100, 1),
                idle: (decimal)Math.Round(idle / devices.Count() * 100, 1),
                alarm: (decimal)Math.Round(alarm / devices.Count() * 100, 1),
                off: (decimal)Math.Round(off / devices.Count() * 100, 1)
            );

            result.Add(new FactorysStatusDistribution
                (
                    factoryName: "安南新廠",
                    distribution: statusDistribution

                ));

            //for (int i = 0; i < 5; i++)
            //{
            //    result.Add(new FactorysStatusDistribution
            //    (
            //        $@"FA-0{i + 1}",
            //        new StatusDistribution
            //        (
            //            run: 52.3m,
            //            idle: 27.8m,
            //            alarm: 16.7m,
            //            off: 3.2m
            //        )
            //    ));
            //}

            return new ActionResponse<List<FactorysStatusDistribution>>
            {
                Data = result
            };
        }

      /// <summary>
      /// 取得各產線狀態分布
      /// </summary>
      /// <param name="factory">廠區代號</param>
      /// <returns></returns>
        [HttpGet("productionline/{factory}")]
        public ActionResponse<List<ProductionLineStatusDistribution>> GetPrductionLines(string factory)
        {
            var result = new List<ProductionLineStatusDistribution>();

            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json");
            var config = builder.Build();
            //double run = Convert.ToDouble(config["MCStatusRatio:RUN"]);
            //double idle = Convert.ToDouble(config["MCStatusRatio:IDLE"]);
            //double alarm = Convert.ToDouble(config["MCStatusRatio:ALARM"]);
            //double off = Convert.ToDouble(config["MCStatusRatio:OFF"]);
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

            //StatusDistribution statusDistribution = new StatusDistribution
            //(
            //    run: (decimal)run,
            //    idle: (decimal)idle,
            //    alarm: (decimal)alarm,
            //    off: (decimal)off
            //);

            StatusDistribution statusDistribution = new StatusDistribution
            (
                run: (decimal)Math.Round(run / devices.Count() * 100, 1),
                idle: (decimal)Math.Round(idle / devices.Count() * 100, 1),
                alarm: (decimal)Math.Round(alarm / devices.Count() * 100, 1),
                off: (decimal)Math.Round(off / devices.Count() * 100, 1)
            );


            result.Add(new ProductionLineStatusDistribution
                (
                    productionLineName: "全產線",
                    distribution: statusDistribution

                ));


            //for (int i = 0; i < 5; i++)
            //{
            //    result.Add(new ProductionLineStatusDistribution
            //    (
            //        $@"PRL-0{i + 1}",
            //        new StatusDistribution
            //        (
            //            run: 52.3m,
            //            idle: 27.8m,
            //            alarm: 16.7m,
            //            off: 3.2m
            //        )
            //    ));
            //}

            return new ActionResponse<List<ProductionLineStatusDistribution>>
            {
                Data = result
            };
        }

        /// <summary>
        /// 取得全廠機台狀態分布比例(狀態比例)
        /// </summary>
        /// <returns></returns>
        [HttpGet("all")]
        public ActionResponse<StatusDistribution> GetAllStatusDistribution()
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json");
            var config = builder.Build();
            //double run = Convert.ToDouble(config["MCStatusRatio:RUN"]);
            //double idle = Convert.ToDouble(config["MCStatusRatio:IDLE"]);
            //double alarm = Convert.ToDouble(config["MCStatusRatio:ALARM"]);
            //double off = Convert.ToDouble(config["MCStatusRatio:OFF"]);
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
                if(!string.IsNullOrEmpty(item.MAKTX))
                {
                    run += 1;
                }
                else
                {
                    idle += 1;
                }
            }
            #endregion


            //////////////////////////////////////////////////////////////
            //Random random = new Random();
            //// 隨機生成 run 和 idle 的數字
            //double run = Math.Round((random.NextDouble() * 0.2 + 0.5) * 100,1);
            //double idle = Math.Round((random.NextDouble() * 0.1 + 0.2) * 100,1);
            //// 計算 alarm 和 off，確保總和為100
            //double alarm = Math.Round( random.NextDouble() * (100 - run - idle),1);
            //double off = 100 - run - idle - alarm;
            /////////////////////////////////////////////////////////////
            // 將結果封裝到 StatusDistribution 對象中
            StatusDistribution statusDistribution = new StatusDistribution
            (
                run: (decimal) Math.Round(run/ devices.Count()*100,1),
                idle: (decimal) Math.Round(idle / devices.Count()*100, 1),
                alarm: (decimal) Math.Round(alarm / devices.Count()*100, 1),
                off: (decimal) Math.Round(off / devices.Count()*100, 1)
            );

            // 返回 ActionResponse
            return new ActionResponse<StatusDistribution>
            {
                Data = statusDistribution
            };
            //return new ActionResponse<StatusDistribution>
            //{
            //    Data = new StatusDistribution
            //        (
            //            run: 52.3m,
            //            idle: 27.8m,
            //            alarm: 16.7m,
            //            off: 3.2m
            //        )
            //};
        }

    }
}
