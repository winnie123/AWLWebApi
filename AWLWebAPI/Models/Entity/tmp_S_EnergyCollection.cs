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
    
    public partial class tmp_S_EnergyCollection
    {
        public int ID { get; set; }
        public string DivisionID { get; set; }
        public string BlockID { get; set; }
        public Nullable<System.DateTime> CollectionTime { get; set; }
        public string EquipmentID { get; set; }
        public string EquipmentSort { get; set; }
        public string EnergyCode { get; set; }
        public Nullable<decimal> Quantity { get; set; }
        public string Active { get; set; }
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public string ModifyBy { get; set; }
        public Nullable<System.DateTime> ModifyTime { get; set; }
        public Nullable<decimal> Qty { get; set; }
        public string IsMain { get; set; }
        public string UnMain { get; set; }
        public string SecondCollected { get; set; }
    }
}
