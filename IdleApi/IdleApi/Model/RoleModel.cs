using FreeSql.DataAnnotations;
using IdleAuto.Db;
using IdleAuto.Scripts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RoleModel : IModel
{
    [Column(IsPrimary = true)]
    public int RoleId { get; set; }


    public string RoleName { get; set; }
    public string RoleInfo { get; set; }
    [Navigate(nameof(CharAttributeModel.RoleId))]
    public List<CharAttributeModel> Attribute { get; set; }

    public MapModel Map { get; set; }

    public int Level
    {
        get
        {
            var s = RoleInfo.Split(' ');
            return int.Parse(s[0].Replace("Lv", ""));
        }
    }
    public emJob Job
    {
        get
        {
            var s = RoleInfo.Split(' ');
            var job = s[1].Substring(2, 2);
            return (emJob)Enum.Parse(typeof(emJob), job);
        }
    }
    public emRace Race
    {
        get
        {
            var s = RoleInfo.Split(' ');
            var race = s[1].Substring(0, 2);
            return (emRace)Enum.Parse(typeof(emRace), race);
        }
    }
    public override bool Equals(object obj)
    {
        var t = obj as RoleModel;
        if (t == null) return false;
        return t.RoleId == this.RoleId;
    }
    public override int GetHashCode()
    {
        return this.RoleId;
    }

    /// <summary>
    /// 获取小组技能bd
    /// </summary>
    /// <returns></returns>
    public emSkillMode GetRoleSkillMode()
    {
        var role = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.RoleId == RoleId).First();
        if (role == null)
        {
            throw new Exception("请先初始化组队表");
        }
        return role.SkillMode;
    }

    public emSuitName GetRoleSuitName()
    {
        var role = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.RoleId == RoleId).First();
        if (role == null)
        {
            throw new Exception("请先初始化组队表");
        }
        return role.SuitName;
    }

    public List<GroupModel> GetGroup()
    {

        var role = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.RoleId == RoleId).First();
        if (role == null)
        {
            throw new Exception("请先初始化组队表");
        }
        var list = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.AccountName == role.AccountName && p.TeamIndex == role.TeamIndex).ToList();
        return list;
    }

    public RoleModel GetTeamMember(emJob job, List<RoleModel> roles)
    {
        var group = GetGroup();
        var m = group.Find(p => p.Job == job);
        int roleId = m.RoleId;
        return roles.Find(p => p.RoleId == roleId);

    }


}