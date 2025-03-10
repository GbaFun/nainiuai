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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

public class RepairManager : SingleManagerBase<RepairManager>
{
    /// <summary>
    /// 一键修车（单账号）
    /// 盘库、技能加点、自动更换装备、剩余属性点分配
    /// 修车过程不会将背包物品收拢，不会清仓
    /// </summary>
    public async Task AutoRepair(UserModel account)
    {
        try
        {
            //大号跳过自动修车逻辑，需要手动修车
            if (account.AccountName == "铁矿石")
            {
                MessageBox.Show($"大号跳过自动修车逻辑，请手动修！");
                return;
            }

            BroWindow window = await TabManager.Instance.TriggerAddBroToTap(account);
            EquipController equipController = new EquipController();
            //window.GetBro().ShowDevTools();

            //将挂机装备放入仓库
           // await EquipToRepository(window, equipController, account);
            //盘点仓库装备
           // await InventoryEquips(window, equipController, account);
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
            window.Close();
            MessageBox.Show($"自动修车完成");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"自动修车异常：{ex.Message}");
        }
    }

    /// <summary>
    /// 一键修车（全账号，凌晨自动任务用）
    /// 收菜、盘库、技能加点、自动更换装备、剩余属性点分配
    /// 修车过程不会清仓，如果收菜过程仓库满了，会跳过后续角色背包收菜
    /// </summary>
    public async Task AutoRepair()
    {
        try
        {
            foreach (var user in AccountCfg.Instance.Accounts)
            {
                //大号跳过自动修车逻辑，需要手动修车
                if (user.AccountName == "铁矿石")
                {
                    P.Log($"大号跳过自动修车逻辑，请手动修！", emLogType.Warning);
                    continue;
                }
                UserModel account = new UserModel(user);
                BroWindow window = await TabManager.Instance.TriggerAddBroToTap(account);
                EquipController equipController = new EquipController();
                //window.GetBro().ShowDevTools();

                //将挂机装备放入仓库
                await EquipToRepository(window, equipController, account, false);
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

                window.Close();
            }
            MessageBox.Show($"自动修车完成");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"自动修车异常：{ex.Message}");
        }
    }

    /// <summary>
    /// 一键清仓
    /// 收菜、清仓、盘库
    /// </summary>
    public async Task UpdateEquips(UserModel account)
    {
        try
        {
            BroWindow window = await TabManager.Instance.TriggerAddBroToTap(account);
            EquipController equipController = new EquipController();
            //window.GetBro().ShowDevTools();

            //将挂机装备放入仓库
            await EquipToRepository(window, equipController, account, true);
            //盘点仓库装备
            await InventoryEquips(window, equipController, account);

            window.Close();
            MessageBox.Show($"一键清仓完成");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"一键清仓异常：{ex.Message}");
        }
    }


    public async Task EquipToRepository(BroWindow win, EquipController controller, UserModel account, bool cleanWhenFull = false)
    {
        await controller.EquipsToRepository(win, account, cleanWhenFull);
    }
    public async Task InventoryEquips(BroWindow win, EquipController controller, UserModel account)
    {
        await controller.InventoryEquips(win, account, true);
    }
    public async Task AddSkillPoint(BroWindow win, RoleModel role)
    {
        var charControl = new CharacterController(win);
        await charControl.AddSkillPoints(win.GetBro(), role);
    }
    public async Task AutoEquip(BroWindow win, EquipController controller, UserModel account, RoleModel role)
    {
        await controller.AutoEquips(win, account, role);
    }
    public async Task AddAttrPoint(BroWindow win, RoleModel role)
    {
        await Task.Delay(1000);
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