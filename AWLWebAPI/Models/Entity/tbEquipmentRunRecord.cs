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
    
    public partial class tbEquipmentRunRecord
    {
        public int ID { get; set; }
        public string DivisionID { get; set; }
        public string Buildcode { get; set; }
        public string EquipmentID { get; set; }
        public string EquipmentName { get; set; }
        public string EquipmentModel { get; set; }
        public string UsedDepartment { get; set; }
        public string Patrolman { get; set; }
        public Nullable<System.DateTime> PatrolDate { get; set; }
        public string Equipmentstatus { get; set; }
        public string DisposeAdvise { get; set; }
        public string Remark { get; set; }
    }
}
