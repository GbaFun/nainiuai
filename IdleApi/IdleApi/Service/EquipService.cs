using IdleApi.Model.Exceptions;
using IdleApi.Service.Interface;
using IdleApi.Utils;
using IdleApi.Wrap;
using IdleAuto.Db;
using System.Data;
using System.Security.Principal;

namespace IdleApi.Service
{
    public class EquipService : BaseService, IService, IEquip
    {
        public EquipService(BroWindow win) : base(win)
        {
        }

        public async Task CollectRolesEquips()
        {
            var win = GetWin();
            string content = "";
            
            foreach (var role in User.Roles)
            {
                if (win.CurrentAddress != IdleUrlHelper.EquipUrl(role.RoleId))
                {
                    content = await win.LoadUrlAsync(IdleUrlHelper.EquipUrl(role.RoleId));
                }
                var parser = new EquipParser(content);
                SaveCurEquips(parser, role);
                var equips=parser.GetPackageEquips();
                while(equips.Count != 0)
                {
                    //全收进包
                    var url = IdleUrlHelper.EquipStoreAllUrl();
                    content =await win.SubmitFormAsync(url);
                    parser= new EquipParser(content);
                    equips = parser.GetPackageEquips();
                }

            }
        }

        public async Task SaveAll()
        {
            if (User.AccountName.StartsWith("0"))
            {
                await SaveAllRolesEquipInBag();
            }
            else
            {
                await CollectRolesEquips();
            }
            await SaveEquipInRepo();
        }

        public async Task SaveAllRolesEquipInBag()
        {
            foreach (var role in User.Roles)
            {
                await Task.Delay(1000);
                await SaveEquipInBag(role.RoleId);
            }
        }

        /// <summary>
        /// 保存当前穿戴装备
        /// </summary>
        /// <param name="roleid"></param>
        /// <returns></returns>
        public void SaveCurEquips(EquipParser parser, RoleModel role)
        {

            var curEquips = parser.GetCurEquips();
            var list = curEquips.Values.ToList();
            list.ForEach(p =>
            {
                p.SetAccountInfo(User, role);
            });
            DbUtil.InsertOrUpdate(list);

        }

        public async Task SaveEquipInBag(int roleid)
        {
            Dictionary<long, EquipModel> repositoryEquips = new Dictionary<long, EquipModel>();
            var win = GetWin();
            var role = win.User.Roles.Find(p => p.RoleId == roleid);
            string content = "";
            if (win.CurrentAddress != IdleUrlHelper.EquipUrl(role.RoleId))
            {
                content = await win.LoadUrlAsync(IdleUrlHelper.EquipUrl(role.RoleId));
            }
            int page = 0;
            bool canJumpNextPage = false;
            do
            {
                canJumpNextPage = false;
                EquipParser parser = new EquipParser(content);
                var equips = parser.GetPackageEquips();
                SaveCurEquips(parser, role);

                foreach (var item in equips)
                {
                    EquipModel equip = item.Value;
                    // equip.Category = CategoryUtil.GetCategory(equip.EquipBaseName);
                    equip.SetAccountInfo(win.User, role);
                    if (!repositoryEquips.ContainsKey(item.Key))
                    {
                        repositoryEquips.Add(item.Key, item.Value);
                    }

                }
                var nextUrl = parser.GetBagNextPageUrl();

                if (nextUrl != null)
                {
                    canJumpNextPage = true;
                    page++;
                    content = await win.LoadUrlAsync(IdleUrlHelper.EquipUrl(role.RoleId, page, 0));
                }
                DbUtil.InsertOrUpdate(repositoryEquips.Values.ToList());


            } while (canJumpNextPage);
        }

        /// <summary>
        /// 将仓库数据存表
        /// </summary>
        /// <returns></returns>
        public async Task SaveEquipInRepo()
        {
            Dictionary<long, EquipModel> repositoryEquips = new Dictionary<long, EquipModel>();
            var win = GetWin();
            var role = win.User.FirstRole;
            string content = "";
            if (win.CurrentAddress != IdleUrlHelper.EquipUrl(role.RoleId))
            {
                content = await win.LoadUrlAsync(IdleUrlHelper.EquipUrl(role.RoleId));
            }
            int page = 0;
            bool canJumpNextPage = false;
            do
            {
                canJumpNextPage = false;
                EquipParser parser = new EquipParser(content);
                var equips = parser.GetRepositoryEquips();


                foreach (var item in equips)
                {
                    EquipModel equip = item.Value;
                    // equip.Category = CategoryUtil.GetCategory(equip.EquipBaseName);
                    equip.SetAccountInfo(win.User);
                    if (!repositoryEquips.ContainsKey(item.Key))
                    {
                        item.Value.EquipStatus = emEquipStatus.Repo;
                        repositoryEquips.Add(item.Key, item.Value);
                    }

                }
                var nextBoxUrl = parser.GetRepoNextPageUrl();

                if (nextBoxUrl != null)
                {
                    canJumpNextPage = true;
                    page++;
                    content = await win.LoadUrlAsync(IdleUrlHelper.EquipUrl(role.RoleId, 0, page));
                }
                DbUtil.InsertOrUpdate(repositoryEquips.Values.ToList());


            } while (canJumpNextPage);
        }
    }
}
