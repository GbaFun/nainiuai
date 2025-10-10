using FreeSql.DataAnnotations;
using IdleAuto.Scripts.Model;

namespace IdleApi.Model
{
    public class ErrorLog:IModel
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }
        public string AccountName { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public string Msg { get; set; }
    }
}
