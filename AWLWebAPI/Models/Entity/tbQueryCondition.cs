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
    
    public partial class tbQueryCondition
    {
        public int UniqueID { get; set; }
        public Nullable<int> QueryType { get; set; }
        public string QueryField { get; set; }
        public string FieldDescription { get; set; }
        public Nullable<short> DataType { get; set; }
        public Nullable<int> QueryValues { get; set; }
        public string QueryValuesFrom { get; set; }
        public Nullable<int> SeqNo { get; set; }
        public Nullable<int> Status { get; set; }
    }
}
