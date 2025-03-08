using System.ComponentModel.DataAnnotations;

namespace BlogManagement.Common
{
    public class BaseModel
    {
        [Key]
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}
