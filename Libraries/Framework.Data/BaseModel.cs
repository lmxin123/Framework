using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Framework.Common;
using Framework.Common.Extensions;

namespace Framework.Data
{
    public class ModelBase
    {
        protected const string DateFormetStr = "yyyy-MM-dd";
        protected const string DateTimeFormetStr = "yyyy-MM-dd HH:mm:ss";
        protected const string LengthErrMsg = "{0}长度必需在{1}个字符以内";
        protected const string RequiredErrMsg = "请输入{0}";
        protected const string RequiredSelectErrMsg = "请选择{0}";
        protected const string RangeLengthErrMsg = "{0}必需在{1}到{2}个字符以内";
    }
    /// <summary>
    /// 父级实体，建议新建实体都继承至此类
    /// </summary>
    public partial class BaseModel<TKey> : ModelBase
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        public virtual TKey ID { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Display(Name = "创建时间")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public virtual DateTime CreateDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 数据状态，默认为发布状态
        /// </summary>
        [Display(Name = "状态")]
        public virtual RecordStates RecordState { get; set; } = RecordStates.AuditPass;
        /// <summary>
        /// 操作员，一般存当前用户Id
        /// </summary>
        [StringLength(50)]
        [Display(Name = "操作员")]
        public virtual string Operator { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        [Display(Name = "备注")]
        public virtual string Remark { get; set; }
    }

    public partial class BaseModel<TKey>
    {
        /// <summary>
        /// 状态字符
        /// </summary>
        [NotMapped]
        public virtual string RecordStateDisplay
        {
            get
            {
                return RecordState.GetDisplayName();
            }
        }
        /// <summary>
        /// 某些序列化格式会乱
        /// </summary>
        [NotMapped]
        public virtual string CreateDateDisplay
        {
            get { return CreateDate == null ? string.Empty : CreateDate.ToString(DateTimeFormetStr); }
        }


    }
}
