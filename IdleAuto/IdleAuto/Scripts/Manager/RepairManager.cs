using AttributeMatch;
using CefSharp;
using CefSharp.DevTools.FedCm;
using IdleAuto.Db;
using IdleAuto.Scripts.Controller;
using IdleAuto.Scripts.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class RepairManager : SingleManagerBase<RepairManager>
{
    /// <summary>
    /// 开始自动修车
    /// </summary>
    /// <returns></returns>
    public async Task AutoRepair(UserModel account)
    {
        try
        {
            EquipController equipController = new EquipController();
            //跳转账户首页
            int repairBroSeed = await BroTabManager.Instance.TriggerAddTabPage(account.AccountName, IdleUrlHelper.HomeUrl(), "char");
            var bro = BroTabManager.Instance.GetBro(repairBroSeed);
            bro.ShowDevTools();

            //将挂机装备放入仓库
            await EquipToRepository(repairBroSeed, equipController, account);
            //盘点仓库装备
            await InventoryEquips(repairBroSeed, equipController, account);
            //遍历账户下角色修车
            foreach (var role in account.Roles)
            {
                //技能加点
                await AddSkillPoint(repairBroSeed, account.AccountName, role);
                //自动更换装备
                await AutoEquip(repairBroSeed, equipController, account, role);
                //角色剩余属性点分配
                await AddAttrPoint(repairBroSeed, account.AccountName, role);
            }

            MessageBox.Show($"自动修车完成");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"自动修车异常：{ex.Message}");
        }
    }

    public async Task ClearEquips(UserModel account)
    {
        EquipController equipController = new EquipController();
        //跳转账户首页
        int repairBroSeed = await BroTabManager.Instance.TriggerAddTabPage(account.AccountName, IdleUrlHelper.HomeUrl(), "char");
        var bro = BroTabManager.Instance.GetBro(repairBroSeed);
        bro.ShowDevTools();
        await ClearRepository(repairBroSeed, equipController, account);
    }

    public async Task ClearRepository(int broSeed, EquipController controller, UserModel account)
    {
        await controller.ClearRepository(broSeed, account);
    }
    public async Task EquipToRepository(int broSeed, EquipController controller, UserModel account)
    {
        await controller.EquipsToRepository(broSeed, account);
    }
    public async Task InventoryEquips(int broSeed, EquipController controller, UserModel account)
    {
        await controller.InventoryEquips(broSeed, account, true);
    }
    public async Task AddSkillPoint(int broSeed, string title, RoleModel role)
    {
        await CharacterController.Instance.AddSkillPoints(BroTabManager.Instance.GetBro(broSeed), role);
    }
    public async Task AutoEquip(int broSeed, EquipController controller, UserModel account, RoleModel role)
    {
        await controller.AutoEquips(broSeed, account, role);
    }
    public async Task AddAttrPoint(int broSeed, string title, RoleModel role)
    {
        await BroTabManager.Instance.TriggerLoadUrl(title, IdleUrlHelper.RoleUrl(role.RoleId), broSeed, "char");
        var response3 = await BroTabManager.Instance.TriggerCallJs(broSeed, $@"_char.getSimpleAttribute();");
        if (response3.Success)
        {
            var baseAttr = response3.Result.ToObject<CharBaseAttributeModel>();
            ///满足装备需求属性后仍有属性点剩余，自动加到体力值
            if (baseAttr.Point > 0)
            {
                baseAttr.VitAdd += baseAttr.Point;
                baseAttr.Point = 0;
                var response8 = await BroTabManager.Instance.TriggerCallJsWithReload(broSeed, $@"_char.attributeSave({baseAttr.ToLowerCamelCase()});", "char");
                if (response8.Success)
                {
                    P.Log($"{role.RoleName}的属性加点完成", emLogType.AutoEquip);
                }
            }
        }
    }
}

