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
    public class DeviceInfoController : BaseApiController
    {
        ConnectStr _ConnectStr = new ConnectStr();
        public DeviceInfoController()
        {

        }

        [HttpPost]
        public ActionResponse<OperationInfo> Post([FromBody] RequestFactory device)
        {
            Random rand = new Random();
            var tempinfo = new DeviceInfoTemp();
            
            #region 撈取各機台生產資料
            //取得工單資料
            var sqlStr = @$"SELECT
                            a.WIPEvent,
                            a.OrderID,
                            a.OPID,
                            c.OPLTXA1,
                            c.MAKTX,
                            g.Name,
                            d.CustomerInfo,
                            c.AssignDate,
                            a.OrderQTY,
                            a.QtyGood,
                            (CAST(a.QtyGood AS FLOAT) / CAST(a.OrderQTY AS FLOAT) * 100.0) AS ProductionProgress,
                            f.img
                        FROM
                            {_ConnectStr.APSDB}.[dbo].[WIP] AS a
                        LEFT JOIN
                            {_ConnectStr.APSDB}.[dbo].[Assignment] AS c ON a.OrderID = c.OrderID AND a.OPID = c.OPID AND a.WIPEvent in (1,2)
                        LEFT JOIN
                           {_ConnectStr.APSDB}.[dbo].OrderOverview AS d ON c.ERPOrderID = d.OrderID
                        INNER JOIN 
                            {_ConnectStr.MRPDB}.[dbo].[Part] as g ON c.maktx = g.number
                        RIGHT JOIN
                            {_ConnectStr.APSDB}.[dbo].[Device] AS f ON c.WorkGroup = f.remark
						LEFT JOIN
							{_ConnectStr.APSDB}.[dbo].[WipRegisterLog] AS e ON f.ID = e.DeviceID
                        WHERE
                            f.remark = '{device.DeviceName}'";



            #region Ver.1 語法1
            //var sqlStr = @$"SELECT
            //                a.WorkOrderID,
            //                a.OPID,
            //                c.OPLTXA1,
            //                c.MAKTX,
            //                d.CustomerInfo,
            //                c.AssignDate,
            //                e.OrderQTY,
            //                e.QtyGood,
            //                (CAST(e.QtyGood AS FLOAT) / CAST(e.OrderQTY AS FLOAT) * 100.0) AS ProductionProgress,
            //                b.img
            //            FROM
            //                {_ConnectStr.APSDB}.[dbo].[WipRegisterLog] AS a
            //            LEFT JOIN
            //                {_ConnectStr.APSDB}.[dbo].[Device] AS b ON a.DeviceID = b.ID
            //            LEFT JOIN
            //                {_ConnectStr.APSDB}.[dbo].[Assignment] AS c ON a.WorkOrderID = c.OrderID AND a.OPID = c.OPID
            //            LEFT JOIN
            //                {_ConnectStr.APSDB}.[dbo].OrderOverview AS d ON c.ERPOrderID = d.OrderID
            //            LEFT JOIN
            //                {_ConnectStr.APSDB}.[dbo].[WIP] AS e ON c.OrderID = e.OrderID AND c.OPID = e.OPID
            //            RIGHT JOIN
            //                {_ConnectStr.APSDB}.[dbo].[Device] AS f ON a.DeviceID = f.ID
            //            WHERE
            //                f.remark = '{device.DeviceName}'";
            #endregion
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
                                tempinfo.WIPEvent = String.IsNullOrEmpty(SqlData["WIPEvent"].ToString().Trim()) ? "-" : SqlData["WIPEvent"].ToString().Trim();
                                tempinfo.OrderNo = String.IsNullOrEmpty(SqlData["OrderID"].ToString().Trim()) ? "-" : SqlData["OrderID"].ToString().Trim();
                                tempinfo.OPNo = Convert.ToInt32(String.IsNullOrEmpty(SqlData["OPID"].ToString().Trim())?"00": SqlData["OPID"].ToString().Trim());
                                tempinfo.OPName = String.IsNullOrEmpty(SqlData["OPLTXA1"].ToString().Trim())? "-" : SqlData["OPLTXA1"].ToString().Trim();
                                tempinfo.ProductNo = String.IsNullOrEmpty(SqlData["Name"].ToString().Trim())? "-" : SqlData["Name"].ToString().Trim();
                                tempinfo.DueDate = !Convert.IsDBNull(SqlData["AssignDate"]) ? Convert.ToDateTime(SqlData["AssignDate"]).ToString("yyyy-MM-dd") : "-";
                                tempinfo.RequireCount = Convert.ToInt32(!Convert.IsDBNull(SqlData["OrderQTY"]) ? SqlData["OrderQTY"].ToString().Trim() : "0");
                                tempinfo.CurrentCount = Convert.ToInt32(!Convert.IsDBNull(SqlData["QtyGood"]) ? SqlData["QtyGood"].ToString().Trim() : "0");
                                tempinfo.CustomName = String.IsNullOrEmpty(SqlData["CustomerInfo"].ToString().Trim())? "-" : SqlData["CustomerInfo"].ToString().Trim();
                                tempinfo.ProductionProgress = Convert.ToDouble(!Convert.IsDBNull(SqlData["ProductionProgress"]) ? SqlData["ProductionProgress"].ToString() : "0.0");
                                tempinfo.DeviceImg = String.IsNullOrEmpty(SqlData["img"].ToString().Trim()) ? "default.jpg" : SqlData["img"].ToString().Trim();
                            };
                        }
                    }
                }
            }
            if (tempinfo.DeviceImg[0]=='?')
            {
                tempinfo.DeviceImg = "default.jpg";
            }

            #endregion
            if (!string.IsNullOrEmpty(tempinfo.DeviceImg) && tempinfo.ProductNo != "-" && tempinfo.WIPEvent=="1")
            {
                return new ActionResponse<OperationInfo>
                {
                    Data = new OperationInfo
                (
                    utilizationRate: Math.Round((rand.NextDouble()*0.3+0.7)*100,1),
                    status: "RUN",
                    productionProgress: tempinfo.ProductionProgress,
                    customName: tempinfo.CustomName.Split('/')[1],
                    deviceImg: "/images/device/"+ tempinfo.DeviceImg,
                    orderInfo: new OrderInformation(orderNo: tempinfo.OrderNo, oPNo: tempinfo.OPNo, opName: tempinfo.OPName,
                    productNo: tempinfo.ProductNo, requireCount: tempinfo.RequireCount, currentCount: tempinfo.CurrentCount, dueDate: tempinfo.DueDate, customerinfo:""))
                    
                };
            }
            else if(tempinfo.WIPEvent == "2")
            {
                return new ActionResponse<OperationInfo>
                {
                    Data = new OperationInfo
                (
                    utilizationRate: Math.Round((rand.NextDouble() * 0.3 + 0.7) * 100, 1),
                    status: "IDLE",
                    productionProgress: tempinfo.ProductionProgress,
                    customName: tempinfo.CustomName.Split('/')[1],
                    deviceImg: "/images/device/" + tempinfo.DeviceImg,
                    orderInfo: new OrderInformation(orderNo: tempinfo.OrderNo, oPNo: tempinfo.OPNo, opName: tempinfo.OPName,
                    productNo: tempinfo.ProductNo, requireCount: tempinfo.RequireCount, currentCount: tempinfo.CurrentCount, dueDate: tempinfo.DueDate, customerinfo: ""))

                };
            }
            else
            {
                return new ActionResponse<OperationInfo>
                {
                    Data = new OperationInfo
                (
                    utilizationRate: Math.Round((rand.NextDouble() * 0.4 + 0.6) * 100, 1),
                    status: "IDLE",
                    productionProgress: tempinfo.ProductionProgress,
                    customName: "-",
                    deviceImg: "/images/device/" + tempinfo.DeviceImg,
                    orderInfo: new OrderInformation(orderNo: "-", oPNo: 0, opName: "-",
                    productNo: "-", requireCount: 0, currentCount: 0, dueDate: "-", customerinfo: "-"))

                };
            }
            //else 
            //{
            //    return new ActionResponse<OperationInfo>();
            //}


            //return new ActionResponse<OperationInfo>
            //{
            //    Data = new OperationInfo
            //    (
            //        utilizationRate: 85.9d, 
            //        status: "RUN", 
            //        productionProgress: 67.8d, 
            //        customName: @"-",
            //        orderInfo: new OrderInformation(orderNo: $@"10411110002", oPNo: 66, opName: "車牙",
            //        productNo: $@"11110001", requireCount: 3000, currentCount:1500, dueDate: "2021-12-03"))
            //};
        }

    }
}
