using AttributeMatch;
using CefSharp;
using CefSharp.DevTools.FedCm;
using IdleAuto.Db;
using IdleAuto.Scripts.Controller;
using IdleAuto.Scripts.Model;
using IdleAuto.Scripts.Wrap;
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
            BroWindow window = await TabManager.Instance.TriggerAddBroToTap(account);
            EquipController equipController = new EquipController();
            //window.GetBro().ShowDevTools();

            //将挂机装备放入仓库
            await EquipToRepository(window, equipController, account);
            //盘点仓库装备
            await InventoryEquips(window, equipController, account);
            //遍历账户下角色修车
            foreach (var role in account.Roles)
            {
                //技能加点
                await AddSkillPoint(window, role);
                //自动更换装备
                await AutoEquip(window, equipController, account, role);
                //角色剩余属性点分配
                await AddAttrPoint(window, role);
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
        try
        {
            EquipController equipController = new EquipController();
            //跳转账户首页
            BroWindow window = await TabManager.Instance.TriggerAddBroToTap(account);
            //var bro = BroTabManager.Instance.GetBro(repairBroSeed);
            //bro.ShowDevTools();

            await EquipToRepository(window, equipController, account);
            await ClearRepository(window, equipController, account);

            MessageBox.Show($"清理装备完成");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"清理装备异常：{ex.Message}");
        }
    }
    public async Task ClearRoleEquips()
    {
        //try
        //{
        //    RoleModel role = AccountController.Instance.CurRole;
        //    UserModel account = AccountController.Instance.User;
        //    if (account != null && role != null)
        //    {
        //        EquipController equipController = new EquipController();
        //        //跳转账户首页
        //        int repairBroSeed = BroTabManager.Instance.GetFocusID();
        //        //var bro = BroTabManager.Instance.GetBro(repairBroSeed);
        //        //bro.ShowDevTools();

        //        await equipController.EquipsToRepository(repairBroSeed, account.AccountName, role);
        //        await ClearRepository(repairBroSeed, equipController, account);

        //        MessageBox.Show($"清理装备完成");
        //    }
        //    else
        //    {
        //        MessageBox.Show($"获取角色信息失败，无法清理！");
        //    }
        //}
        //catch (Exception ex)
        //{
        //    MessageBox.Show($"清理装备异常：{ex.Message}");
        //}
    }
    public async Task ClearRepository(BroWindow win, EquipController controller, UserModel account)
    {
        await controller.ClearRepository(win, account);
    }
    public async Task EquipToRepository(BroWindow win, EquipController controller, UserModel account)
    {
        await controller.EquipsToRepository(win, account);
    }
    public async Task InventoryEquips(BroWindow win, EquipController controller, UserModel account)
    {
        await controller.InventoryEquips(win, account, true);
    }
    public async Task AddSkillPoint(BroWindow win, RoleModel role)
    {
        await win.CharController.AddSkillPoints(win.GetBro(), role);
    }
    public async Task AutoEquip(BroWindow win, EquipController controller, UserModel account, RoleModel role)
    {
        await controller.AutoEquips(win, account, role);
    }
    public async Task AddAttrPoint(BroWindow win, RoleModel role)
    {
        P.Log($"开始跳转{role.RoleName}的角色详情页");
        var result = await win.LoadUrlWaitJsInit(IdleUrlHelper.RoleUrl(role.RoleId), "char");
        if (result.Success)
        {
            P.Log($"跳转{role.RoleName}的角色详情页完成");

            var response3 = await win.CallJs($@"_char.getSimpleAttribute();");
            if (response3.Success)
            {
                var baseAttr = response3.Result.ToObject<CharBaseAttributeModel>();
                ///满足装备需求属性后仍有属性点剩余，自动加到体力值
                if (baseAttr.Point > 0)
                {
                    baseAttr.VitAdd += baseAttr.Point;
                    baseAttr.Point = 0;
                    await Task.Delay(1000);
                    var response8 = await win.CallJsWaitReload($@"_char.attributeSave({role.RoleId},{baseAttr.ToLowerCamelCase()});", "char");
                    if (response8.Success)
                    {
                        P.Log($"{role.RoleName}的属性加点完成", emLogType.AutoEquip);
                    }
                }
            }
        }
    }
}
