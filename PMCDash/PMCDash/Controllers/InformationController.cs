using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PMCDash.Models;
using System.Data;
using System.Data.SqlClient;

namespace PMCDash.Controllers
{
    [Route("api/[controller]")]
    public class InformationController : BaseApiController
    {

        ConnectStr _ConnectStr = new ConnectStr();
        public InformationController()
        {

        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("Factorys")]
        //public ActionResponse<List<FactoryImformation>> GetFactory()
        //{
        //    var result = new List<FactoryImformation>();
        //    var factorynName = new string[] { "大里廠", "松竹廠", "松竹五廠", "鐮村廠", "松竹七廠" };
        //    for (int i = 0; i < 5; i++)
        //    {
        //        result.Add(new FactoryImformation($@"FA-0{i + 1}", factorynName[i]));
        //    }
        //    return new ActionResponse<List<FactoryImformation>>
        //    {
        //        Data = result
        //    };
        //}

        /// <summary>
        /// 工廠與產線清單
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResponse<FactoryDefine> Get()
        {
            var devices = new List<Device>();
            var productionLines = new List<ProductionLine>();

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
                                string remark = SqlData["remark"].ToString().Trim();
                                if (!string.IsNullOrEmpty(remark))
                                {
                                    //if (remark[0] == '?')
                                    //{
                                    //    remark = remark.Substring(1); // 從第二個字元開始擷取字串
                                    //}
                                    devices.Add(new Device(remark, remark));
                                }

                            };
                        }
                    }
                }
            }
            #endregion


            //for (int i = 0; i < 10; i++)
            //{
            //    devices.Add(new Device($@"CNC-{i + 1,00}", $@"CNC-{i + 1,00}"));
            //    if (i == 4)
            //    {
            //        //productionLines.Add(new ProductionLine(@$"PRL-{i - 3,00}", @$"PRL-{i - 3,00}"));
            //        productionLines.Add(new ProductionLine(@$"全產線", @$"全產線"));
            //        productionLines[0].Devices = devices.ToList();
            //        devices.Clear();
            //    }

            //    if (i == 9)
            //    {
            //        //productionLines.Add(new ProductionLine(@$"PRL-02", @$"PRL-02"));
            //        productionLines.Add(new ProductionLine(@$"全產線", @$"全產線"));
            //        productionLines[1].Devices = devices.ToList();
            //        devices.Clear();
            //    }
            //}
            productionLines.Add(new ProductionLine(@$"全產線", @$"全產線"));
            productionLines[0].Devices = devices.ToList();
            var factorys = new List<Factory>();
            var facotorys = new Factory("安南新廠", "安南新廠");
            facotorys.ProductionLines = productionLines.ToList();
            factorys.Add(facotorys);
            var result = new FactoryDefine();
            result.Factorys = factorys;
            return new ActionResponse<FactoryDefine>
            {
                Data = result
            };
        }
        /// <summary>
        /// 取得特定產線名稱
        /// </summary>
        /// <param name="factory">廠區名稱 EX:FA-01</param>
        /// <returns></returns>
        [HttpGet("Productionlines/{factory}")]
        public ActionResponse<List<ProductionLineImformation>> GetProduction(string factory)
        {
            var result = new List<ProductionLineImformation>();
            var factorynName = new string[] { "WGAM", "WGCM", "WEA", "WTA", "WGPK" };
            for (int i = 0; i < 5; i++)
            {
                result.Add(new ProductionLineImformation($@"PRL-0{i}", factorynName[i]));
            }
            return new ActionResponse<List<ProductionLineImformation>>
            {
                Data = result
            };
        }

        /// <summary>
        /// 取得產線中所有的機台名稱
        /// </summary>
        /// <param name="prl">輸入廠區名稱與產線名稱</param>
        /// <returns></returns>
        [HttpPost("Machines")]
        public ActionResponse<List<MachineInformation>> GetMachine([FromBody] RequestProductionLine prl)
        {
            var result = new List<MachineInformation>();

            var status = new string[] { "RUN", "IDLE", "ALARM", "OFF" };

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
            #endregion

            //for (int i = 0; i < 10; i++)
            //{
            //    result.Add(new MachineInformation($@"CYY-{i + 1, 2:00}", status[i % 4], $@"CYY-{i + 1,2:00}"));
            //}
            foreach (var item in devices)
            {
                result.Add(new MachineInformation
                (
                    machineName: item.Remark,
                    status: !String.IsNullOrEmpty(item.MAKTX) ? "RUN" : "IDLE",
                    displayName: item.Remark
                ));
            }
            return new ActionResponse<List<MachineInformation>>
            {
                Data = result
            };
        }
    }
}
