namespace IdleApi.Service.Interface
{
    public interface IEquip
    {
        /// <summary>
        /// 将储藏箱的装备存库
        /// </summary>
        /// <returns></returns>
        Task SaveEquipInRepo();

        /// <summary>
        /// 将背包的装备存库
        /// </summary>
        /// <returns>范围特定装备列表</returns>
        Task<List<long>> SaveEquipInBag(int roleid);

        /// <summary>
        ///保存所有在包裹中的装备
        /// </summary>
        /// <returns></returns>
        Task SaveAllRolesEquipInBag();

        /// <summary>
        /// 保存所有装备数据 
        /// </summary>
        /// <returns></returns>
        Task SaveAll();

        /// <summary>
        /// 收菜
        /// </summary>
        /// <returns></returns>
        Task CollectRolesEquips();
    }
}
