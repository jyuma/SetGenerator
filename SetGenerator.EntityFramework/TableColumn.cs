//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SetGenerator.EntityFramework
{
    using System;
    using System.Collections.Generic;
    
    public partial class TableColumn
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TableColumn()
        {
            this.UserPreferenceTableColumns = new HashSet<UserPreferenceTableColumn>();
        }
    
        public int Id { get; set; }
        public int TableId { get; set; }
        public string Name { get; set; }
        public string Data { get; set; }
        public bool AlwaysVisible { get; set; }
        public int Sequence { get; set; }
    
        public virtual Table Table { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserPreferenceTableColumn> UserPreferenceTableColumns { get; set; }
    }
}
