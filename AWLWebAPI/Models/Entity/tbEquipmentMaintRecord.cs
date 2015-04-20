//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace AWLWebAPI.Models.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbEquipmentMaintRecord
    {
        public int RecordID { get; set; }
        public string DivisionID { get; set; }
        public string RecordType { get; set; }
        public string PlanID { get; set; }
        public string EquipmentID { get; set; }
        public string EquipmentName { get; set; }
        public string EquipmentModel { get; set; }
        public string UsedDepartment { get; set; }
        public string MaintPosition { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> FinishDate { get; set; }
        public Nullable<decimal> MaintTime { get; set; }
        public Nullable<decimal> StopTime { get; set; }
        public Nullable<decimal> LabourCost { get; set; }
        public Nullable<decimal> MaterialCost { get; set; }
        public Nullable<decimal> MaintCost { get; set; }
        public Nullable<decimal> OtherCose { get; set; }
        public Nullable<decimal> MaintMonth { get; set; }
        public string Remark { get; set; }
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> CreatDate { get; set; }
        public string ModifyBy { get; set; }
        public Nullable<System.DateTime> ModifyDate { get; set; }
        public string MainMaterial { get; set; }
        public string MaintUnit { get; set; }
    }
}
