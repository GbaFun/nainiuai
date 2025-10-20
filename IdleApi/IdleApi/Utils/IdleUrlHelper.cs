using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class IdleUrlHelper
{
    public const string Idle = "https://www.idleinfinity.cn";
    public const string Home = "Home/Index";
    public const string Map = "Map/Detail";
    public const string Role = "Character/Detail";
    public const string Equip = "Equipment/Query";
    public const string EquipStoreAll = "Equipment/EquipStoreAll";
    public const string EquipStore= "Equipment/EquipStore";
    public const string Ah = "Auction/Query";
    public const string Inlay = "Equipment/Inlay";
    public const string Material = "Equipment/Material";
    private const string Notice = "Notice/Query";
    private const string NoticeYes = "Notice/NoticeYes";
    public const string Reform = "Equipment/Reform";
    private const string m_roleArg = "id=";
    public static string HomeUrl()
    {
        return $"{Idle}/{Home}";
    }

    public static string ReformUrl(int roleId, long equipId)
    {
        return $"{Idle}/{Reform}?{m_roleArg}{roleId}&eid={equipId}";

    }
    public static string RoleUrl(int roleId)
    {
        return $"{Idle}/{Role}?{m_roleArg}{roleId}";
    }
    public static string MapUrl(int roleId)
    {
        return $"{Idle}/{Map}?{m_roleArg}{roleId}";
    }
    public static string EquipUrl(int roleId)
    {
        return $"{Idle}/{Equip}?{m_roleArg}{roleId}";
    }
    public static string EquipUrl(int roleId, int bagpi, int boxpi)
    {
        return $"{Idle}/{Equip}?{m_roleArg}{roleId}&pt2=&pi2={boxpi}&et2=&pi={bagpi}&pt=&et=&aid=";
    }

    public static string EquipStoreAllUrl()
    {
        return $"{Idle}/{EquipStoreAll}";
    }
    public static string EquipStoreUrl()
    {
        return $"{Idle}/{EquipStore}";
    }
    public static string AhUrl(int roleId)
    {
        return $"{Idle}/{Ah}?{m_roleArg}{roleId}";
    }
    public static string MaterialUrl(int roleId)
    {
        return $"{Idle}/{Material}?{m_roleArg}{roleId}";
    }

    public static string InlayUrl(int roleid, long eid)
    {
        return $"{Idle}/{Inlay}?id={roleid}&eid={eid}&pi=0&pt=&et=&pi2=0&pt2=&et2=&aid=";
    }
    public static string NoticeUrl(int page)
    {
        if (page == -1)
        {
            return $"{Idle}/{Notice}";
        }
        return $"{Idle}/{Notice}?pi={page}";
    }
    public static string NoticeYesUrl()
    {
        
        return $"{Idle}/{NoticeYes}";
    }
}
