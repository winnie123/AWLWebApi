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
    
    public partial class tbRegion
    {
        public int ID { get; set; }
        public Nullable<int> ParentID { get; set; }
        public string RegionCode { get; set; }
        public string RegionName { get; set; }
        public Nullable<short> Level { get; set; }
        public Nullable<int> Serial { get; set; }
        public string Remark { get; set; }
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public string ModifyBy { get; set; }
        public Nullable<System.DateTime> ModifyTime { get; set; }
        public Nullable<int> RegionType { get; set; }
        public string ProvCode { get; set; }
        public string ProvName { get; set; }
        public string CityCode { get; set; }
        public string CityName { get; set; }
    }
}
